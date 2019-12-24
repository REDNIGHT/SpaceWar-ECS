using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;

namespace RN.Network
{
    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    public class NetworkStreamSystem : JobComponentSystem
    {
        //
        public UdpNetworkDriver driver => _driver;
        internal UdpNetworkDriver.Concurrent concurrentDriver => _concurrentDriver;

        public NetworkPipeline unreliablePipeline => _unreliablePipeline;
        public NetworkPipeline reliablePipeline => _reliablePipeline;
        //public bool listening => _listening;

        private UdpNetworkDriver _driver;
        private UdpNetworkDriver.Concurrent _concurrentDriver;
        private NetworkPipeline _unreliablePipeline;
        private NetworkPipeline _reliablePipeline;
        private bool _listening;


        /// <summary> 客户端尝试连接次数 </summary>
        public int maxConnectAttempts = NetworkParameterConstants.MaxConnectAttempts;
        //public int connectTimeoutMS = NetworkParameterConstants.ConnectTimeoutMS;//这个变量没用 UdpNetworkDriver里的逻辑有问题
        /// <summary> 在服务器 连接在没有收到数据时 超过这个时间会断开连接 </summary>
        public int disconnectTimeoutMS = NetworkParameterConstants.DisconnectTimeoutMS * 2;

        public int clientPacketDelay = 0;
        public int clientPacketDrop = 0;

        EndCommandBufferSystem endBarrier;
        protected override void OnDestroy()
        {
            _driver.Dispose();
        }

        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Debug.Assert(UnsafeUtility.SizeOf<NetworkInBuffer>() == 1);
            Debug.Assert(UnsafeUtility.SizeOf<NetworkReliableOutBuffer>() == 1);
            Debug.Assert(UnsafeUtility.SizeOf<NetworkUnreliableOutBuffer>() == 1);
#endif

            init();
        }

        protected void init()
        {
            //
            var reliabilityParams = new ReliableUtility.Parameters { WindowSize = 32 };
            var configParameter = new NetworkConfigParameter
            {
                maxConnectAttempts = maxConnectAttempts,
                connectTimeoutMS = NetworkParameterConstants.ConnectTimeoutMS,
                disconnectTimeoutMS = disconnectTimeoutMS
            };
#if UNITY_EDITOR
            int networkRate = (int)(1f / Time.fixedDeltaTime); // TODO: read from some better place
                                                               // All 3 packet types every frame stored for maximum delay, doubled for safety margin
            int maxPackets = 2 * (networkRate * 3 * clientPacketDelay + 999) / 1000;
            var simulatorParams = new SimulatorUtility.Parameters
            { MaxPacketSize = NetworkParameterConstants.MTU, MaxPacketCount = maxPackets, PacketDelayMs = clientPacketDelay, PacketDropPercentage = clientPacketDrop };

            _driver = new UdpNetworkDriver(simulatorParams, reliabilityParams, configParameter);
#else
            _driver = new UdpNetworkDriver(reliabilityParams, configParameter);
#endif

            _concurrentDriver = _driver.ToConcurrent();
            _unreliablePipeline = NetworkPipeline.Null;
            _reliablePipeline = NetworkPipeline.Null;
            _listening = false;
        }

        Entity connectEntity;
        public void Connect(NetworkEndPoint endpoint)
        {
            if (connectEntity != Entity.Null)
            {
                Debug.LogError("connectEntity != Entity.Null");
                return;
            }

            if (_unreliablePipeline == NetworkPipeline.Null)
            {
#if UNITY_EDITOR
                if (clientPacketDelay > 0 || clientPacketDrop > 0)
                    _unreliablePipeline = _driver.CreatePipeline(typeof(SimulatorPipelineStage), typeof(SimulatorPipelineStageInSend));
                else
#endif
                {
                    _unreliablePipeline = _driver.CreatePipeline(typeof(NullPipelineStage));
                }
            }
            if (_reliablePipeline == NetworkPipeline.Null)
            {
#if UNITY_EDITOR
                if (clientPacketDelay > 0 || clientPacketDrop > 0)
                    _reliablePipeline = _driver.CreatePipeline(typeof(SimulatorPipelineStageInSend), typeof(ReliableSequencedPipelineStage), typeof(SimulatorPipelineStage));
                else
#endif
                {
                    _reliablePipeline = _driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
                }
            }

            if (closeQuery == null)
            {
                closeQuery = GetEntityQuery(new EntityQueryDesc
                {
                    All = new ComponentType[] { ComponentType.ReadOnly<NetworkDisconnectedMessage>(), /*ComponentType.ReadOnly<NetworkConnection>()*/ },
                });
            }


            //
            var connection = _driver.Connect(endpoint);
            connectEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(connectEntity, new NetworkConnection { value = connection });
            EntityManager.AddComponent<NetworkInBuffer>(connectEntity);
            EntityManager.AddComponent<NetworkReliableOutBuffer>(connectEntity);
            EntityManager.AddComponent<NetworkUnreliableOutBuffer>(connectEntity);
            //EntityManager.AddComponent<NetworkConnectedMessage>(connectEntity);
            EntityManager.AddComponent<NetworkConnectMessage>(connectEntity);
#if UNITY_EDITOR
            EntityManager.SetName(connectEntity, "cNetworkConnection");
#endif
        }
        public void Disconnected()
        {
            if (connectEntity == Entity.Null)
            {
                Debug.LogError("connectEntity == Entity.Null");
                return;
            }
            var endCommandBuffer = endBarrier.CreateCommandBuffer();
            endCommandBuffer.AddComponent(connectEntity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrors.Disconnect });
            connectEntity = Entity.Null;
        }

        EntityQuery closeQuery;
        void OnClose()
        {
            if (closeQuery.IsEmptyIgnoreFilter == false)
            {
                using (var entitys = closeQuery.ToEntityArray(Allocator.TempJob))
                {
                    foreach (var entity in entitys)
                    {
                        if (entity == connectEntity)
                        {
                            connectEntity = Entity.Null;
                        }
                        else
                        {
                            Debug.LogError("entity != connectEntity");
                        }
                    }
                }
            }
        }

        public bool Listen(NetworkEndPoint endpoint)
        {
            if (_unreliablePipeline == NetworkPipeline.Null)
                _unreliablePipeline = _driver.CreatePipeline(typeof(NullPipelineStage));
            if (_reliablePipeline == NetworkPipeline.Null)
                _reliablePipeline = _driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            // Switching to server mode
            if (_driver.Bind(endpoint) != 0)
                return false;
            if (_driver.Listen() != 0)
                return false;
            _listening = true;
            // FIXME: Bind breaks all copies of the driver nad makes them send to the wrong socket
            _concurrentDriver = _driver.ToConcurrent();


#if UNITY_EDITOR
            renameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<NetworkConnectedMessage>() },
            });
#endif
            return true;
        }

#if UNITY_EDITOR
        EntityQuery renameQuery;
        void OnRename()
        {
            if (renameQuery.IsEmptyIgnoreFilter == false)
            {
                using (var entitys = renameQuery.ToEntityArray(Allocator.TempJob))
                {
                    foreach (var entity in entitys)
                    {
                        EntityManager.SetName(entity, "sNetworkConnection:" + entity.Index);
                    }
                }
            }
        }
#endif

        //[BurstCompile]
        struct AcceptJob : IJob
        {
            public ComponentType NetworkInBuffer;
            public ComponentType NetworkReliableOutBuffer;
            public ComponentType NetworkUnreliableOutBuffer;
            public EntityCommandBuffer endCommandBuffer;
            public UdpNetworkDriver driver;

            public void Execute()
            {
                Unity.Networking.Transport.NetworkConnection connection;
                while ((connection = driver.Accept()) != default)
                {
                    // New connection can never have any events, if this one does - just close it
                    DataStreamReader reader;
                    if (connection.PopEvent(driver, out reader) != NetworkEvent.Type.Empty)
                    {
                        //connection.Disconnect(driver);
#if UNITY_EDITOR
                        Debug.LogError("connection.PopEvent(driver, out reader) != NetworkEvent.Type.Empty");
#endif


                        var entity = endCommandBuffer.CreateEntity();
                        endCommandBuffer.AddComponent(entity, new NetworkConnection { value = connection });

                        endCommandBuffer.AddComponent(entity, new NetworkConnectedMessage { });
                        endCommandBuffer.AddComponent(entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrors.Accept });

                        //endCommandBuffer.RemoveComponent<NetworkConnectedMessage>(entity);//这里的entity不能跨CommandBuffer使用
                        //endCommandBuffer.RemoveComponent<NetworkDisconnectedMessage>(entity);//这里的entity不能跨CommandBuffer使用
                    }
                    else
                    {
                        // create an entity for the new connection
                        var entity = endCommandBuffer.CreateEntity();
                        endCommandBuffer.AddComponent(entity, new NetworkConnection { value = connection });

                        endCommandBuffer.AddComponent(entity, NetworkInBuffer);
                        endCommandBuffer.AddComponent(entity, NetworkReliableOutBuffer);
                        endCommandBuffer.AddComponent(entity, NetworkUnreliableOutBuffer);

                        endCommandBuffer.AddComponent(entity, new NetworkConnectedMessage { });
                        //endCommandBuffer.RemoveComponent<NetworkConnectedMessage>(entity);//这里的entity不能跨CommandBuffer使用
                    }
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(NetworkDisconnectedMessage))]
        struct CloseJob : IJobForEachWithEntity<NetworkConnection>
        {
            public EntityCommandBuffer.Concurrent commandBuffer;
            public UdpNetworkDriver driver;
            public void Execute(Entity entity, int index, [ReadOnly] ref NetworkConnection connection)
            {
                //Debug.LogWarning($"Close  entity{entity.Index}  {connection.value.GetState(driver)}");
                if (connection.value.IsCreated && connection.value.GetState(driver) != Unity.Networking.Transport.NetworkConnection.State.Disconnected)
                {
                    connection.value.Close(driver);
                }

                commandBuffer.DestroyEntity(index, entity);
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //
            inputDeps = new CloseJob
            {
                commandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
                driver = driver,
            }
            .ScheduleSingle(this, inputDeps);


            //
            inputDeps = _driver.ScheduleUpdate(inputDeps);


            //
            if (_listening)
            {
                inputDeps = new AcceptJob()
                {
                    NetworkInBuffer = typeof(NetworkInBuffer),
                    NetworkReliableOutBuffer = typeof(NetworkReliableOutBuffer),
                    NetworkUnreliableOutBuffer = typeof(NetworkUnreliableOutBuffer),
                    endCommandBuffer = endBarrier.CreateCommandBuffer(),
                    driver = _driver,
                }
                .Schedule(inputDeps);


#if UNITY_EDITOR
                OnRename();
#endif
            }
            else if (connectEntity != Entity.Null)
            {
                OnClose();
            }


            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}
