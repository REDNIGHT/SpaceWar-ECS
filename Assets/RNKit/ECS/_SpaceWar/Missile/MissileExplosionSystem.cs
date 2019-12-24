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
    public class MissileExplosionServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;

        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OnPhysicsCallMessage), typeof(Missile))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct AutoExplosionByRaycastJob : IJobForEachWithEntity_EB<PhysicsRaycastResults>
        {
            public ComponentType OnDestroyMessage;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity missileEntity, int index,
                [ReadOnly, ChangedFilter]DynamicBuffer<PhysicsRaycastResults> raycastResults)
            {
                if (raycastResults.Length <= 0)
                    return;

                endCommandBuffer.AddComponent(index, missileEntity, OnDestroyMessage);
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(MissileAutoExplosionByTouche))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct AutoExplosionByTouchJob : IJobForEachWithEntity_EB<PhysicsTriggerResults>
        {
            public ComponentType OnDestroyMessage;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity missileEntity, int index,
                [ReadOnly, ChangedFilter]DynamicBuffer<PhysicsTriggerResults> physicsTriggerResults)
            {
                if (physicsTriggerResults.Length <= 0)
                    return;

                endCommandBuffer.AddComponent(index, missileEntity, OnDestroyMessage);
            }
        }


        [BurstCompile]
        [RequireComponentTag(typeof(MissileAutoExplosionByAngle))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct AutoExplosionByAngleJob : IJobForEachWithEntity<TraceDirectionData>
        {
            public ComponentType OnDestroyMessage;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity missileEntity, int index, [ReadOnly] ref TraceDirectionData missileTrace)
            {
                if (missileTrace.targetAngleOffset > missileTrace.lastTargetAngleOffset + 15f)
                {
                    endCommandBuffer.AddComponent(index, missileEntity, OnDestroyMessage);
                    return;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var inputDepsA = new AutoExplosionByRaycastJob
            {
                OnDestroyMessage = typeof(OnDestroyMessage),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);

            var inputDepsB = new AutoExplosionByAngleJob
            {
                OnDestroyMessage = typeof(OnDestroyMessage),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);

            var inputDepsC = new AutoExplosionByTouchJob
            {
                OnDestroyMessage = typeof(OnDestroyMessage),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);

            var ds = JobHandle.CombineDependencies(inputDepsA, inputDepsB, inputDepsC);
            endBarrier.AddJobHandleForProducer(ds);

            return ds;
        }
    }
}
