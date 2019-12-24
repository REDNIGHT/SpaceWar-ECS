using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    //收到PlayerEnterGameMessage后会倒计时 完事后才会开始游戏和创建角色
    //角色死亡后也会进入倒计时


    [ServerNetworkEntity]
    [ClientPlayerEntity]
    public struct PlayerGameReady : IComponentData
    {
        public float countdown;
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct PlayerGameReadyNetMessage : IComponentData
    {
        public float countdown;
    }
    public struct PlayerGameReadySerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.PlayerGameReady;

        public float countdown;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.AddComponent(index, entity, new PlayerGameReadyNetMessage { countdown = countdown });
        }
    }


    [ServerNetworkEntity]
    [ClientPlayerEntity]
    [AutoClear]
    public struct PlayerGameStartNetMessage : IComponentData
    {
    }
    public struct PlayerGameStartSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.PlayerGameStart;


        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.AddComponent(index, entity, new PlayerGameStartNetMessage { });
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerGameReadyServerSystem : JobComponentSystem
    {
        public float gameReadyTime = 1f;
        protected override void OnDestroy()
        {
        }
        PlayerServerSystem playerServerSystem;
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            enterGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerEnterGameMessage>()/*, ComponentType.ReadOnly<Player>() */},
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });

            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }
        EntityQuery enterGameQuery;


        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage), typeof(PlayerGameStartNetMessage))]
        struct FindGameReadyPlayerJob : IJobForEach<PlayerActorArray, PlayerGameReady>
        {
            public float gameReadyTime;
            public void Execute([ChangedFilter, ReadOnly]ref PlayerActorArray playerActorArray, ref PlayerGameReady player_GameReady)
            {
                if (player_GameReady.countdown <= 0)
                {
                    if (playerActorArray.mainActorEntity != Entity.Null)
                    {
                        return;
                    }

                    //
                    player_GameReady.countdown = gameReadyTime;
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct GameReadyCountdownJob : IJobForEachWithEntity_EBC<NetworkReliableOutBuffer, PlayerGameReady>
        {
            public float gameReadyTime;
            public float fixedDeltaTime;

            public ComponentType PlayerGameStartNetMessage;

            public EntityCommandBuffer.Concurrent endCommandBuffer;

            public void Execute(Entity playerEntity, int index, DynamicBuffer<NetworkReliableOutBuffer> outBuffer, ref PlayerGameReady player_GameReady)
            {
                if (player_GameReady.countdown == gameReadyTime)
                {
                    var s = new PlayerGameReadySerialize { countdown = player_GameReady.countdown };
                    s._DoSerialize(outBuffer);
                }


                if (player_GameReady.countdown > 0f)
                {
                    player_GameReady.countdown -= fixedDeltaTime;

                    if (player_GameReady.countdown <= 0)
                    {
                        var s = new PlayerGameStartSerialize { };
                        s._DoSerialize(outBuffer);

                        endCommandBuffer.AddComponent(index, playerEntity, PlayerGameStartNetMessage);
                    }
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //
            if (enterGameQuery.IsEmptyIgnoreFilter == false)
            {
                //var endCommandBuffer = endBarrier.CreateCommandBuffer();
                using (var enterGamePlayerEntitys = enterGameQuery.ToEntityArray(Allocator.TempJob))
                {
                    for (var i = 0; i < enterGamePlayerEntitys.Length; ++i)
                    {
                        EntityManager.AddComponentData(enterGamePlayerEntitys[i], new PlayerGameReady { countdown = gameReadyTime });
                        //endCommandBuffer.AddComponent(enterGamePlayerEntitys[i], new PlayerGameReady { countdown = gameReadyTime });
                    }
                }
            }


            //
            inputDeps = new FindGameReadyPlayerJob
            {
                gameReadyTime = gameReadyTime,
            }
            .Schedule(this, inputDeps);


            //
            inputDeps = new GameReadyCountdownJob
            {
                gameReadyTime = gameReadyTime,
                fixedDeltaTime = Time.fixedDeltaTime,

                PlayerGameStartNetMessage = typeof(PlayerGameStartNetMessage),

                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerGameReadyClientSystem : ComponentSystem
    {
        public event System.Action<float> gameReady;
        public event System.Action gameStart;

        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<NetworkConnection, PlayerGameReadyNetMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((ref PlayerGameReadyNetMessage gameReadyNetMessage) =>
                {
                    gameReady(gameReadyNetMessage.countdown);
                });

            Entities
                .WithAllReadOnly<NetworkConnection, PlayerGameStartNetMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((ref PlayerGameStartNetMessage gameStartMessage) =>
                {
                    gameStart();
                });
        }
    }

}