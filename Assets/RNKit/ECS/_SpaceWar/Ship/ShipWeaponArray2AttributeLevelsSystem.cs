using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipWeaponArray2AttributeLevelsServerSystem : JobComponentSystem//
    {
        [BurstCompile]
        //[RequireComponentTag(typeof(Ship))]
        public struct AttributeLevelsJob : IJobForEach<ActorAttribute1<_VelocityLevel>, ActorAttribute1<_PowerLevel>, ActorAttribute1<_ShieldLevel>, ShipWeaponArray>
        {
            [ReadOnly] public ComponentDataFromEntity<Weapon> weaponFromEntity;
            public void Execute
                (ref ActorAttribute1<_VelocityLevel> velocityLevel
                , ref ActorAttribute1<_PowerLevel> powerLevel
                , ref ActorAttribute1<_ShieldLevel> shieldLevel
                , [ReadOnly, ChangedFilter] ref ShipWeaponArray shipWeaponArray)
            {
                velocityLevel.value = 0f;
                shieldLevel.value = 0f;
                powerLevel.value = 0f;

                for (int i = 0; i < ShipWeaponArray.AssistWeaponMaxCount; ++i)
                {
                    var assistWeapon = shipWeaponArray.GetAssistWeapon(i);
                    if (assistWeapon == Entity.Null)
                        continue;

                    var w = weaponFromEntity[assistWeapon];

                    if (w.type == WeaponType.Velocity)
                    {
                        velocityLevel.value += 1f;
                    }
                    else if (w.type == WeaponType.Power)
                    {
                        powerLevel.value += 1f;
                    }
                    else if (w.type == WeaponType.Shield)
                    {
                        shieldLevel.value += 1f;
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new AttributeLevelsJob
            {
                weaponFromEntity = GetComponentDataFromEntity<Weapon>(),
            }
            .Schedule(this, inputDeps);
            return inputDeps;
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipWeaponArray2AttributeLevelsClientSystem : JobComponentSystem//
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new ShipWeaponArray2AttributeLevelsServerSystem.AttributeLevelsJob
            {
                weaponFromEntity = GetComponentDataFromEntity<Weapon>(),
            }
            .Schedule(this, inputDeps);
            return inputDeps;
        }
    }
}
