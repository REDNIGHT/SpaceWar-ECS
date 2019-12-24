using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    //
    public struct KillersOnActorDeath : IBufferElementData
    {
        public Entity playerEntity; //哪个玩家释放的
        //public Entity actor;      //哪个角色释放的
        public int type;            //那个技能释放的
    }


    //
    [ClientAutoClear]
    public struct KillInfoNetMessage : IBufferElementData
    {
        public const int MaxKillerCount = 3;

        public unsafe fixed int killerIds[MaxKillerCount];
        public int targetId;
    }
    public struct KillInfoSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.KillInfo;

        public KillInfoNetMessage killInfoNetMessage;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, killInfoNetMessage, commandBuffer);
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class KillServerSystem : JobComponentSystem
    {
        NativeQueue<KillInfoSerialize> outs = new NativeQueue<KillInfoSerialize>(Allocator.Persistent);
        NativeList<KillInfoSerialize> ins = new NativeList<KillInfoSerialize>(Allocator.Persistent);

        protected override void OnDestroy()
        {
            outs.Dispose();
            ins.Dispose();
        }


        PlayerScoreServerSystem playerScoreServerSystem;
        protected void OnInit(Transform root)
        {
            playerScoreServerSystem = World.GetExistingSystem<PlayerScoreServerSystem>();
        }

        [BurstCompile]
        //[RequireComponentTag(typeof(ActorDeathByKillers))]
        struct KillerCountJob : IJobForEach_B<KillersOnActorDeath>
        {
            public int actorDeathByKillerCount;
            public void Execute([ChangedFilter]DynamicBuffer<KillersOnActorDeath> actorDeathByKillers)
            {
                if (actorDeathByKillers.Length > actorDeathByKillerCount)
                {
                    actorDeathByKillers.RemoveRange(0, actorDeathByKillers.Length - actorDeathByKillerCount);
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(ActorScoreTag), typeof(OnDestroyMessage))]
        struct KillInfoSerializeJob : IJobForEach_BC<KillersOnActorDeath, ActorOwner>
        {
            public NativeQueue<KillInfoSerialize>.ParallelWriter outs;
            public unsafe void Execute([ReadOnly]DynamicBuffer<KillersOnActorDeath> killersOnActorDeath, [ReadOnly]ref ActorOwner actorOwner)
            {
                var killInfoSerialize = new KillInfoSerialize();
                killInfoSerialize.killInfoNetMessage.targetId = actorOwner.playerId;

                for (int i = killersOnActorDeath.Length - 1, j = 0; i >= 0 && j < KillInfoNetMessage.MaxKillerCount; --i, ++j)
                {
                    var killer = killersOnActorDeath[i];

                    killInfoSerialize.killInfoNetMessage.killerIds[j] = killer.playerEntity.Index;
                }

                outs.Enqueue(killInfoSerialize);
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SyncKillInfoJob : IJobForEach_B<NetworkUnreliableOutBuffer>
        {
            [ReadOnly] public NativeArray<KillInfoSerialize> ins;
            public void Execute(DynamicBuffer<NetworkUnreliableOutBuffer> outBuffer)
            {
                for (var i = 0; i < ins.Length; ++i)
                {
                    var s = ins[i];
                    s._DoSerialize(outBuffer);
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //
            inputDeps = new KillerCountJob
            {
                actorDeathByKillerCount = playerScoreServerSystem.scoreCount,
            }
            .Schedule(this, inputDeps);


            outs.Clear();
            ins.Clear();
            inputDeps = new KillInfoSerializeJob
            {
                outs = outs.AsParallelWriter(),
            }
            .Schedule(this, inputDeps);

            inputDeps = outs.ToListJob(ref ins, inputDeps);

            inputDeps = new SyncKillInfoJob
            {
                ins = ins.AsDeferredJobArray(),
            }
            .Schedule(this, inputDeps);

            return inputDeps;
        }
    }


    public abstract class KillInfosPanel : MonoBehaviour
    {
        public abstract void pushKillInfo(in string targetName, in string[] killerNames);
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class KillClientSystem : ComponentSystem
    {
        PlayerClientSystem playerClientSystem;
        KillInfosPanel killInfosPanel;
        protected void OnInit(Transform root)
        {
            playerClientSystem = World.GetExistingSystem<PlayerClientSystem>();

            killInfosPanel = GameObject.FindObjectOfType<KillInfosPanel>();
        }


        protected unsafe override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<NetworkConnection, NetworkConnectedMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddComponent<KillInfoNetMessage>(entity);
                });


            Entities
                .WithAllReadOnly<NetworkConnection, KillInfoNetMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((DynamicBuffer<KillInfoNetMessage> killInfoNetMessages) =>
                {
                    for (int i = 0; i < killInfoNetMessages.Length; ++i)
                    {
                        var killInfoNetMessage = killInfoNetMessages[i];

                        var targetName = "npc";

                        getName(killInfoNetMessage.targetId, ref targetName);


                        var killerNames = new string[KillInfoNetMessage.MaxKillerCount];
                        for (int j = 0; j < KillInfoNetMessage.MaxKillerCount; ++j)
                        {
                            var killerId = killInfoNetMessage.killerIds[j];

                            getName(killerId, ref killerNames[i]);
                        }

                        killInfosPanel.pushKillInfo(targetName, killerNames);
                        //Debug.Log($"targetName={targetName}  killerNames[0]={killerNames[0]}");
                    }
                });
        }

        void getName(int playerId, ref string name)
        {
            if (playerId <= 0)
                return;

            if (playerClientSystem.playerEntityFromPlayerId.TryGetValue(playerId, out Entity playerEntity) == false)
            {
                Debug.LogWarning($"{World.Name} => playerEntityFromPlayerId.TryGetValue(playerId={playerId}, out Entity playerEntity) == false");
                name = "off line player";
            }
            else
            {
                name = EntityManager.GetComponentData<PlayerName>(playerEntity).value.ToString();
            }
        }
    }
}
