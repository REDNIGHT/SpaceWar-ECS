using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    public struct PhysicsSphereCastAll : IComponentData
    {
        public Ray ray;
        public float radius;
        public float distance;
        public int layerMask;
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    class PhysicsSphereCastAllSystem : ComponentSystem
    {
        protected override void OnDestroy()
        {
        }
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        RaycastHit[] results = new RaycastHit[16];
        protected override void OnUpdate()
        {
            var endCommandBuffer = endBarrier.CreateCommandBuffer();

            Entities
                .WithAll<PhysicsRaycastResults>().WithAllReadOnly<PhysicsSphereCastAll, OnPhysicsCallMessage>()
                .WithNone<OnDestroyMessage>()
                .ForEach((DynamicBuffer<PhysicsRaycastResults> raycastResults, ref PhysicsSphereCastAll sphereCastAll) =>
                {
                    int numFound = Physics.SphereCastNonAlloc(sphereCastAll.ray, sphereCastAll.radius, results, sphereCastAll.distance, sphereCastAll.layerMask);
                    if (numFound > 0)
                    {
                        for (int i = 0; i < numFound; i++)
                        {
                            var hitInfo = results[i];

                            if (EntityBehaviour.getEntity(hitInfo.rigidbody, out var entity, World))
                            {
                                raycastResults.Add(new PhysicsRaycastResults { entity = entity, point = hitInfo.point, normal = hitInfo.normal, distance = hitInfo.distance });
                            }
                        }
                    }
                });
        }
    }
}
