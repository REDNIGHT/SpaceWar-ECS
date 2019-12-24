using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network
{
    public struct PhysicsOverlapSphere : IComponentData
    {
        public float radius;
        public int layerMask;

        public bool linecastFilter;
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    class PhysicsOverlapSphereSystem : ComponentSystem
    {
        protected override void OnDestroy()
        {
        }
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        Collider[] results = new Collider[16];
        protected override void OnUpdate()
        {
            var endCommandBuffer = endBarrier.CreateCommandBuffer();

            /*Entities
                .WithAll<Translation, OnPhysicsCallMessage, PhysicsOverlapSphere, PhysicsColliderResults>()
                .ForEach((Entity entity, DynamicBuffer<PhysicsColliderResults> overlapResults, ref Translation translation, ref PhysicsOverlapSphere overlapSphere) =>
                {
                    int numFound = Physics.OverlapSphereNonAlloc(translation.Value, overlapSphere.radius, results, overlapSphere.layerMask);
                    if (numFound > 0)
                    {
                        for (int i = 0; i < numFound; i++)
                        {
                            if (GOEntity.getEntity(results[i], out Entity colliderEntity, World) == false)
                                continue;
                            overlapResults.Add(new PhysicsColliderResults { colliderEntity = colliderEntity });
                        }
                    }
                });*/

            Entities
                .WithAll<PhysicsResults>().WithAllReadOnly<Translation, PhysicsOverlapSphere, OnPhysicsCallMessage>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Entity entity, DynamicBuffer<PhysicsResults> rigidbodyResults, ref Translation translation, ref PhysicsOverlapSphere overlapSphere) =>
                {
                    int numFound = Physics.OverlapSphereNonAlloc(translation.Value, overlapSphere.radius, results, overlapSphere.layerMask);
                    if (numFound > 0)
                    {
                        var rs = results
                            .Take(numFound)
                            .Where(x => x.attachedRigidbody != null)
                            ;//.Distinct();移除不同Collider但是却是同一个刚体的

                        DynamicBuffer<PhysicsOverlapHitPoints> physicsOverlapHitPoints = default;
                        var hasPhysicsOverlapHitPoints = false;
                        if (EntityManager.HasComponent<PhysicsOverlapHitPoints>(entity))
                        {
                            hasPhysicsOverlapHitPoints = true;
                            physicsOverlapHitPoints = EntityManager.GetBuffer<PhysicsOverlapHitPoints>(entity);
                        }

                        foreach (var collider in rs)
                        {
                            var rigidbody = collider.attachedRigidbody;
                            if (EntityBehaviour.getEntity(rigidbody, out var rigidbodyEntity, World) == false/* || entity == rigidbodyEntity*/)
                                continue;

                            Debug.Assert(entity != rigidbodyEntity, $"entity != rigidbodyEntity  rigidbody={rigidbody}", rigidbody);

                            if (overlapSphere.linecastFilter)
                            {
                                var start = translation.Value;
                                var end = rigidbody.position;
                                if (Physics.Linecast(start, end, out var hitInfo, overlapSphere.layerMask))
                                {
                                    if (hitInfo.rigidbody != rigidbody)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    hitInfo.point = start;
                                }


                                if (hasPhysicsOverlapHitPoints)
                                {
                                    physicsOverlapHitPoints.Add(new PhysicsOverlapHitPoints { value = hitInfo.point });
                                }
                            }


                            rigidbodyResults.Add(new PhysicsResults { entity = rigidbodyEntity });
                        };
                    }
                });
        }
    }
}