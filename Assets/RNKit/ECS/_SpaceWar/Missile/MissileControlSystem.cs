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
    public class MissileControlServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(OnCreateMessage))]
        [ExcludeComponent(typeof(ControlForceDirection), typeof(ControlTorqueDirection))]
        struct ForceJobA : IJobForEach<MissileAutoExplosionByTouche, RigidbodyVelocity, Rotation>
        {
            public void Execute([ReadOnly] ref MissileAutoExplosionByTouche missileAutoExplosionByTouche, ref RigidbodyVelocity rigidbodyVelocity, [ReadOnly]ref Rotation rotation)
            {
                var forword = math.forward(rotation.Value);
                rigidbodyVelocity.linear += forword * missileAutoExplosionByTouche.beginForce;
                rigidbodyVelocity.angular += new float3(0f, missileAutoExplosionByTouche.beginTorque, 0f);
            }
        }


        [BurstCompile]
        [RequireComponentTag(typeof(Missile), typeof(OnCreateMessage))]
        //[ExcludeComponent(typeof(TraceDirection))]
        struct ForceJobB : IJobForEach<ControlForceDirection, ControlTorqueDirection, Rotation>
        {
            public void Execute(ref ControlForceDirection controlForceDirection, ref ControlTorqueDirection controlTorqueDirection, [ReadOnly]ref Rotation rotation)
            {
                var forword = math.forward(rotation.Value);
                controlForceDirection.direction = forword;
                controlTorqueDirection.direction = forword;
            }
        }


        [BurstCompile]
        [RequireComponentTag(typeof(Missile))]
        struct ForceJobC : IJobForEach<ControlForceDirection, ControlTorqueDirection, TraceDirection, Rotation>
        {
            public void Execute(ref ControlForceDirection controlForceDirection, ref ControlTorqueDirection controlTorqueDirection, [ReadOnly]ref TraceDirection traceDirection, [ReadOnly]ref Rotation rotation)
            {
                if (traceDirection.value.Equals(float3.zero))
                    return;

                controlForceDirection.direction = math.forward(rotation.Value);
                controlTorqueDirection.direction = traceDirection.value;
            }
        }

        [BurstCompile]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct AccelerateJob : IJobForEach<FindTraceTarget, MissilePhysics, ControlForceDirection>
        {
            public void Execute([ReadOnly, ChangedFilter]ref FindTraceTarget missileFindTarget, [ReadOnly]ref MissilePhysics missilePhysics, ref ControlForceDirection controlForceDirection)
            {
                if (missileFindTarget.targetEntity == default)
                    return;

                if (missilePhysics.accelerateByFindTarget)
                {
                    controlForceDirection.force = missilePhysics.forceByTarget;
                    controlForceDirection.maxVelocity = missilePhysics.maxVelocityByTarget;
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var inputDepsA = new ForceJobA { }.Schedule(this, inputDeps);
            inputDeps = new ForceJobB { }.Schedule(this, inputDeps);
            inputDeps = new ForceJobC { }.Schedule(this, inputDeps);
            inputDeps = new AccelerateJob { }.Schedule(this, inputDeps);

            return JobHandle.CombineDependencies(inputDepsA, inputDeps);
        }
    }
}
