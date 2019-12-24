
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    [ServerNetworkEntity]
    public struct NetworkHeartbeat : IComponentData
    {
        public float time;
    }


    [BurstCompile]
    [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
    struct Heartbeat2CJob : IJobForEachWithEntity_EBC<NetworkInBuffer, NetworkHeartbeat>
    {
        public float fixedDeltaTime;
        public float disconnectedTime;
        public SampleCommandBuffer<NetworkDisconnectedMessage>.Concurrent commandBuffer;
        public void Execute(Entity entity, int index, [ReadOnly]DynamicBuffer<NetworkInBuffer> inBuffer, ref NetworkHeartbeat heartbeat)
        {
            if (inBuffer.Length > 0)
            {
                heartbeat.time = 0;
            }
            else
            {
                heartbeat.time += fixedDeltaTime;

                if (heartbeat.time > disconnectedTime)
                {
                    commandBuffer.AddComponent(entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrorsInSystem.Heartbeat });
                }
            }

        }
    }

    [BurstCompile]
    [ExcludeComponent(typeof(NetworkDisconnectedMessage), typeof(NetworkConnectedMessage))]
    struct AddJob : IJobForEachWithEntity<NetworkConnection>
    {
        public ComponentType NetworkHeartbeat;
        public EntityCommandBuffer.Concurrent commandBuffer;
        public void Execute(Entity entity, int index, ref NetworkConnection connection)
        {
            commandBuffer.AddComponent(index, entity, NetworkHeartbeat);
        }
    }



    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class NetworkHeartbeatServerSystem : JobComponentSystem
    {
        public float disconnectedTime = 30f;

        //
        EndCommandBufferSystem endBarrier;

        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //
            inputDeps = new AddJob
            {
                NetworkHeartbeat = typeof(NetworkHeartbeat),
                commandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);



            //
            using (var commandBuffer = new SampleCommandBuffer<NetworkDisconnectedMessage>(Allocator.TempJob))
            {
                inputDeps = new Heartbeat2CJob
                {
                    commandBuffer = commandBuffer.ToConcurrent(),
                    fixedDeltaTime = Time.fixedDeltaTime,
                    disconnectedTime = disconnectedTime,
                }
                .Schedule(this, inputDeps);

                inputDeps.Complete();
                commandBuffer.Playback(EntityManager);
            }


            //endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}
