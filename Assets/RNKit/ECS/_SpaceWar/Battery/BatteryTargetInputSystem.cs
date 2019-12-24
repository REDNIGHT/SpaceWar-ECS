using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class BatteryTargetInputServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;

        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        //[BurstCompile]
        //[RequireComponentTag(typeof(BatteryFindTarget))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct FireInputJob : IJobForEachWithEntity<FindTraceTarget, TracePoint, ShipWeaponArray>
        {
            public EntityCommandBuffer.Concurrent endCommandBuffer;

            public void Execute(Entity _, int index, [ReadOnly, ChangedFilter]ref FindTraceTarget findTarget, [ReadOnly, ChangedFilter]ref TracePoint targetPoint, [ReadOnly]ref ShipWeaponArray shipWeaponArray)
            {
                if (findTarget.targetEntity == default)
                    return;

                for (var i = 0; i < ShipWeaponArray.WeaponMaxCount; ++i)
                {
                    var weaponEntity = shipWeaponArray.GetWeaponEntity(i);
                    if (weaponEntity == default)
                        continue;

                    if (weaponEntity != Entity.Null)
                    {
                        endCommandBuffer.SetComponent(index, weaponEntity, new WeaponInput { fireType = FireType.Fire, firePosition = targetPoint.value });
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new FireInputJob
            {
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}
