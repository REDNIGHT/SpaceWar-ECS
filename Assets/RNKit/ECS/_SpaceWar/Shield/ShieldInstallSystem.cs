using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShieldOnWeaponInstallServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponInstallMessage))]
        struct InstallMessageJob : IJobForEach<WeaponShield, WeaponControlInfo, ControlTorqueAngular, WeaponInstalledState>
        {
            [ReadOnly] public ComponentDataFromEntity<ShipShields> shipShieldInfosFromEntity;
            [ReadOnly] public ComponentDataFromEntity<ActorAttribute1<_ShieldLevel>> shieldLevelFromEntity;
            public void Execute(ref WeaponShield shieldWeapon, ref WeaponControlInfo weaponControlData, ref ControlTorqueAngular controlTorqueAngular, [ReadOnly] ref WeaponInstalledState weaponInstalled)
            {
                var shipShieldInfos = shipShieldInfosFromEntity[weaponInstalled.shipEntity];
                var _ShieldLevel = shieldLevelFromEntity[weaponInstalled.shipEntity];
                var shieldLevel = (int)_ShieldLevel.value - 1;

                shieldWeapon.level = (byte)shieldLevel;

                if (shieldLevel < 0)
                    return;

                shipShieldInfos.get(shieldLevel, ref weaponControlData, ref controlTorqueAngular);
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponUninstallMessage))]
        struct UninstallMessageJob : IJobForEach<WeaponShield, WeaponControlInfo, ControlTorqueAngular, WeaponInstalledState>
        {
            [ReadOnly] public ComponentDataFromEntity<ShipShields> shipShieldInfosFromEntity;
            [ReadOnly] public ComponentDataFromEntity<ActorAttribute1<_ShieldLevel>> shieldLevelFromEntity;
            public void Execute(ref WeaponShield shieldWeapon, ref WeaponControlInfo weaponControlData, ref ControlTorqueAngular controlTorqueAngular, [ReadOnly] ref WeaponInstalledState weaponInstalled)
            {
                if (shipShieldInfosFromEntity.Exists(weaponInstalled.shipEntity) == false)
                    return;

                var shipShieldInfos = shipShieldInfosFromEntity[weaponInstalled.shipEntity];
                var _ShieldLevel = shieldLevelFromEntity[weaponInstalled.shipEntity];
                var shieldLevel = (int)_ShieldLevel.value - 1;

                shieldWeapon.level = (byte)shieldLevel;

                if (shieldLevel < 0)
                    return;

                shipShieldInfos.get(shieldLevel, ref weaponControlData, ref controlTorqueAngular);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var shipShieldInfosFromEntity = GetComponentDataFromEntity<ShipShields>(true);
            var shieldLevelFromEntity = GetComponentDataFromEntity<ActorAttribute1<_ShieldLevel>>(true);
            inputDeps = new InstallMessageJob
            {
                shipShieldInfosFromEntity = shipShieldInfosFromEntity,
                shieldLevelFromEntity = shieldLevelFromEntity,
            }
            .Schedule(this, inputDeps);

            inputDeps = new UninstallMessageJob
            {
                shipShieldInfosFromEntity = shipShieldInfosFromEntity,
                shieldLevelFromEntity = shieldLevelFromEntity,
            }
            .Schedule(this, inputDeps);

            return inputDeps;
        }
    }

}
