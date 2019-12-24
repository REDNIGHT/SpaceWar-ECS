using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class FindTraceTargetSystem : JobComponentSystem
    {
        [BurstCompile]
        //[RequireComponentTag(typeof(OnCallMessage))]
        [ExcludeComponent(typeof(OnDestroyMessage), typeof(TraceDirectionData))]
        struct FindTargetJobA : IJobForEach_BC<PhysicsTriggerResults, FindTraceTarget>
        {
            public void Execute([ReadOnly, ChangedFilter]DynamicBuffer<PhysicsTriggerResults> physicsTriggerResults, ref FindTraceTarget findTarget)
            {
                if (findTarget.targetEntity == default)
                {
                    if (physicsTriggerResults.Length > 0)
                    {
                        findTarget.targetEntity = physicsTriggerResults[0].entity;
                    }
                }
                else if (findTarget.lostTargetEnable)
                {
                    if (physicsTriggerResults.Length == 0)
                    {
                        findTarget.targetEntity = default;
                    }
                }
            }
        }

        [BurstCompile]
        //[RequireComponentTag(typeof(OnCallMessage))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct FindTargetJobB : IJobForEach_BCC<PhysicsTriggerResults, FindTraceTarget, TraceDirectionData>
        {
            public void Execute([ReadOnly, ChangedFilter]DynamicBuffer<PhysicsTriggerResults> physicsTriggerResults, ref FindTraceTarget findTarget, ref TraceDirectionData traceDirection)
            {
                if (findTarget.targetEntity == default)
                {
                    if (physicsTriggerResults.Length > 0)
                    {
                        findTarget.targetEntity = physicsTriggerResults[0].entity;
                        traceDirection.enable = true;
                    }
                }
                else if (findTarget.lostTargetEnable)
                {
                    if (physicsTriggerResults.Length == 0)
                    {
                        findTarget.targetEntity = default;
                        traceDirection.enable = false;
                    }
                }
            }
        }


        [BurstCompile]
        //[RequireComponentTag(typeof(OnCallMessage))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct FindTargetPositionJob : IJobForEachWithEntity<FindTraceTarget, TracePoint>
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;
            [ReadOnly] public ComponentDataFromEntity<RigidbodyVelocity> rigidbodyVelocityFromEntity;

            public void Execute(Entity entity, int index, [ReadOnly]ref FindTraceTarget findTarget, ref TracePoint tracePoint)
            {
                if (findTarget.targetEntity == default)
                    return;
                if (translationFromEntity.Exists(findTarget.targetEntity) == false)
                    return;

                tracePoint.value = translationFromEntity[findTarget.targetEntity].Value;
                tracePoint.value += rigidbodyVelocityFromEntity[findTarget.targetEntity].linear * findTarget.targetVelocityScale;
            }
        }

        [BurstCompile]
        //[RequireComponentTag(typeof(OnCallMessage))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct TraceDirectionJob : IJobForEach<TraceDirection, TraceDirectionData, TracePoint, Translation, Rotation>
        {
            public void Execute(ref TraceDirection traceDirection, ref TraceDirectionData traceDirectionData, [ReadOnly] ref TracePoint targetPosition, [ReadOnly] ref Translation translation, [ReadOnly] ref Rotation rotation)
            {
                if (traceDirectionData.enable == false)
                    return;

                traceDirection.value = math.normalize(targetPosition.value - translation.Value);

                traceDirectionData.lastTargetAngleOffset = traceDirectionData.targetAngleOffset;
                traceDirectionData.targetAngleOffset = Vector3.Angle(math.forward(rotation.Value), targetPosition.value - translation.Value);

                //
                if (traceDirectionData.cancelOnGotoTargetPoint)
                {
                    if (traceDirectionData.targetAngleOffset > traceDirectionData.lastTargetAngleOffset + traceDirectionData.cancelAngle)
                    {
                        traceDirectionData.enable = false;
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new FindTargetJobA
            {
            }.Schedule(this, inputDeps);

            inputDeps = new FindTargetJobB
            {
            }.Schedule(this, inputDeps);

            inputDeps = new FindTargetPositionJob
            {
                translationFromEntity = GetComponentDataFromEntity<Translation>(true),
                rigidbodyVelocityFromEntity = GetComponentDataFromEntity<RigidbodyVelocity>(true),
            }.Schedule(this, inputDeps);

            inputDeps = new TraceDirectionJob
            {
            }.Schedule(this, inputDeps);

            return inputDeps;
        }
    }
}
