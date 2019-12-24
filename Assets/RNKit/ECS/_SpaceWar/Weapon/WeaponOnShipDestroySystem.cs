using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class WeaponOnShipDestroyServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Weapon_OnShipDestroyMessage))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct WeaponByShipDestroyJob : IJobForEachWithEntity<WeaponAttribute>
        {
            public ComponentType OnDestroyMessage;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity weaponEntity, int index, ref WeaponAttribute weaponAttribute)
            {
                weaponAttribute.hp -= 1;
                if (weaponAttribute.hp == 0 || weaponAttribute.itemCount == 0)
                {
                    endCommandBuffer.AddComponent(index, weaponEntity, OnDestroyMessage);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new WeaponByShipDestroyJob
            {
                OnDestroyMessage = typeof(OnDestroyMessage),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);



            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}
