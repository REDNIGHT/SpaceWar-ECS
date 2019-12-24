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
    public class BulletOnCreateServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(OnCreateMessage))]
        struct OnCreateJob : IJobForEach<Bullet, Translation, Rotation, RigidbodyVelocity>
        {
            public void Execute([ReadOnly]ref Bullet bullet, [ReadOnly] ref Translation translation, [ReadOnly]ref Rotation rotation, ref RigidbodyVelocity rigidbodyVelocity)
            {
                rigidbodyVelocity.linear += math.forward(rotation.Value) * bullet.velocity;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new OnCreateJob { }.Schedule(this, inputDeps);
        }
    }
}
