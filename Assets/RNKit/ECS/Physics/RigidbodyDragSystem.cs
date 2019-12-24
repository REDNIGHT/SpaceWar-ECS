using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RN.Network
{
    /// <summary>
    /// 增加一层drag效果 和Rigidbody.Drag一起叠加效果
    /// </summary>
    public struct RigidbodyLinearDragChange : IComponentData
    {
        public float drag;
        public float tempValue;
    }

    /// <summary>
    /// 同上
    /// </summary>
    public struct RigidbodyAngularDragChange : IComponentData
    {
        public float drag;
        public float tempValue;
    }

    //public struct RigidbodyAutoSleep : IComponentData { }



    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class RigidbodyDragSystem : JobComponentSystem
    {
        /*[BurstCompile]
        [RequireComponentTag(typeof(RigidbodyAutoSleep))]
        struct VelocitySleepJob : IJobForEach<RigidbodyVelocity>
        {
            public void Execute(ref RigidbodyVelocity rigidbodyVelocity)
            {
                if (math.lengthsq(rigidbodyVelocity.linear) < 0.005f)
                {
                    rigidbodyVelocity.linear = float3.zero;
                }
                if (math.lengthsq(rigidbodyVelocity.angular) < 0.05f)
                {
                    rigidbodyVelocity.angular = float3.zero;
                }
            }
        }*/


        [BurstCompile]
        struct LinearCurDragJob : IJobForEach<RigidbodyLinearDragChange>
        {
            public float fixedDeltaTime;
            public void Execute(ref RigidbodyLinearDragChange rigidbodyLinearDragChange)
            {
                rigidbodyLinearDragChange.tempValue = math.max(0f, 1f - rigidbodyLinearDragChange.drag * fixedDeltaTime);
            }
        }
        [BurstCompile]
        struct AngularCurDragJob : IJobForEach<RigidbodyAngularDragChange>
        {
            public float fixedDeltaTime;
            public void Execute(ref RigidbodyAngularDragChange rigidbodyAngularDragChange)
            {
                rigidbodyAngularDragChange.tempValue = math.max(0f, 1f - rigidbodyAngularDragChange.drag * fixedDeltaTime);
            }
        }

        [BurstCompile]
        struct LinearDragJob : IJobForEach<RigidbodyVelocity, RigidbodyLinearDragChange>
        {
            public void Execute(ref RigidbodyVelocity rigidbodyVelocity, [ReadOnly]ref RigidbodyLinearDragChange rigidbodyLinearDragChange)
            {
                rigidbodyVelocity.linear *= rigidbodyLinearDragChange.tempValue;
            }
        }
        [BurstCompile]
        struct ForceDragJob : IJobForEach<RigidbodyForce, RigidbodyLinearDragChange>
        {
            public void Execute(ref RigidbodyForce rigidbodyForce, [ReadOnly]ref RigidbodyLinearDragChange rigidbodyLinearDragChange)
            {
                rigidbodyForce.force *= rigidbodyLinearDragChange.tempValue;
            }
        }
        [BurstCompile]
        struct AngularDragJob : IJobForEach<RigidbodyVelocity, RigidbodyAngularDragChange>
        {
            public void Execute(ref RigidbodyVelocity rigidbodyVelocity, [ReadOnly]ref RigidbodyAngularDragChange rigidbodyAngularDragChange)
            {
                rigidbodyVelocity.angular *= rigidbodyAngularDragChange.tempValue;
            }
        }
        [BurstCompile]
        struct TorqueDragJob : IJobForEach<RigidbodyTorque, RigidbodyAngularDragChange>
        {
            public void Execute(ref RigidbodyTorque rigidbodyTorque, [ReadOnly]ref RigidbodyAngularDragChange rigidbodyAngularDragChange)
            {
                rigidbodyTorque.torque *= rigidbodyAngularDragChange.tempValue;
            }
        }


        [BurstCompile]
        struct RBsLinearDragJob : IJobForEach_BC<PhysicsResults, RigidbodyLinearDragChange>
        {
            public ComponentDataFromEntity<RigidbodyVelocity> rigidbodyVelocityFromEntity;
            public void Execute([ReadOnly]DynamicBuffer<PhysicsResults> rigidbodyResults, [ReadOnly]ref RigidbodyLinearDragChange rigidbodyLinearDragChange)
            {
                for (var i = 0; i < rigidbodyResults.Length; ++i)
                {
                    var rigidbodyResult = rigidbodyResults[i];

                    if (rigidbodyVelocityFromEntity.Exists(rigidbodyResult.entity) == false)
                        continue;

                    var rigidbodyVelocity = rigidbodyVelocityFromEntity[rigidbodyResult.entity];

                    rigidbodyVelocity.linear *= rigidbodyLinearDragChange.tempValue;

                    rigidbodyVelocityFromEntity[rigidbodyResult.entity] = rigidbodyVelocity;
                }
            }
        }
        [BurstCompile]
        struct RBsForceDragJob : IJobForEach_BC<PhysicsResults, RigidbodyLinearDragChange>
        {
            public ComponentDataFromEntity<RigidbodyForce> rigidbodyForceFromEntity;
            public void Execute([ReadOnly]DynamicBuffer<PhysicsResults> rigidbodyResults, [ReadOnly]ref RigidbodyLinearDragChange rigidbodyLinearDragChange)
            {
                for (var i = 0; i < rigidbodyResults.Length; ++i)
                {
                    var rigidbodyResult = rigidbodyResults[i];

                    if (rigidbodyForceFromEntity.Exists(rigidbodyResult.entity) == false)
                        continue;

                    var rigidbodyForce = rigidbodyForceFromEntity[rigidbodyResult.entity];

                    rigidbodyForce.force *= rigidbodyLinearDragChange.tempValue;

                    rigidbodyForceFromEntity[rigidbodyResult.entity] = rigidbodyForce;
                }
            }
        }

        [BurstCompile]
        struct RBsAngularDragJob : IJobForEach_BC<PhysicsResults, RigidbodyAngularDragChange>
        {
            public ComponentDataFromEntity<RigidbodyVelocity> rigidbodyVelocityFromEntity;
            public void Execute([ReadOnly]DynamicBuffer<PhysicsResults> rigidbodyResults, [ReadOnly]ref RigidbodyAngularDragChange rigidbodyAngularDragChange)
            {
                for (var i = 0; i < rigidbodyResults.Length; ++i)
                {
                    var rigidbodyResult = rigidbodyResults[i];

                    if (rigidbodyVelocityFromEntity.Exists(rigidbodyResult.entity) == false)
                        continue;

                    var rigidbodyVelocity = rigidbodyVelocityFromEntity[rigidbodyResult.entity];

                    rigidbodyVelocity.angular *= rigidbodyAngularDragChange.tempValue;

                    rigidbodyVelocityFromEntity[rigidbodyResult.entity] = rigidbodyVelocity;
                }
            }
        }
        [BurstCompile]
        struct RBsTorqueDragJob : IJobForEach_BC<PhysicsResults, RigidbodyAngularDragChange>
        {
            public ComponentDataFromEntity<RigidbodyTorque> rigidbodyTorqueFromEntity;
            public void Execute([ReadOnly]DynamicBuffer<PhysicsResults> rigidbodyResults, [ReadOnly]ref RigidbodyAngularDragChange rigidbodyAngularDragChange)
            {
                for (var i = 0; i < rigidbodyResults.Length; ++i)
                {
                    var rigidbodyResult = rigidbodyResults[i];

                    if (rigidbodyTorqueFromEntity.Exists(rigidbodyResult.entity) == false)
                        continue;

                    var rigidbodyTorque = rigidbodyTorqueFromEntity[rigidbodyResult.entity];

                    rigidbodyTorque.torque *= rigidbodyAngularDragChange.tempValue;

                    rigidbodyTorqueFromEntity[rigidbodyResult.entity] = rigidbodyTorque;
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var fixedDeltaTime = Time.fixedDeltaTime;
            var rigidbodyVelocityFromEntity = GetComponentDataFromEntity<RigidbodyVelocity>();
            var rigidbodyForceFromEntity = GetComponentDataFromEntity<RigidbodyForce>();
            var rigidbodyTorqueFromEntity = GetComponentDataFromEntity<RigidbodyTorque>();

            //
            var linearCurDragInputDeps = new LinearCurDragJob { fixedDeltaTime = fixedDeltaTime, }.Schedule(this, inputDeps);
            var angularCurDragInputDeps = new AngularCurDragJob { fixedDeltaTime = fixedDeltaTime, }.Schedule(this, inputDeps);
            inputDeps = JobHandle.CombineDependencies(linearCurDragInputDeps, angularCurDragInputDeps);

            //
            var forceInputDeps = new ForceDragJob { }.Schedule(this, inputDeps);
            forceInputDeps = new RBsForceDragJob
            {
                rigidbodyForceFromEntity = rigidbodyForceFromEntity,
            }
            .ScheduleSingle(this, forceInputDeps);

            //
            var torqueInputDeps = new TorqueDragJob { }.Schedule(this, inputDeps);
            torqueInputDeps = new RBsTorqueDragJob
            {
                rigidbodyTorqueFromEntity = rigidbodyTorqueFromEntity,
            }
            .ScheduleSingle(this, torqueInputDeps);


            //
            inputDeps = new LinearDragJob { }.Schedule(this, inputDeps);
            inputDeps = new AngularDragJob { }.Schedule(this, inputDeps);
            inputDeps = new RBsLinearDragJob
            {
                rigidbodyVelocityFromEntity = rigidbodyVelocityFromEntity,
            }
            .ScheduleSingle(this, inputDeps);
            inputDeps = new RBsAngularDragJob
            {
                rigidbodyVelocityFromEntity = rigidbodyVelocityFromEntity,
            }
            .ScheduleSingle(this, inputDeps);
            //inputDeps = new VelocitySleepJob { }.Schedule(this, inputDeps);


            //
            return JobHandle.CombineDependencies(forceInputDeps, torqueInputDeps, inputDeps);
        }
    }
}

