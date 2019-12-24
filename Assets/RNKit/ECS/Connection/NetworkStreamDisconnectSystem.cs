using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Networking.Transport;

namespace RN.Network
{
    /*[DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class NetworkStreamDisconnectSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(NetworkDisconnectedMessage))]
        struct OnCloseJob : IJobForEachWithEntity<NetworkConnection>
        {
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public UdpNetworkDriver driver;
            public void Execute(Entity entity, int index, [ReadOnly] ref NetworkConnection connection)
            {
                if (connection.value.IsCreated && connection.value.GetState(driver) != Unity.Networking.Transport.NetworkConnection.State.Disconnected)
                    connection.value.Close(driver);

                endCommandBuffer.DestroyEntity(index, entity);
            }
        }


        EndFixedCommandBufferSystem endBarrier;
        NetworkStreamSystem networkStreamSystem;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndFixedCommandBufferSystem>();
            networkStreamSystem = World.GetExistingSystem<NetworkStreamSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var endCommandBuffer = endBarrier.CreateCommandBuffer();

            inputDeps = new OnCloseJob
            {
                endCommandBuffer = endCommandBuffer.ToConcurrent(),
                driver = networkStreamSystem.driver,
            }
            .ScheduleSingle(this, inputDeps);

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }*/

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class NetworkStreamDisconnectClientSystem : ComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        private NetworkStreamSystem networkStreamSystem;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
            networkStreamSystem = World.GetExistingSystem<NetworkStreamSystem>();
        }

        public System.Action onClosed;
        protected override void OnUpdate()
        {
            var driver = networkStreamSystem.driver;
            var endCommandBuffer = endBarrier.CreateCommandBuffer();
            Entities
                .WithAllReadOnly<NetworkDisconnectedMessage, NetworkConnection>()
                .ForEach((ref NetworkDisconnectedMessage disconnected, ref NetworkConnection connection) =>
                {
                    onClosed();
                });
        }
    }
}