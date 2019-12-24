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
    public class ShieldControlServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(Shield))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct ShieldControlJobA : IJobForEachWithEntity<Shield_R_Temp, WeaponCreator>
        {
            [ReadOnly] public ComponentDataFromEntity<WeaponInstalledState> weaponInstalledStateFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Rotation> weaponRotationFromEntity;
            public ComponentType OnDestroyMessage;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity shieldEntity, int index, ref Shield_R_Temp TR_Temp, [ReadOnly] ref WeaponCreator weaponCreator)
            {
                if (weaponInstalledStateFromEntity.Exists(weaponCreator.entity))
                {
                    TR_Temp.rotation = weaponRotationFromEntity[weaponCreator.entity].Value;
                }
                else
                {
                    //武器开启自爆模式后 武器被删除 激光应该一起删除
                    //或者武器被卸载
                    endCommandBuffer.AddComponent(index, shieldEntity, OnDestroyMessage);
                }
            }
        }
        [BurstCompile]
        [RequireComponentTag(typeof(Shield))]
        struct ShieldControlJobB : IJobForEach<Rotation, Shield_R_Temp>
        {
            public void Execute(ref Rotation rotation, [ReadOnly] ref Shield_R_Temp shield_R_Temp)
            {
                rotation.Value = shield_R_Temp.rotation;
            }
        }


        EndCommandBufferSystem endBarrier;
        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
            Debug.Assert(endBarrier != null, "endBarrier != null");
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new ShieldControlJobA
            {
                weaponInstalledStateFromEntity = GetComponentDataFromEntity<WeaponInstalledState>(true),
                weaponRotationFromEntity = GetComponentDataFromEntity<Rotation>(true),

                OnDestroyMessage = typeof(OnDestroyMessage),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            inputDeps = new ShieldControlJobB
            {
            }
            .Schedule(this, inputDeps);

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}
