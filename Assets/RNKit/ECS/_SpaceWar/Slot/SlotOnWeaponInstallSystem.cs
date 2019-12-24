using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class SlotOnWeaponInstallServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponUninstallMessage))]
        struct WeaponUninstallJob : IJobForEachWithEntity<WeaponInstalledState>
        {
            public ComponentType SlotUsingState;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity weaponEntity, int index, [ReadOnly]ref WeaponInstalledState weaponInstalledState)
            {
                endCommandBuffer.RemoveComponent(index, weaponInstalledState.slotEntity, SlotUsingState);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new WeaponUninstallJob
            {
                SlotUsingState = typeof(SlotUsingState),

                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }

    }
}
