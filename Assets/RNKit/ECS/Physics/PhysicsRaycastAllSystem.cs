using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    public struct PhysicsRaycastAll : IComponentData
    {
        public Ray ray;
        public float distance;
        public int layerMask;
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    class PhysicsRaycastAllSystem : ComponentSystem
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
                .WithAll<PhysicsRaycastResults>().WithAllReadOnly<PhysicsRaycastAll, OnPhysicsCallMessage>()
                .WithNone<OnDestroyMessage>()
                .ForEach((DynamicBuffer<PhysicsRaycastResults> raycastResults, ref PhysicsRaycastAll raycastAll) =>
                {
                    int numFound = Physics.RaycastNonAlloc(raycastAll.ray, results, raycastAll.distance, raycastAll.layerMask);
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
