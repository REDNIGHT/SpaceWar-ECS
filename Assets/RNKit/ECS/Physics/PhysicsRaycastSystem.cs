using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    public struct PhysicsRaycast : IComponentData
    {
        public Ray ray;
        public float distance;
        public int layerMask;
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    class PhysicsRaycastSystem : ComponentSystem
    {
        protected override void OnDestroy()
        {
        }
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var endCommandBuffer = endBarrier.CreateCommandBuffer();

            Entities
                .WithAll<PhysicsRaycastResults>().WithAllReadOnly<PhysicsRaycast, OnPhysicsCallMessage>()
                .WithNone<OnDestroyMessage>()
                .ForEach((DynamicBuffer<PhysicsRaycastResults> raycastResults, ref PhysicsRaycast raycast) =>
                {
                    if (Physics.Raycast(raycast.ray, out RaycastHit hitInfo, raycast.distance, raycast.layerMask))
                    {
                        if (EntityBehaviour.getEntity(hitInfo.rigidbody, out var entity, World))
                        {
                            raycastResults.Add(new PhysicsRaycastResults { entity = entity, point = hitInfo.point, normal = hitInfo.normal, distance = hitInfo.distance });
                        }
                    }
                });
        }
    }
}
