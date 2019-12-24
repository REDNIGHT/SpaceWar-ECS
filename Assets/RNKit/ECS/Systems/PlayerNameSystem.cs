using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;

namespace RN.Network
{
    [ServerNetworkEntity]
    [ClientPlayerEntity]
    public struct PlayerName : IComponentData
    {
        public NativeString64 value;
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct PlayerNameNetMessages : IBufferElementData
    {
        public int playerId;
        public NativeString64 value;
    }

    public struct PlayerNameSerialize : NetworkSerialize.ISerializer
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.PlayerName;

        public int playerId;
        public NativeString64 value;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new PlayerNameNetMessages { playerId = playerId, value = value }, commandBuffer);
        }

        public unsafe void Serialize(DataStreamWriter writer)
        {
            writer.Write(playerId);

            Serialize(value, writer);
        }

        public unsafe void Deserialize(DataStreamReader reader, ref DataStreamReader.Context ctx)
        {
            playerId = reader.ReadInt(ref ctx);

            Deserialize(ref value, reader, ref ctx);
        }

        public static unsafe void Serialize(in NativeString64 value, DataStreamWriter writer)
        {
            var buffer = stackalloc char[value.Length];
            value.CopyTo(buffer, out int charLength, value.Length);


            var byteLength = charLength * 2;
            writer.Write((byte)byteLength);
            writer.WriteBytes((byte*)buffer, byteLength);
        }

        public static unsafe void Deserialize(ref NativeString64 value, DataStreamReader reader, ref DataStreamReader.Context ctx)
        {
            var byteLength = reader.ReadByte(ref ctx);
            var charLength = byteLength / 2;
            var charBuffer = stackalloc char[charLength];
            reader.ReadBytes(ref ctx, (byte*)charBuffer, byteLength);

            value.CopyFrom(charBuffer, charLength);
        }
    }


    [ServerNetworkEntity]
    [ServerAutoClear]
    public struct PlayerNameChangeNetMessage : IComponentData
    {
        public NativeString64 value;
    }

    public struct PlayerNameChangeSerialize : NetworkSerialize.ISerializer
    {
        public SC_CS sc_cs => SC_CS.client2server;
        public short Type => (short)NetworkSerializeType.PlayerNameChange;

        public NativeString64 value;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.AddComponent(index, entity, new PlayerNameChangeNetMessage { value = value });
        }


        public unsafe void Serialize(DataStreamWriter writer)
        {
            PlayerNameSerialize.Serialize(value, writer);
        }

        public unsafe void Deserialize(DataStreamReader reader, ref DataStreamReader.Context ctx)
        {
            PlayerNameSerialize.Deserialize(ref value, reader, ref ctx);
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerNameServerSystem : JobComponentSystem
    {
        [BurstCompile]//PlayerNameSerialize._DoSerialize.DataStreamWriter.Dispose() 不支持BurstCompile wtf!
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct EnterGamePlayerName2AllPlayerJob : IJobForEachWithEntity_EB<NetworkReliableOutBuffer>
        {
            [ReadOnly] public NativeArray<Entity> enterGamePlayerEntitys;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Player> enterGamePlayers;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<PlayerName> enterGamePlayerNames;
            public void Execute(Entity playerEntity, int index, DynamicBuffer<NetworkReliableOutBuffer> outBuffer)
            {
                //排除刚进入游戏的playerEntity
                for (int i = 0; i < enterGamePlayers.Length; ++i)
                {
                    if (playerEntity == enterGamePlayerEntitys[i])
                        return;
                }

                for (int i = 0; i < enterGamePlayers.Length; ++i)
                {
                    //Debug.LogWarning($"EnterGamePlayerName2AllClientsJob  enterGamePlayers[i].id={enterGamePlayers[i].id}");
                    var s = new PlayerNameSerialize { playerId = enterGamePlayers[i].id, value = enterGamePlayerNames[i].value };
                    s._DoSerializeInJob(outBuffer);
                }
            }
        }

        [BurstCompile]//PlayerNameSerialize._DoSerialize.DataStreamWriter.Dispose() 不支持BurstCompile wtf!
        //[RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage), typeof(PlayerNameChangeNetMessage)/*排除掉这种情况 因为会出现发送两次信息的情况*/)]
        struct AllPlayerName2EnterGamePlayerJob : IJobForEach<Player, PlayerName>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> enterGamePlayerEntitys;
            public BufferFromEntity<NetworkReliableOutBuffer> enterGameOutBuffers;
            public void Execute([ReadOnly] ref Player player, [ReadOnly]ref PlayerName playerName)
            {
                for (int i = 0; i < enterGamePlayerEntitys.Length; ++i)
                {
                    var enterGamePlayerEntity = enterGamePlayerEntitys[i];
                    var enterGame_outBuffer = enterGameOutBuffers[enterGamePlayerEntity];

                    //
                    //Debug.LogWarning($"AllPlayerName2EnterGameClientJob  player.id={player.id}");
                    var s = new PlayerNameSerialize { playerId = player.id, value = playerName.value };
                    s._DoSerializeInJob(enterGame_outBuffer);
                }
            }
        }


        //没有Player时修改PlayerName  修改不会被广播出去
        [BurstCompile]
        [RequireComponentTag(typeof(NetworkConnection))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage), typeof(Player), typeof(PlayerName))]
        struct PlayerNameChangeJobA : IJobForEachWithEntity<PlayerNameChangeNetMessage>
        {
            public SampleCommandBuffer<PlayerName>.Concurrent endCommandBuffer;
            public void Execute(Entity playerNameChangeEntity, int index, [ReadOnly] ref PlayerNameChangeNetMessage playerNameChangeNetMessage)
            {
                endCommandBuffer.AddComponent(playerNameChangeEntity, new PlayerName { value = playerNameChangeNetMessage.value });
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(NetworkConnection))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage), typeof(Player))]
        struct PlayerNameChangeJobB : IJobForEach<PlayerName, PlayerNameChangeNetMessage>
        {
            public void Execute(ref PlayerName playerName, [ReadOnly]ref PlayerNameChangeNetMessage playerNameChangeNetMessage)
            {
                playerName.value = playerNameChangeNetMessage.value;
            }
        }

        //有Player时修改PlayerName  会被广播出去
        [BurstCompile]
        [RequireComponentTag(typeof(NetworkConnection))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct PlayerNameChangeJobC : IJobForEach<Player, PlayerName, PlayerNameChangeNetMessage>
        {
            public NativeQueue<Player>.ParallelWriter players;
            public NativeQueue<PlayerName>.ParallelWriter playerNames;

            public void Execute([ReadOnly]ref Player player, ref PlayerName playerName, [ReadOnly]ref PlayerNameChangeNetMessage playerNameChangeNetMessage)
            {
                playerName.value = playerNameChangeNetMessage.value;

                players.Enqueue(player);
                playerNames.Enqueue(playerName);
            }
        }

        [BurstCompile]//PlayerNameSerialize._DoSerialize.DataStreamWriter.Dispose() 不支持BurstCompile wtf!
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct ChangePlayerName2AllClientsJob : IJobForEach_B<NetworkReliableOutBuffer>
        {
            [ReadOnly] public NativeArray<Player> players;
            [ReadOnly] public NativeArray<PlayerName> playerNames;

            public void Execute(DynamicBuffer<NetworkReliableOutBuffer> outBuffer)
            {
                for (int i = 0; i < players.Length; ++i)
                {
                    var s = new PlayerNameSerialize { playerId = players[i].id, value = playerNames[i].value };
                    s._DoSerializeInJob(outBuffer);
                }
            }
        }

        EndCommandBufferSystem endBarrier;
        protected override void OnDestroy()
        {
            players.Dispose();
            playerNames.Dispose();
            playerList.Dispose();
            playerNameList.Dispose();
        }
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();

            enterGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerEnterGameMessage>(), ComponentType.ReadOnly<Player>() },
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });
        }
        EntityQuery enterGameQuery;


        //PlayerNameChangeNetMessage
        NativeQueue<Player> players = new NativeQueue<Player>(Allocator.Persistent);
        NativeQueue<PlayerName> playerNames = new NativeQueue<PlayerName>(Allocator.Persistent);
        NativeList<Player> playerList = new NativeList<Player>(Allocator.Persistent);
        NativeList<PlayerName> playerNameList = new NativeList<PlayerName>(Allocator.Persistent);
        //PlayerNameChangeNetMessage

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (enterGameQuery.IsEmptyIgnoreFilter == false)
            {
                var enterGamePlayerEntitys = enterGameQuery.ToEntityArray(Allocator.TempJob, out var arrayJobA);
                var enterGamePlayers = enterGameQuery.ToComponentDataArray<Player>(Allocator.TempJob, out var arrayJobB);
                var enterGamePlayerNames = new NativeArray<PlayerName>(enterGamePlayerEntitys.Length, Allocator.TempJob);

                //
                JobHandle.CompleteAll(ref arrayJobA, ref arrayJobB);
                for (var i = 0; i < enterGamePlayerEntitys.Length; ++i)
                {
                    if (EntityManager.HasComponent<PlayerName>(enterGamePlayerEntitys[i]) == false)
                    {
                        var playerName = new PlayerName();
                        playerName.value.CopyFrom($"P{enterGamePlayers[i].id}");
                        enterGamePlayerNames[i] = playerName;

                        EntityManager.AddComponentData(enterGamePlayerEntitys[i], playerName);
                        //endBarrier.curCommandBuffer.AddComponent(enterGamePlayerEntitys[i], playerName);
                    }
                    else
                    {
                        enterGamePlayerNames[i] = EntityManager.GetComponentData<PlayerName>(enterGamePlayerEntitys[i]);
                    }
                }


                //
                inputDeps = new EnterGamePlayerName2AllPlayerJob
                {
                    enterGamePlayerEntitys = enterGamePlayerEntitys,
                    enterGamePlayers = enterGamePlayers,
                    enterGamePlayerNames = enterGamePlayerNames,
                }
                .Schedule(this, inputDeps);


                inputDeps = new AllPlayerName2EnterGamePlayerJob
                {
                    enterGamePlayerEntitys = enterGamePlayerEntitys,
                    enterGameOutBuffers = GetBufferFromEntity<NetworkReliableOutBuffer>(),
                }
                .ScheduleSingle(this, inputDeps);
            }




            //QuitGameMessage
            /*if (quitGameQuery.IsEmptyIgnoreFilter == false)
            {
                EntityManager.RemoveComponent<PlayerName>(quitGameQuery);
            }*/



            //PlayerNameChangeNetMessage
            using (var endCommandBuffer = new SampleCommandBuffer<PlayerName>(Allocator.TempJob))
            {
                inputDeps = new PlayerNameChangeJobA
                {
                    endCommandBuffer = endCommandBuffer.ToConcurrent(),
                }
                .Schedule(this, inputDeps);

                endCommandBuffer.Playback(endBarrier, inputDeps);
            }

            inputDeps = new PlayerNameChangeJobB
            {
            }
            .Schedule(this, inputDeps);

            players.Clear();
            playerNames.Clear();
            playerList.Clear();
            playerNameList.Clear();
            inputDeps = new PlayerNameChangeJobC
            {
                players = players.AsParallelWriter(),
                playerNames = playerNames.AsParallelWriter(),
            }
            .Schedule(this, inputDeps);

            inputDeps = players.ToListJob(ref playerList, inputDeps);
            inputDeps = playerNames.ToListJob(ref playerNameList, inputDeps);
            inputDeps = new ChangePlayerName2AllClientsJob
            {
                players = playerList.AsDeferredJobArray(),
                playerNames = playerNameList.AsDeferredJobArray(),
            }
            .Schedule(this, inputDeps);


            endBarrier.AddJobHandleForProducer(inputDeps);

            return inputDeps;
        }
    }


    //----------------------------------------------------------------------------------------------------------------------
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerNameClientSystem : ComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        PlayerClientSystem playerClientSystem;
        protected void OnInit(Transform root)
        {
            //
            World.GetExistingSystem<NetworkStreamConnectSuccessSystem>().AddSystem(this);

            //
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
            playerClientSystem = World.GetExistingSystem<PlayerClientSystem>();
        }


        //
        public string myPlayerName
        {
            get
            {
                var saveName = PlayerPrefs.GetString(PlayerNameKey);
                var myPlayerSingleton = GetSingleton<MyPlayerSingleton>();
                var myPlayerEntity = myPlayerSingleton.playerEntity;
                if (myPlayerEntity == Entity.Null)
                {
                    Debug.LogWarning("myPlayerEntity == Entity.Null");
                    return saveName;
                }

                if (EntityManager.HasComponent<PlayerName>(myPlayerEntity) == false)
                {
                    Debug.LogWarning("PlayerName Not Find!");
                    return saveName;
                }

                return EntityManager.GetComponentData<PlayerName>(myPlayerEntity).value.ToString();
            }
        }

        public int minPlayerNameCount = 2;
        public int maxPlayerNameCount = 16;
        public const string PlayerNameKey = "PlayerName";
        public void ChangeMyPlayerName(string name)
        {
            if (name.Length > maxPlayerNameCount)
            {
                Debug.LogError("name.Length > maxPlayerNameCount");
                return;
            }
            if (name.Length < minPlayerNameCount)
            {
                Debug.LogError("name.Length < minPlayerNameCount");
                return;
            }
            if (Enabled == false)
            {
                Debug.LogError("Enabled == false  Connecting..."); //连接中...
                return;
            }


            Entities
                .WithAllReadOnly<NetworkConnection>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity, DynamicBuffer<NetworkReliableOutBuffer> outBuffer) =>
                {
                    //修改成功后 服务器会发来修改后的名字 下面代码修改本地名字的代码就不需要了
                    //EntityManager.SetComponentData(playerClientSystem.myPlayerEntity, new PlayerName { value = name });

                    var s = new PlayerNameChangeSerialize();
                    s.value.CopyFrom(name);
                    s._DoSerialize(outBuffer);

                    PlayerPrefs.SetString(PlayerNameKey, name);
                });
        }

        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<NetworkConnection, NetworkConnectedMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddComponent<PlayerNameNetMessages>(entity);

                    if (PlayerPrefs.HasKey(PlayerNameKey))
                    {
                        var newName = PlayerPrefs.GetString(PlayerNameKey);
                        ChangeMyPlayerName(newName);
                    }
                });


            var endCommandBuffer = endBarrier.CreateCommandBuffer();
            //接收服务器上发来的所有新名字或修改的名字
            Entities
                .WithAllReadOnly<NetworkConnection, PlayerNameNetMessages>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity, DynamicBuffer<PlayerNameNetMessages> playerNameNetMessages) =>
                {
                    for (int i = 0; i < playerNameNetMessages.Length; ++i)
                    {
                        var playerId = playerNameNetMessages[i].playerId;
                        var playerName = playerNameNetMessages[i].value;

                        if (playerClientSystem.playerEntityFromPlayerId.TryGetValue(playerId, out Entity playerEntity) == false)
                        {
                            Debug.LogError($"{World.Name} => playerEntityFromPlayerId.TryGetValue(playerId={playerId}, out Entity playerEntity) == false");
                            endCommandBuffer.AddComponent<NetworkDisconnectedMessage>(entity);
                            continue;
                        }

                        if (EntityManager.HasComponent<PlayerName>(playerEntity))
                        {
                            PostUpdateCommands.SetComponent(playerEntity, new PlayerName { value = playerName });
                        }
                        else
                        {
                            PostUpdateCommands.AddComponent(playerEntity, new PlayerName { value = playerName });
                        }
                    }
                });


            //接收服务器上发来的所有delete PlayerId
            //PlayerEntity已经删除了 这里就不用删除PlayerName




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