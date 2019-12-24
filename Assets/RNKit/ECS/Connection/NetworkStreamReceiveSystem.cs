using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using Unity.Networking.Transport.LowLevel.Unsafe;

namespace RN.Network
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class NetworkStreamReceiveSystem : JobComponentSystem
    {
        NetworkStreamSystem networkStreamSystem;
        EndCommandBufferSystem endBarrier;

        protected override void OnDestroy()
        {
        }

        protected override void OnCreate()
        {
            networkStreamSystem = World.GetExistingSystem<NetworkStreamSystem>();
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        [BurstCompile]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct ReceiveJob : IJobForEachWithEntity_EBC<NetworkInBuffer, NetworkConnection>
        {
            public ComponentType NetworkConnectedMessage;
            public EntityCommandBuffer.Concurrent commandBuffer;
            public SampleCommandBuffer<NetworkDisconnectedMessage>.Concurrent endCommandBuffer;
            public UdpNetworkDriver.Concurrent driver;
            public unsafe void Execute(Entity entity, int index, DynamicBuffer<NetworkInBuffer> inBuffer, ref NetworkConnection connection)
            {
                //if (!connection.value.IsCreated) return;


                if (inBuffer.Length > 0)
                {
                    inBuffer.Clear();
                    endCommandBuffer.AddComponent(entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrors.Receive_InBufferLength });
                    return;
                }


                DataStreamReader reader;
                NetworkEvent.Type networkEvent;
                while ((networkEvent = driver.PopEventForConnection(connection.value, out reader)) != NetworkEvent.Type.Empty)
                {
                    //Debug.LogWarning($"networkEvent={networkEvent}");
                    switch (networkEvent)
                    {
                        case NetworkEvent.Type.Connect:
                            commandBuffer.AddComponent(index, entity, NetworkConnectedMessage);
                            break;

                        case NetworkEvent.Type.Disconnect:
                            //Debug.Log($"Disconnect  entity={entity}");
                            // Flag the connection as lost, it will be deleted in a separate system, giving user code one frame to detect and respond to lost connection
                            endCommandBuffer.AddComponent(entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrors.Disconnect });
                            return;

                        case NetworkEvent.Type.Data:
                            var oldLen = inBuffer.Length;
                            inBuffer.ResizeUninitialized(oldLen + reader.Length);
                            UnsafeUtility.MemCpy(((byte*)inBuffer.GetUnsafePtr()) + oldLen,
                                reader.GetUnsafeReadOnlyPtr(),
                                reader.Length);
                            break;

                        default:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                            throw new InvalidOperationException("Received unknown network event " + networkEvent);
#else
                            break;
#endif
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //
            using (var commandBuffer = new EntityCommandBuffer(Allocator.TempJob))
            using (var endCommandBuffer = new SampleCommandBuffer<NetworkDisconnectedMessage>(Allocator.TempJob))
            {
                inputDeps = new ReceiveJob()
                {
                    NetworkConnectedMessage = typeof(NetworkConnectedMessage),
                    driver = networkStreamSystem.concurrentDriver,
                    commandBuffer = commandBuffer.ToConcurrent(),
                    endCommandBuffer = endCommandBuffer.ToConcurrent(),
                }
                .Schedule(this, inputDeps);

                inputDeps.Complete();
                commandBuffer.Playback(EntityManager);
                endCommandBuffer.Playback(endBarrier);
            }

            //endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}