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
    public class WeaponOnInstallServerSystem : JobComponentSystem
    {
        public float uninstallForce = 5f;
        public float uninstallTorque = 1f;
        public float uninstallInputTimeScale = 2.5f;

        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }


        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponUninstallMessage))]
        struct WeaponUninstallJobA : IJobForEach<WeaponControl>
        {
            //public float uninstallInputTimeScale;

            //public EntityCommandBuffer.Concurrent endCommandBuffer;

            public void Execute(/*[ReadOnly]ref WeaponInstalledState weaponInstalledState,*/ ref WeaponControl weaponControl)
            {
#if false
                //todo...  排除死亡的ship
                if (weaponControl.inputTime > 0f)
                {
                    weaponControl.inputTime *= uninstallInputTimeScale;

                    endCommandBuffer.AddComponent(index, weaponEntity, new WeaponExplosionSelf
                    {
                        lastShipActorOwner = weaponInstalledState.shipActorOwner,
                        lastShipEntity = weaponInstalledState.shipEntity
                    });
                    endCommandBuffer.AddComponent(index, weaponEntity, new ActorLifetime
                    {
                        lifetime = weaponControl.inputTime + 1f,
                        value = weaponControl.inputTime + 1f,
                        needSyncOnDestroy = true,
                    });
                }
#endif

                weaponControl.reset();
            }
        }


        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponUninstallMessage))]
        struct WeaponUninstallJobB : IJobForEachWithEntity<WeaponAttribute>
        {
            public ComponentType WeaponInstalledState;
            public ComponentType OnDestroyMessage;

            public EntityCommandBuffer.Concurrent endCommandBuffer;

            public void Execute(Entity weaponEntity, int index, [ReadOnly]ref WeaponAttribute weaponAttribute)
            {
                endCommandBuffer.RemoveComponent(index, weaponEntity, WeaponInstalledState);

                if (weaponAttribute.itemCount == 0)
                {
                    endCommandBuffer.AddComponent(index, weaponEntity, OnDestroyMessage);
                }
            }
        }


        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponUninstallMessage))]
        struct WeaponUninstallJobC : IJobForEach<WeaponInput, Translation, RigidbodyForce>
        {
            public float uninstallForce;

            public void Execute([ReadOnly]ref WeaponInput weaponInput, [ReadOnly] ref Translation translation, ref RigidbodyForce rigidbodyForce)
            {
                //
                if (weaponInput.lastFireType == FireType.Uninstall)
                {
                    var direction = weaponInput.firePosition - translation.Value;
                    //卸载时 扔远些 避免重新安装
                    //fireDirection = math.normalize(fireDirection);
                    rigidbodyForce.force += direction * uninstallForce;
                }
            }
        }


        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponUninstallMessage))]
        struct WeaponUninstallJobD : IJobForEach<RigidbodyVelocity, ControlTorqueDirection, RigidbodyTorque>
        {
            public float uninstallTorque;
            public void Execute([ReadOnly] ref RigidbodyVelocity rigidbodyVelocity,
                ref ControlTorqueDirection controlTorqueDirection, ref RigidbodyTorque rigidbodyTorque)
            {
                controlTorqueDirection.direction = default;

                //
                //rigidbodyTorque.torque += new float3(0f, random.NextFloat(-uninstallTorque, uninstallTorque), 0f);
                rigidbodyTorque.torque += rigidbodyVelocity.angular.y * uninstallTorque;
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var inputDepsA = new WeaponUninstallJobA
            {
                //uninstallInputTimeScale = uninstallInputTimeScale,
                //endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            var inputDepsB = new WeaponUninstallJobB
            {
                WeaponInstalledState = typeof(WeaponInstalledState),
                OnDestroyMessage = typeof(OnDestroyMessage),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            var inputDepsAB = JobHandle.CombineDependencies(inputDepsA, inputDepsB);
            endBarrier.AddJobHandleForProducer(inputDepsAB);

            var inputDepsC = new WeaponUninstallJobC
            {
                uninstallForce = uninstallForce,
            }
            .Schedule(this, inputDeps);

            var inputDepsD = new WeaponUninstallJobD
            {
                uninstallTorque = uninstallTorque,
            }
            .Schedule(this, inputDeps);


            inputDeps = JobHandle.CombineDependencies(inputDepsAB, inputDepsC, inputDepsD);
            return inputDeps;
        }
    }
}
