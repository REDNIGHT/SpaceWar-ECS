using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;

namespace RN.Network
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class NetworkStreamSendSystem : JobComponentSystem
    {
        [BurstCompile]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SendJob<TBufferElementData> : IJobForEachWithEntity_EBC<TBufferElementData, NetworkConnection> where TBufferElementData : struct, IBufferElementData
        {
            public UdpNetworkDriver.Concurrent driver;
            public NetworkPipeline pipeline;
            public unsafe void Execute(Entity entity, int index, DynamicBuffer<TBufferElementData> buffer, ref NetworkConnection connection)
            {
                //if (!connection.value.IsCreated)
                //    return;

                if (buffer.Length > 0)
                {
                    driver.Send(pipeline, connection.value, (System.IntPtr)buffer.GetUnsafePtr(), buffer.Length);
                    buffer.Clear();

                    /*DataStreamWriter tmp = new DataStreamWriter(buffer.Length, Allocator.Temp);
                    tmp.WriteBytes((byte*)buffer.GetUnsafePtr(), buffer.Length);
                    driver.Send(unreliablePipeline, connection.Value, tmp);
                    buffer.Clear();*/
                }
            }
        }



        //
        private NetworkStreamSystem networkStreamSystem;
        protected override void OnCreate()
        {
            networkStreamSystem = World.GetExistingSystem<NetworkStreamSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new SendJob<NetworkReliableOutBuffer>
            {
                driver = networkStreamSystem.concurrentDriver,
                pipeline = networkStreamSystem.reliablePipeline,
            }
            .Schedule(this, inputDeps);

            inputDeps = new SendJob<NetworkUnreliableOutBuffer>
            {
                driver = networkStreamSystem.concurrentDriver,
                pipeline = networkStreamSystem.unreliablePipeline,
            }
            .Schedule(this, inputDeps);

            return inputDeps;
        }
    }

}