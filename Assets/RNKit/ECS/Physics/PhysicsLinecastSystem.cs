using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    public struct PhysicsLinecast : IComponentData
    {
        public Vector3 start;
        public Vector3 end;
        public int layerMask;
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    class PhysicsLinecastSystem : ComponentSystem
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
                .WithAll<PhysicsRaycastResults>().WithAllReadOnly<PhysicsLinecast, OnPhysicsCallMessage>()
                .WithNone<OnDestroyMessage>()
                .ForEach((DynamicBuffer<PhysicsRaycastResults> raycastResults, ref PhysicsLinecast linecast) =>
                {
                    //Debug.DrawLine(linecast.start, linecast.end, Color.white, 0.25f);

                    if (Physics.Linecast(linecast.start, linecast.end, out var hitInfo, linecast.layerMask))
                    {
                        if (EntityBehaviour.getEntity(hitInfo.rigidbody, out Entity entity, World))
                        {
                            raycastResults.Add(new PhysicsRaycastResults { entity = entity, point = hitInfo.point, normal = hitInfo.normal, distance = hitInfo.distance });
                        }
                    }
                });
        }
    }
}

