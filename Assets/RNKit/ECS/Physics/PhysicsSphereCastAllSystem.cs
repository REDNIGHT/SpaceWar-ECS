using System.Linq;
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

        /// <summary>
        /// 移除多余的Collider 这些Collider都是同一个刚体的
        /// </summary>
        public bool distinctDisable;
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
        PhysicsRaycastAllSystem.RaycastHitCompareByRigidbody raycastHitCompareByRigidbody = new PhysicsRaycastAllSystem.RaycastHitCompareByRigidbody();
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
                        var rs = results
                            .Take(numFound)
                            .Where(x => x.rigidbody != null);

                        if (sphereCastAll.distinctDisable == false)
                        {
                            rs = rs.Distinct(raycastHitCompareByRigidbody);
                        }


                        foreach (var hitInfo in rs)
                        { 
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
