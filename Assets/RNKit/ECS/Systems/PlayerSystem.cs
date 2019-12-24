using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    //NetworkId就是PlayerId
    //客户端只有一个NetworkId 但是会有所有的PlayerId
    //在服务器NetworkId和PlayerId是放在同一个entity


    //收到PlayerEnterGameMessage后才会有Player
    [ServerNetworkEntity]
    [ClientPlayerEntity]
    public struct Player : IComponentData
    {
        public int id;
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct PlayerCreateNetMessages : IBufferElementData
    {
        public int id;
    }
    public struct PlayerCreateSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.PlayerCreate;

        public int value;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new PlayerCreateNetMessages { id = value }, commandBuffer);

            //Debug.LogWarning($"PlayerCreateSerialize id={value}");
        }
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct PlayerDestroyNetMessages : IBufferElementData
    {
        public int value;
    }
    public struct PlayerDestroySerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.PlayerDestroy;

        public int value;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new PlayerDestroyNetMessages { value = value }, commandBuffer);
        }
    }



    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct EnterGamePlayer2AllPlayerJob : IJobForEachWithEntity_EB<NetworkReliableOutBuffer>
        {
            [ReadOnly] public NativeArray<Entity> enterGamePlayerEntitys;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Player> enterGamePlayers;
            public void Execute(Entity playerEntity, int index, DynamicBuffer<NetworkReliableOutBuffer> outBuffer)
            {
                //排除刚进入游戏的playerEntity
                for (int i = 0; i < enterGamePlayerEntitys.Length; ++i)
                {
                    if (playerEntity == enterGamePlayerEntitys[i])
                        return;
                }

                for (int i = 0; i < enterGamePlayerEntitys.Length; ++i)
                {
                    //Debug.LogWarning($"EnterGamePlayer2AllClientsJob  enterGamePlayers[i].id={enterGamePlayers[i].id}");
                    var s = new PlayerCreateSerialize { value = enterGamePlayers[i].id };
                    s._DoSerialize(outBuffer);
                }
            }
        }

        [BurstCompile]
        //[RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct AllPlayer2EnterGamePlayerJob : IJobForEach<Player>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> enterGamePlayerEntitys;
            public BufferFromEntity<NetworkReliableOutBuffer> enterGameOutBuffers;
            public void Execute([ReadOnly]ref Player player)
            {
                //Debug.LogWarning($"AllPlayer2EnterGameClientJob  enterGamePlayerEntitys.Length={enterGamePlayerEntitys.Length}  to playerEntity={playerEntity.Index}");
                for (int i = 0; i < enterGamePlayerEntitys.Length; ++i)
                {
                    var enterGamePlayerEntity = enterGamePlayerEntitys[i];
                    var outBuffer = enterGameOutBuffers[enterGamePlayerEntity];

                    //
                    //Debug.LogWarning($"AllPlayer2EnterGameClientJob  player.id={player.id}");
                    var s = new PlayerCreateSerialize { value = player.id };
                    s._DoSerialize(outBuffer);
                }
            }
        }

        [BurstCompile]
        struct PlayerDestroy2AllPlayerJob : IJobForEach_B<NetworkReliableOutBuffer>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Player> quitGamePlayers;
            public void Execute(DynamicBuffer<NetworkReliableOutBuffer> outBuffer)
            {
                for (int i = 0; i < quitGamePlayers.Length; ++i)
                {
                    var s = new PlayerDestroySerialize { value = quitGamePlayers[i].id };
                    s._DoSerialize(outBuffer);
                }
            }
        }

        protected override void OnDestroy()
        {
        }
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();

            enterGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerEnterGameMessage>(), ComponentType.ReadOnly<NetworkId>() },
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });
            quitGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<Player>(), ComponentType.ReadOnly<NetworkDisconnectedMessage>() },
                //Any = new ComponentType[] { ComponentType.ReadOnly<NetworkStreamDisconnectedMessage>() }
            });


            //Player.id 就是Entity.Index, Player.id是不能为0的, 但是第一个创建的Entity的Index就是0(Entity.Version=1)
            //所以这里先创建一个 避免出现Player.id=0
            var NullEntity = EntityManager.CreateEntity();
#if UNITY_EDITOR
            EntityManager.SetName(NullEntity, "NullEntity");
#endif
        }
        EntityQuery enterGameQuery;
        EntityQuery quitGameQuery;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //EnterGameMessage
            if (enterGameQuery.IsEmptyIgnoreFilter == false)
            {
                var enterGamePlayerEntitys = enterGameQuery.ToEntityArray(Allocator.TempJob, out var arrayJobA);
                using (var networkIds = enterGameQuery.ToComponentDataArray<NetworkId>(Allocator.TempJob, out var arrayJobB))
                {
                    var enterGamePlayers = new NativeArray<Player>(enterGamePlayerEntitys.Length, Allocator.TempJob);

                    //
                    JobHandle.CompleteAll(ref arrayJobA, ref arrayJobB);
                    for (var i = 0; i < enterGamePlayerEntitys.Length; ++i)
                    {
                        var player = new Player { id = networkIds[i].value };
                        enterGamePlayers[i] = player;

                        EntityManager.AddComponentData(enterGamePlayerEntitys[i], enterGamePlayers[i]);
                    }


                    //
                    inputDeps = new EnterGamePlayer2AllPlayerJob
                    {
                        enterGamePlayerEntitys = enterGamePlayerEntitys,
                        enterGamePlayers = enterGamePlayers,
                    }
                    .Schedule(this, inputDeps);


                    //
                    inputDeps = new AllPlayer2EnterGamePlayerJob
                    {
                        enterGamePlayerEntitys = enterGamePlayerEntitys,
                        enterGameOutBuffers = GetBufferFromEntity<NetworkReliableOutBuffer>(),
                    }
                    .ScheduleSingle(this, inputDeps);
                }
            }



            //QuitGameMessage
            if (quitGameQuery.IsEmptyIgnoreFilter == false)
            {
                var quitGamePlayers = quitGameQuery.ToComponentDataArray<Player>(Allocator.TempJob, out var arrayJob);

                //
                inputDeps = new PlayerDestroy2AllPlayerJob
                {
                    quitGamePlayers = quitGamePlayers,
                }
                .Schedule(this, JobHandle.CombineDependencies(inputDeps, arrayJob));


                //

                //需要延迟删除 其他地方需要处理
                endBarrier.CreateCommandBuffer().RemoveComponent(quitGameQuery, typeof(Player));
            }

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }



    //----------------------------------------------------------------------------------------------------------------------

    public struct MyPlayerSingleton : IComponentData
    {
        public int networkId;
        public Entity playerEntity;
        public int playeId;
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerClientSystem : ComponentSystem
    {
        public int maxPlayerCount = 128;
        NativeHashMap<int, Entity> _playerEntityFromPlayerId;
        public NativeHashMap<int, Entity> playerEntityFromPlayerId => _playerEntityFromPlayerId;
        //public NativeArray<int> playerIds => _playerEntityFromPlayerId.GetKeyArray(Allocator.Temp);

        //
        protected override void OnDestroy()
        {
            _playerEntityFromPlayerId.Dispose();
        }

        protected override void OnCreate()
        {
            _playerEntityFromPlayerId = new NativeHashMap<int, Entity>(maxPlayerCount, Allocator.Persistent);


            var singletonEntity = EntityManager.CreateEntity(ComponentType.ReadOnly<MyPlayerSingleton>());
#if UNITY_EDITOR
            EntityManager.SetName(singletonEntity, "MyPlayerSingleton");
#endif
            EntityManager.SetComponentData(singletonEntity, new MyPlayerSingleton { networkId = -1, playerEntity = Entity.Null, playeId = -1 });


            /*enterGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<Player>(), ComponentType.ReadOnly<T>() },
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });*/
        }
        //EntityQuery enterGameQuery;


        protected override void OnUpdate()
        {
            var myPlayerSingleton = GetSingleton<MyPlayerSingleton>();
            if (myPlayerSingleton.networkId <= 0)
            {
                Entities
                    .WithAllReadOnly<NetworkConnection, NetworkId>()
                    .WithNone<NetworkDisconnectedMessage>()
                    .ForEach((ref NetworkId networkId) =>
                    {
                        myPlayerSingleton.networkId = networkId.value;

                        SetSingleton(myPlayerSingleton);
                    });
            }

            Entities
                .WithAllReadOnly<NetworkConnection, NetworkConnectedMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddComponent<PlayerCreateNetMessages>(entity);
                    EntityManager.AddComponent<PlayerDestroyNetMessages>(entity);
                });



            //接收服务器上发来的所有new PlayerId
            using (var playerCreateNetMessagesList = Entities
                                                        .WithAllReadOnly<NetworkConnection, PlayerCreateNetMessages>()
                                                        .WithNone<NetworkDisconnectedMessage>()
                                                        .AllBufferElementToList<PlayerCreateNetMessages>(Allocator.Temp))
            {
                foreach (var playerCreateNetMessage in playerCreateNetMessagesList)
                {
                    var playerEntity = EntityManager.CreateEntity();
                    EntityManager.AddComponentData(playerEntity, new Player { id = playerCreateNetMessage.id });
                    //EntityManager.AddComponent<OnCreateMessage>(playerEntity);
#if UNITY_EDITOR
                    EntityManager.SetName(playerEntity, $"cPlayer:{playerCreateNetMessage.id}");
#endif

                    //Debug.Log($"PlayerCreateNetMessages  playerEntity={playerEntity}");
                    if (_playerEntityFromPlayerId.TryAdd(playerCreateNetMessage.id, playerEntity))
                    {
                        if (myPlayerSingleton.networkId == playerCreateNetMessage.id)
                        {
                            myPlayerSingleton.playerEntity = playerEntity;
                            myPlayerSingleton.playeId = playerCreateNetMessage.id;
                            SetSingleton(myPlayerSingleton);
                        }
                    }
                    else
                    {
                        Debug.LogError($"{World.Name} => playerEntityFromId.TryAdd(playerId.value, entity) == false  playerId:{playerCreateNetMessage.id}");
                    }
                }
            }
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerDestroyClientSystem : ComponentSystem
    {
        PlayerClientSystem playerClientSystem;
        protected override void OnCreate()
        {
            playerClientSystem = World.GetExistingSystem<PlayerClientSystem>();

            quitGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<Player>() },
            });
        }
        EntityQuery quitGameQuery;

        protected override void OnUpdate()
        {
            //接收服务器上发来的所有delete PlayerId
            Entities
                .WithAllReadOnly<NetworkConnection, PlayerDestroyNetMessages>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((DynamicBuffer<PlayerDestroyNetMessages> playerDestroyNetMessages) =>
                {
                    for (int i = 0; i < playerDestroyNetMessages.Length; ++i)
                    {
                        var playerId = playerDestroyNetMessages[i];

                        if (playerClientSystem.playerEntityFromPlayerId.TryGetValue(playerId.value, out Entity playerEntity) == false)
                        {
                            Debug.LogError($"{World.Name} => playerEntityFromPlayerId.TryGetValue(playerId.value={playerId.value}, out Entity playerEntity) == false");
                            continue;
                        }

                        playerClientSystem.playerEntityFromPlayerId.Remove(playerId.value);
                        PostUpdateCommands.DestroyEntity(playerEntity);
                    }
                });





            //客户端断开后的处理
            var disconnectedMessage = false;
            Entities
                .WithAllReadOnly<NetworkConnection>()
                .WithAnyReadOnly<NetworkDisconnectedMessage>()
                .ForEach((Entity _) =>
                {
                    disconnectedMessage = true;
                });


            if (disconnectedMessage)
            {
                EntityManager.DestroyEntity(quitGameQuery);

                playerClientSystem.playerEntityFromPlayerId.Clear();

                SetSingleton(new MyPlayerSingleton { networkId = -1, playerEntity = Entity.Null, playeId = -1 });
            }
        }
    }
}