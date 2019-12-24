using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network
{
    [ActorEntity]
    public struct ActorVelocity : IComponentData
    {
        public float3 value;
        public float drag;
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorVelocitySystem : JobComponentSystem
    {
        [BurstCompile]
        struct ActorVelocityJob : IJobForEach<Translation, ActorVelocity>
        {
            public float fixedDeltaTime;

            public void Execute(ref Translation translation, ref ActorVelocity velocity)
            {
                translation.Value = translation.Value + velocity.value * fixedDeltaTime;

                var drag = math.max(0f, 1f - velocity.drag * fixedDeltaTime);
                velocity.value = velocity.value * drag;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new ActorVelocityJob
            {
                fixedDeltaTime = Time.fixedDeltaTime,
            }
            .Schedule(this, inputDeps);

            return inputDeps;
        }
    }
}