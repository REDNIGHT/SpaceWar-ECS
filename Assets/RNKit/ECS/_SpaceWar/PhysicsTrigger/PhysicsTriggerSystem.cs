using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PhysicsTriggerServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(OnCreateMessage))]
        //[RequireComponentTag(typeof(OnPhysicsCallMessage))]
        struct PhysicsTriggerJob : IJobForEach<PhysicsTrigger, Rotation, RigidbodyForce>
        {
            public void Execute([ReadOnly]ref PhysicsTrigger physicsTrigger, [ReadOnly] ref Rotation physicsTriggerRotation, ref RigidbodyForce rigidbodyForce)
            {
                rigidbodyForce.force = math.forward(physicsTriggerRotation.Value) * physicsTrigger.force;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new PhysicsTriggerJob
            {
            }
            .Schedule(this, inputDeps);

            return inputDeps;
        }
    }
}
