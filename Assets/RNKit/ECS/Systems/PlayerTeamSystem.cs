using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    [ServerNetworkEntity]
    [ClientPlayerEntity]
    public struct PlayerTeam : IComponentData
    {
        /// <summary>
        /// 编队0就是独立编队 同是编队0的都是敌人 其他编号除外
        /// </summary>
        public int value;
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct PlayerTeamNetMessages : IBufferElementData
    {
        public int playerId;
        public int value;
    }

    public struct PlayerTeamSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.PlayerTeam;

        public int playerId;
        public int value;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new PlayerTeamNetMessages { playerId = playerId, value = value }, commandBuffer);
        }
    }

    [ServerNetworkEntity]
    [ServerAutoClear]
    public struct PlayerTeamChangeNetMessage : IComponentData
    {
        public int value;
    }

    public struct PlayerTeamChangeSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.client2server;
        public short Type => (short)NetworkSerializeType.PlayerTeamChange;

        public int value;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.AddComponent(index, entity, new PlayerTeamChangeNetMessage { value = value });
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerTeamServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct EnterGamePlayerTeam2AllPlayerJob : IJobForEachWithEntity_EB<NetworkReliableOutBuffer>
        {
            [ReadOnly] public NativeArray<Entity> enterGamePlayerEntitys;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Player> enterGamePlayers;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<PlayerTeam> enterGamePlayerTeams;
            public void Execute(Entity playerEntity, int index, DynamicBuffer<NetworkReliableOutBuffer> outBuffer)
            {
                //排除刚进入游戏的playerEntity
                for (int i = 0; i < enterGamePlayerEntitys.Length; ++i)
                {
                    if (playerEntity == enterGamePlayerEntitys[i])
                        return;
                }

                for (int i = 0; i < enterGamePlayerTeams.Length; ++i)
                {
                    var s = new PlayerTeamSerialize { playerId = enterGamePlayers[i].id, value = enterGamePlayerTeams[i].value };
                    s._DoSerialize(outBuffer);
                }
            }
        }

        [BurstCompile]
        //[RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage), typeof(PlayerTeamChangeNetMessage)/*排除掉这种情况 因为会出现发送两次信息的情况*/)]
        struct AllPlayerTeam2EnterGamePlayerJob : IJobForEach<Player, PlayerTeam>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> enterGamePlayerEntitys;
            public BufferFromEntity<NetworkReliableOutBuffer> enterGameOutBuffers;
            public void Execute([ReadOnly] ref Player player, [ReadOnly]ref PlayerTeam playerTeam)
            {
                for (int i = 0; i < enterGamePlayerEntitys.Length; ++i)
                {
                    var enterGamePlayerEntity = enterGamePlayerEntitys[i];

                    var enterGameOutBuffer = enterGameOutBuffers[enterGamePlayerEntity];

                    //
                    //Debug.LogWarning($"AllPlayerTeam2EnterGameClientJob  player.id={player.id}  playerTeam.value={playerTeam.value}");
                    var s = new PlayerTeamSerialize { playerId = player.id, value = playerTeam.value };
                    s._DoSerialize(enterGameOutBuffer);
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct ChangePlayerTeam2AllPlayerJob : IJobForEach_B<NetworkReliableOutBuffer>
        {
            [ReadOnly] public NativeList<Player> players;
            [ReadOnly] public NativeList<PlayerTeam> playerTeams;

            public void Execute(DynamicBuffer<NetworkReliableOutBuffer> outBuffer)
            {
                for (int i = 0; i < playerTeams.Length; ++i)
                {
                    var s = new PlayerTeamSerialize { playerId = players[i].id, value = playerTeams[i].value };
                    s._DoSerialize(outBuffer);
                }
            }
        }

        EndCommandBufferSystem endBarrier;
        protected override void OnDestroy()
        {
            players.Dispose();
            playerTeams.Dispose();
        }
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();

            enterGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerEnterGameMessage>(), ComponentType.ReadOnly<Player>() },
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });

            playerTeamChangeQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<NetworkConnection>(), ComponentType.ReadOnly<PlayerTeamChangeNetMessage>() },//可以没有Player时修改PlayerTeam
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });
        }
        EntityQuery enterGameQuery;


        EntityQuery playerTeamChangeQuery;
        NativeList<Player> players = new NativeList<Player>(Allocator.Persistent);
        NativeList<PlayerTeam> playerTeams = new NativeList<PlayerTeam>(Allocator.Persistent);

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (enterGameQuery.IsEmptyIgnoreFilter == false)
            {
                var enterGamePlayerEntitys = enterGameQuery.ToEntityArray(Allocator.TempJob, out var arrayJobA);
                var enterGamePlayerTeams = new NativeArray<PlayerTeam>(enterGamePlayerEntitys.Length, Allocator.TempJob);

                //
                arrayJobA.Complete();
                for (var i = 0; i < enterGamePlayerEntitys.Length; ++i)
                {
                    if (EntityManager.HasComponent<PlayerTeam>(enterGamePlayerEntitys[i]) == false)
                    {
                        var playerTeam = new PlayerTeam { value = 0 };//编队0就是独立编队
                        enterGamePlayerTeams[i] = playerTeam;

                        EntityManager.AddComponentData(enterGamePlayerEntitys[i], playerTeam);
                    }
                    else
                    {
                        var playerTeam = EntityManager.GetComponentData<PlayerTeam>(enterGamePlayerEntitys[i]);
                        enterGamePlayerTeams[i] = playerTeam;
                    }
                }


                //
                var enterGamePlayers = enterGameQuery.ToComponentDataArray<Player>(Allocator.TempJob, out var arrayJobB);
                inputDeps = new EnterGamePlayerTeam2AllPlayerJob
                {
                    enterGamePlayerEntitys = enterGamePlayerEntitys,
                    enterGamePlayers = enterGamePlayers,
                    enterGamePlayerTeams = enterGamePlayerTeams,
                }
                .Schedule(this, JobHandle.CombineDependencies(inputDeps, arrayJobB));


                inputDeps = new AllPlayerTeam2EnterGamePlayerJob
                {
                    enterGamePlayerEntitys = enterGamePlayerEntitys,
                    enterGameOutBuffers = GetBufferFromEntity<NetworkReliableOutBuffer>(),
                }
                .ScheduleSingle(this, inputDeps);
            }




            //QuitGameMessage
            /*if (quitGameQuery.IsEmptyIgnoreFilter == false)
            {
                EntityManager.RemoveComponent<PlayerTeam>(quitGameQuery);
            }*/




            //PlayerTeamChangeNetMessage
            if (playerTeamChangeQuery.IsEmptyIgnoreFilter == false)
            {
                using (var playerTeamChangeEntitys = playerTeamChangeQuery.ToEntityArray(Allocator.TempJob, out var arrayJobA))
                using (var playerTeamChangeNetMessages = playerTeamChangeQuery.ToComponentDataArray<PlayerTeamChangeNetMessage>(Allocator.TempJob, out var arrayJobB))
                {
                    players.Clear();
                    playerTeams.Clear();

                    //
                    JobHandle.CompleteAll(ref arrayJobA, ref arrayJobB);
                    for (var i = 0; i < playerTeamChangeEntitys.Length; ++i)
                    {
                        var playerTeamChangeEntity = playerTeamChangeEntitys[i];

                        var playerTeam = new PlayerTeam { value = playerTeamChangeNetMessages[i].value };
                        if (EntityManager.HasComponent<PlayerTeam>(playerTeamChangeEntity) == false)
                        {
                            EntityManager.AddComponentData(playerTeamChangeEntity, playerTeam);
                        }
                        else
                        {
                            EntityManager.SetComponentData(playerTeamChangeEntity, playerTeam);
                        }

                        //只有创建了Player的才会广播
                        if (EntityManager.HasComponent<Player>(playerTeamChangeEntity))
                        {
                            players.Add(EntityManager.GetComponentData<Player>(playerTeamChangeEntity));
                            playerTeams.Add(playerTeam);
                        }
                    }

                    //
                    inputDeps = new ChangePlayerTeam2AllPlayerJob
                    {
                        players = players,
                        playerTeams = playerTeams,
                    }
                    .Schedule(this, inputDeps);
                }
            }

            return inputDeps;
        }
    }


    //----------------------------------------------------------------------------------------------------------------------
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerTeamClientSystem : ComponentSystem
    {
        //编队0就是独立编队或者是没有编队
        public int myPlayerTeam
        {
            get
            {
                var myPlayerSingleton = GetSingleton<MyPlayerSingleton>();
                var myPlayerEntity = myPlayerSingleton.playerEntity;

                if (myPlayerEntity == Entity.Null)
                {
                    Debug.LogWarning("myPlayerEntity == Entity.Null");
                    return 0;
                }

                if (EntityManager.HasComponent<PlayerTeam>(myPlayerEntity) == false)
                {
                    Debug.LogWarning("PlayerTeam Not Find!");
                    return 0;
                }

                return EntityManager.GetComponentData<PlayerTeam>(myPlayerEntity).value;
            }
        }

        public void ChangeMyPlayerTeam(int team)
        {
            if (Enabled == false)
            {
                Debug.LogError("Enabled == false  Connecting..."); //连接中...
                return;
            }

            Entities
                .WithAllReadOnly<NetworkConnection>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((DynamicBuffer<NetworkReliableOutBuffer> outBuffer) =>
                {
                    //修改成功后 服务器会发来修改后的名字 下面代码修改本地名字的代码就不需要了
                    //EntityManager.SetComponentData(playerClientSystem.myPlayerEntity, new PlayerTeam { value = name });

                    var s = new PlayerTeamChangeSerialize { value = team };
                    s._DoSerialize(outBuffer);
                });
        }


        PlayerClientSystem playerClientSystem;
        EndCommandBufferSystem endBarrier;
        protected void OnInit(Transform root)
        {
            //
            World.GetExistingSystem<NetworkStreamConnectSuccessSystem>().AddSystem(this);

            //
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
            playerClientSystem = World.GetExistingSystem<PlayerClientSystem>();
        }


        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<NetworkConnection, NetworkConnectedMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddComponent<PlayerTeamNetMessages>(entity);
                });



            var endCommandBuffer = endBarrier.CreateCommandBuffer();

            //接收服务器上发来的所有新名字或修改的名字
            Entities
                .WithAllReadOnly<NetworkConnection, PlayerTeamNetMessages>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity, DynamicBuffer<PlayerTeamNetMessages> newPlayerTeamNetMessages) =>
                {
                    for (int i = 0; i < newPlayerTeamNetMessages.Length; ++i)
                    {
                        var playerId = newPlayerTeamNetMessages[i].playerId;
                        var playerTeam = newPlayerTeamNetMessages[i].value;

                        if (playerClientSystem.playerEntityFromPlayerId.TryGetValue(playerId, out Entity playerEntity) == false)
                        {
                            Debug.LogError($"{World.Name} => playerEntityFromPlayerId.TryGetValue(playerId={playerId}, out Entity playerEntity) == false");
                            endCommandBuffer.AddComponent<NetworkDisconnectedMessage>(entity);
                            continue;
                        }


                        if (EntityManager.HasComponent<PlayerTeam>(playerEntity))
                        {
                            PostUpdateCommands.SetComponent(playerEntity, new PlayerTeam { value = playerTeam });
                        }
                        else
                        {
                            PostUpdateCommands.AddComponent(playerEntity, new PlayerTeam { value = playerTeam });
                        }
                    }
                });


            //接收服务器上发来的所有delete PlayerId
            //PlayerEntity已经删除了 这里就不用删除PlayerTeam




            //离开游戏场景后的处理 并没有断开
            //客户端断开后的处理
            /*Entities
                .WithAll<NetworkStreamConnection>()
                .WithAny<NetworkStreamDisconnectedMessage, PlayerQuitGameMessage>()
                .ForEach((Entity _) =>
                {
                });*/
        }
    }

}