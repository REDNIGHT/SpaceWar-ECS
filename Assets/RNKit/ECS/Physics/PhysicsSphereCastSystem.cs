using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    public struct PhysicsSphereCast : IComponentData
    {
        public Ray ray;
        public float radius;
        public float distance;
        public int layerMask;
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    class PhysicsSphereCastSystem : ComponentSystem
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
                .WithAll<PhysicsRaycastResults>().WithAllReadOnly<PhysicsSphereCast, OnPhysicsCallMessage>()
                .WithNone<OnDestroyMessage>()
                .ForEach((DynamicBuffer<PhysicsRaycastResults> raycastResults, ref PhysicsSphereCast sphereCast) =>
                {
                    //sphereCast.ray.drawRay(sphereCast.distance);
                    if (Physics.SphereCast(sphereCast.ray, sphereCast.radius, out var hitInfo, sphereCast.distance, sphereCast.layerMask))
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
