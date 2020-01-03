using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace RN.Network
{
    public struct PhysicsRaycastAll : IComponentData
    {
        public Ray ray;
        public float distance;
        public int layerMask;

        /// <summary>
        /// 移除多余的Collider 这些Collider都是同一个刚体的
        /// </summary>
        public bool distinctDisable;
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
        RaycastHitCompareByRigidbody raycastHitCompareByRigidbody = new RaycastHitCompareByRigidbody();
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
                        var rs = results
                            .Take(numFound)
                            .Where(x => x.rigidbody != null);

                        if (raycastAll.distinctDisable == false)
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

        public struct RaycastHitCompareByRigidbody : IEqualityComparer<RaycastHit>
        {
            public bool Equals(RaycastHit x, RaycastHit y)
            {
                if (x.rigidbody == y.rigidbody)
                    return true;
                else
                    return false;
            }

            public int GetHashCode(RaycastHit x)
            {
                if (x.rigidbody == null)
                    return 0;
                else
                    return x.rigidbody.GetHashCode();
            }
        }
    }
}
