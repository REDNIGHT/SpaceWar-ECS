using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    [ServerNetworkEntity]
    [ClientNetworkEntity]
    public struct NetworkId : IComponentData
    {
        public int value;
    }

    public struct NetworkIdSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.NetworkId;

        public int value;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.AddComponent(index, entity, new NetworkId { value = value });
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class NetworkIdServerSystem : JobComponentSystem
    {
        //
        protected override void OnDestroy()
        {
        }
        protected override void OnCreate()
        {
        }
        /*protected override void OnUpdate()
        {
            Entities
                .WithAll<NetworkConnection>()
                .WithNone<NetworkId>()
                .ForEach((Entity entity, DynamicBuffer<NetworkReliableOutBuffer> outBuffer) =>
                {
                    PostUpdateCommands.AddComponent(entity, new NetworkId { value = entity.Index });
#if UNITY_EDITOR
                    EntityManager.SetName(entity, $"serverNetwork:{entity.Index}");
#endif
                    var s = new NetworkIdSerialize { value = entity.Index };
                    s._DoSerialize(outBuffer);
                });
        }*/

        [BurstCompile]
        [RequireComponentTag(typeof(NetworkConnection))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage), typeof(NetworkId))]
        struct AddNetworkIdJob : IJobForEachWithEntity_EB<NetworkReliableOutBuffer>
        {
            public SampleCommandBuffer<NetworkId>.Concurrent commandBuffer;
            public void Execute(Entity entity, int index, DynamicBuffer<NetworkReliableOutBuffer> outBuffer)
            {
                commandBuffer.AddComponent(entity, new NetworkId { value = entity.Index });

                var s = new NetworkIdSerialize { value = entity.Index };
                s._DoSerialize(outBuffer);
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            SampleCommandBuffer<NetworkId> commandBuffer = new SampleCommandBuffer<NetworkId>(Allocator.TempJob);
            inputDeps = new AddNetworkIdJob
            {
                commandBuffer = commandBuffer.ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            inputDeps.Complete();
            commandBuffer.Playback(EntityManager);
            commandBuffer.Dispose();

            return inputDeps;
        }
    }

    [DisableAutoCreation]
    ////[AlwaysUpdateSystem]
    public class NetworkIdClientSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
        }
    }
}