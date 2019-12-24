using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    [ActorEntity]
    public struct ActorScoreTag : IComponentData
    {
    }

    [ServerNetworkEntity]
    [ClientPlayerEntity]
    public struct PlayerScore : IComponentData
    {
        public short value;
    }

    [ServerNetworkEntity]
    [ServerAutoClear]
    public struct PlayerKillList : IBufferElementData
    {
        public Entity targetPlayerEntity;   //杀死了那个玩家
        //public Entity actorEntity         //杀死了那个玩家的角色  角色已经删除了  存着entity也没有  
        public short score;
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct PlayerScoreNetMessages : IBufferElementData
    {
        public int playerId;
        public short score;
    }
    public struct PlayerScoreSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.PlayerScore;

        public int playerId;
        public short score;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new PlayerScoreNetMessages { playerId = playerId, score = score }, commandBuffer);
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerScoreServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        NativeArray<short> _scores;
        public int scoreCount => Enabled ? _scores.Length : 0;
        public void setScores(short[] scores)
        {
            if (Enabled == true)
            {
                Debug.LogError("Enabled == true");
                return;
            }


            _scores = new NativeArray<short>(scores, Allocator.Persistent);
            Enabled = true;

            Debug.Assert(_scores.Length > 0, "_scores.Length > 0");
        }
        protected override void OnDestroy()
        {
            _scores.Dispose();

            outs.Dispose();
            ins.Dispose();
        }
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();

            enterGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerEnterGameMessage>()/*, ComponentType.ReadOnly<Player>() */},
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });

            Enabled = false;
        }
        EntityQuery enterGameQuery;


        [BurstCompile]
        //[RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct AllPlayerScore2EnterGamePlayerJob : IJobForEachWithEntity<Player, PlayerScore>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> enterGamePlayerEntitys;
            public BufferFromEntity<NetworkUnreliableOutBuffer> enterGameOutBuffers;
            public void Execute(Entity playerEntity, int index, [ReadOnly] ref Player player, [ReadOnly]ref PlayerScore playerScore)
            {
                if (playerScore.value == 0)
                    return;

                for (int i = 0; i < enterGamePlayerEntitys.Length; ++i)
                {
                    var enterGamePlayerEntity = enterGamePlayerEntitys[i];

                    var enterGameOutBuffer = enterGameOutBuffers[enterGamePlayerEntity];

                    //
                    var s = new PlayerScoreSerialize { playerId = player.id, score = playerScore.value };
                    s._DoSerialize(enterGameOutBuffer);
                }
            }
        }


        [BurstCompile]
        [RequireComponentTag(typeof(ActorScoreTag), typeof(OnDestroyMessage))]
        struct Score2PlayerJob : IJobForEach_BC<KillersOnActorDeath, ActorOwner>
        {
            [ReadOnly] public NativeArray<short> scores;
            public BufferFromEntity<PlayerKillList> playerKillBufferFromEntity;
            public void Execute([ReadOnly]DynamicBuffer<KillersOnActorDeath> killersOnActorDeath, [ReadOnly]ref ActorOwner actorOwner)
            {
                if (killersOnActorDeath.Length <= 0)//离线时自爆
                    return;

                var sIndex = scores.Length - 1;
                for (int i = killersOnActorDeath.Length - 1; i >= 0 && sIndex >= 0; --i, --sIndex)
                {
                    var killer = killersOnActorDeath[i];

                    addScore(killer, actorOwner, sIndex);
                }

                //剩下的分数归杀死角色的玩家
                for (; sIndex >= 0; --sIndex)
                {
                    var killer = killersOnActorDeath[killersOnActorDeath.Length - 1];

                    addScore(killer, actorOwner, sIndex);
                }
            }

            void addScore(in KillersOnActorDeath killer, in ActorOwner actorOwner, int sIndex)
            {
                if (killer.playerEntity != Entity.Null)//可能是场景里创建的陷阱
                {
                    if (playerKillBufferFromEntity.Exists(killer.playerEntity))
                    {
                        playerKillBufferFromEntity[killer.playerEntity].Add(new PlayerKillList { targetPlayerEntity = actorOwner.playerEntity, score = scores[sIndex] });
                    }
                }
            }
        }

        [BurstCompile]
        struct ScoreCalculateJob : IJobForEachWithEntity_EBCC<PlayerKillList, Player, PlayerScore>
        {
            [ReadOnly] public ComponentDataFromEntity<PlayerTeam> playerTeamFromEntity;
            public NativeQueue<PlayerScoreSerialize>.ParallelWriter outs;
            public void Execute(Entity playerEntity, int index, [ChangedFilter, ReadOnly]DynamicBuffer<PlayerKillList> playerKillList, [ReadOnly]ref Player player, ref PlayerScore playerScore)
            {
                if (playerKillList.Length > 0)
                {
                    for (var i = 0; i < playerKillList.Length; ++i)
                    {
                        var playerKill = playerKillList[i];

                        var myTeam = playerTeamFromEntity[playerEntity].value;
                        var targetTeam = 0;
                        if (playerKill.targetPlayerEntity != default && playerTeamFromEntity.Exists(playerKill.targetPlayerEntity)/*可能断线了*/)
                            targetTeam = playerTeamFromEntity[playerKill.targetPlayerEntity].value;

                        if (myTeam == 0 || myTeam != targetTeam)
                            playerScore.value += playerKill.score;
                        else
                            playerScore.value -= playerKill.score;
                    }

                    outs.Enqueue(new PlayerScoreSerialize { playerId = player.id, score = playerScore.value });
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SyncScoreJob : IJobForEach_B<NetworkUnreliableOutBuffer>
        {
            [ReadOnly] public NativeArray<PlayerScoreSerialize> ins;
            public void Execute(DynamicBuffer<NetworkUnreliableOutBuffer> outBuffer)
            {
                for (var i = 0; i < ins.Length; ++i)
                {
                    var s = ins[i];
                    s._DoSerialize(outBuffer);
                }
            }
        }

        NativeQueue<PlayerScoreSerialize> outs = new NativeQueue<PlayerScoreSerialize>(Allocator.Persistent);
        NativeList<PlayerScoreSerialize> ins = new NativeList<PlayerScoreSerialize>(Allocator.Persistent);
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //
            if (enterGameQuery.IsEmptyIgnoreFilter == false)
            {
                EntityManager.AddComponent<PlayerScore>(enterGameQuery);
                EntityManager.AddComponent<PlayerKillList>(enterGameQuery);

                var enterGamePlayerEntitys = enterGameQuery.ToEntityArray(Allocator.TempJob, out var arrayJob);

                inputDeps = new AllPlayerScore2EnterGamePlayerJob
                {
                    enterGamePlayerEntitys = enterGamePlayerEntitys,
                    enterGameOutBuffers = GetBufferFromEntity<NetworkUnreliableOutBuffer>(),
                }
                .ScheduleSingle(this, JobHandle.CombineDependencies(inputDeps, arrayJob));
            }


            //
            inputDeps = new Score2PlayerJob
            {
                scores = _scores,
                playerKillBufferFromEntity = GetBufferFromEntity<PlayerKillList>(),
            }
            .ScheduleSingle(this, inputDeps);


            outs.Clear();
            ins.Clear();
            inputDeps.Complete();//ChangedFilter需要inputDeps.Complete()
            inputDeps = new ScoreCalculateJob
            {
                playerTeamFromEntity = GetComponentDataFromEntity<PlayerTeam>(true),
                outs = outs.AsParallelWriter(),
            }
            .Schedule(this, inputDeps);

            inputDeps = outs.ToListJob(ref ins, inputDeps);

            inputDeps = new SyncScoreJob
            {
                ins = ins.AsDeferredJobArray(),
            }
            .Schedule(this, inputDeps);

            return inputDeps;
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerScoreClientSystem : ComponentSystem
    {
        PlayerClientSystem playerClientSystem;
        protected override void OnCreate()
        {
            playerClientSystem = World.GetExistingSystem<PlayerClientSystem>();
        }


        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<NetworkConnection, NetworkConnectedMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddComponent<PlayerScoreNetMessages>(entity);
                });

            Entities
                .WithAllReadOnly<Player>()
                .WithNone<PlayerScore>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddComponent<PlayerScore>(entity);
                });



            using (var playerScoreNetMessages = Entities
                .WithAllReadOnly<NetworkConnection, PlayerScoreNetMessages>()
                .WithNone<NetworkDisconnectedMessage>()
                .AllBufferElementToList<PlayerScoreNetMessages>(Allocator.Temp))
            {
                for (int i = 0; i < playerScoreNetMessages.Length; ++i)
                {
                    var playerScoreNetMessage = playerScoreNetMessages[i];
                    if (playerClientSystem.playerEntityFromPlayerId.TryGetValue(playerScoreNetMessage.playerId, out Entity playerEntity) == false)
                    {
                        Debug.LogError($"{World.Name} => playerEntityFromPlayerId.TryGetValue(playerScoreNetMessage.playerId={playerScoreNetMessage.playerId}, out Entity playerEntity) == false");
                        continue;
                    }


                    //
                    var playerScore = new PlayerScore { value = playerScoreNetMessage.score };
                    EntityManager.SetComponentData(playerEntity, playerScore);
                }
            }
        }
    }
}