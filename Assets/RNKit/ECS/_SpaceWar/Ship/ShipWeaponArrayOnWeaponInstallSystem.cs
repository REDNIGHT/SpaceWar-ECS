using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipWeaponArrayOnWeaponInstallServerSystem : JobComponentSystem//
    {
        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponInstallMessage))]
        public struct WeaponInstallJob : IJobForEachWithEntity<Weapon, WeaponInstalledState>
        {
            public ComponentDataFromEntity<ShipWeaponArray> shipWeaponArrayFromEntity;

            public void Execute(Entity weaponEntity, int index, [ReadOnly]ref Weapon weapon, [ReadOnly]ref WeaponInstalledState weaponInstalledState)
            {
                if (shipWeaponArrayFromEntity.Exists(weaponInstalledState.shipEntity) == false)//client那边的运行 只有玩家控制的飞船才有ShipWeaponArray
                    return;


                var weaponInstalledArray = shipWeaponArrayFromEntity[weaponInstalledState.shipEntity];

                if (weapon.type == WeaponType.Attack)
                {
                    weaponInstalledArray.SetWeaponEntity(weaponInstalledState.slot.index, weaponEntity);
                }
                else
                {
                    weaponInstalledArray.SetAssistWeapon(weaponInstalledState.slot.index, weaponEntity);
                }

                shipWeaponArrayFromEntity[weaponInstalledState.shipEntity] = weaponInstalledArray;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponUninstallMessage))]
        [ExcludeComponent(typeof(Weapon_OnShipDestroyMessage))]
        public struct WeaponUninstallJob : IJobForEach<Weapon, WeaponInstalledState>
        {
            public ComponentDataFromEntity<ShipWeaponArray> shipWeaponArrayFromEntity;

            public void Execute([ReadOnly]ref Weapon weapon, [ReadOnly]ref WeaponInstalledState weaponInstalledState)
            {
                if (shipWeaponArrayFromEntity.Exists(weaponInstalledState.shipEntity) == false)//client那边的运行 只有玩家控制的飞船才有ShipWeaponArray
                    return;


                var weaponInstalledArray = shipWeaponArrayFromEntity[weaponInstalledState.shipEntity];

                if (weapon.type == WeaponType.Attack)
                {
                    weaponInstalledArray.SetWeaponEntity(weaponInstalledState.slot.index, Entity.Null);
                }
                else
                {
                    weaponInstalledArray.SetAssistWeapon(weaponInstalledState.slot.index, Entity.Null);
                }

                shipWeaponArrayFromEntity[weaponInstalledState.shipEntity] = weaponInstalledArray;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var shipWeaponArrayFromEntity = GetComponentDataFromEntity<ShipWeaponArray>();

            inputDeps = new WeaponUninstallJob
            {
                shipWeaponArrayFromEntity = shipWeaponArrayFromEntity,
            }
            .ScheduleSingle(this, inputDeps);

            inputDeps = new WeaponInstallJob
            {
                shipWeaponArrayFromEntity = shipWeaponArrayFromEntity,
            }
            .ScheduleSingle(this, inputDeps);

            return inputDeps;
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipWeaponArrayOnWeaponInstallClientSystem : JobComponentSystem//
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var shipWeaponArrayFromEntity = GetComponentDataFromEntity<ShipWeaponArray>();

            inputDeps = new ShipWeaponArrayOnWeaponInstallServerSystem.WeaponUninstallJob
            {
                shipWeaponArrayFromEntity = shipWeaponArrayFromEntity,
            }
            .ScheduleSingle(this, inputDeps);

            inputDeps = new ShipWeaponArrayOnWeaponInstallServerSystem.WeaponInstallJob
            {
                shipWeaponArrayFromEntity = shipWeaponArrayFromEntity,
            }
            .ScheduleSingle(this, inputDeps);

            return inputDeps;
        }
    }
}
