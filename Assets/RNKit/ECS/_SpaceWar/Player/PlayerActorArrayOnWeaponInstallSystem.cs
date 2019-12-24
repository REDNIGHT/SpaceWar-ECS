using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerActorArrayOnWeaponInstallServerSystem : JobComponentSystem//
    {
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }


        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponInstallMessage))]
        struct WeaponInstallJob : IJobForEachWithEntity<Weapon, WeaponInstalledState>
        {
            [ReadOnly] public ComponentDataFromEntity<ActorOwner> actorOwnerFromEntity;
            public ComponentDataFromEntity<PlayerActorArray> playerActorArrayFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Slot> slotFromEntity;

            public void Execute(Entity weaponEntity, int index, [ReadOnly]ref Weapon weapon, [ReadOnly]ref WeaponInstalledState weaponInstalledState)
            {
                var actorOwner = actorOwnerFromEntity[weaponInstalledState.shipEntity];

                if (playerActorArrayFromEntity.Exists(actorOwner.playerEntity) == false)//炮台没有PlayerActorArray
                    return;

                var playerActorArray = playerActorArrayFromEntity[actorOwner.playerEntity];
                var weaponSlot = slotFromEntity[weaponInstalledState.slotEntity];

                if (weapon.type == WeaponType.Attack)
                {
                    playerActorArray.SetWeaponEntity(weaponSlot.index, weaponEntity);
                }
                else
                {
                    playerActorArray.SetAssistWeaponEntity(weaponSlot.index, weaponEntity);

                    if (weapon.type == WeaponType.Shield)
                    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                        if (playerActorArray.GetShieldWeaponEntity(weaponSlot.index) != Entity.Null)
                            throw new System.Exception($"playerActorArray.GetShieldWeaponEntity({weaponSlot.index}) != Entity.Null");
                        //Debug.Assert(playerActorArray.GetShieldWeaponEntity(weaponSlot.slotIndex) == Entity.Null, $"playerActorArray.GetShieldWeaponEntity({weaponSlot.slotIndex}) == Entity.Null");
#endif
                        playerActorArray.SetShieldWeaponEntity(weaponSlot.index, weaponEntity);

                        if (playerActorArray.curShieldWeaponEntity == Entity.Null)
                        {
                            playerActorArray.curShieldWeaponEntity = weaponEntity;
                        }
                    }
                }

                playerActorArrayFromEntity[actorOwner.playerEntity] = playerActorArray;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponUninstallMessage))]
        [ExcludeComponent(typeof(Weapon_OnShipDestroyMessage))]
        struct WeaponUninstallJob : IJobForEachWithEntity<Weapon, WeaponInstalledState>
        {
            [ReadOnly] public ComponentDataFromEntity<ActorOwner> actorOwnerFromEntity;
            public ComponentDataFromEntity<PlayerActorArray> playerActorArrayFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Slot> slotFromEntity;

            public void Execute(Entity weaponEntity, int index, [ReadOnly]ref Weapon weapon, [ReadOnly]ref WeaponInstalledState weaponInstalledState)
            {
                var actorOwner = actorOwnerFromEntity[weaponInstalledState.shipEntity];

                if (playerActorArrayFromEntity.Exists(actorOwner.playerEntity) == false)//炮台没有PlayerActorArray
                    return;

                var playerActorArray = playerActorArrayFromEntity[actorOwner.playerEntity];
                var weaponSlot = slotFromEntity[weaponInstalledState.slotEntity];

                if (weapon.type == WeaponType.Attack)
                {
                    playerActorArray.SetWeaponEntity(weaponSlot.index, Entity.Null);
                }
                else
                {
                    playerActorArray.SetAssistWeaponEntity(weaponSlot.index, Entity.Null);

                    if (weapon.type == WeaponType.Shield)
                    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                        if (playerActorArray.GetShieldWeaponEntity(weaponSlot.index) == Entity.Null)
                            throw new System.Exception($"playerActorArray.GetShieldWeaponEntity({weaponSlot.index}) == Entity.Null");
                        //Debug.Assert(playerActorArray.GetShieldWeaponEntity(weaponSlot.slotIndex) != Entity.Null, $"playerActorArray.GetShieldWeaponEntity({weaponSlot.slotIndex}) != Entity.Null");
#endif

                        playerActorArray.SetShieldWeaponEntity(weaponSlot.index, Entity.Null);


                        if (playerActorArray.curShieldWeaponEntity == weaponEntity)
                        {
                            playerActorArray.curShieldWeaponEntity = Entity.Null;

                            for (var i = 0; i < PlayerActorArray.ShieldWeaponMaxCount; ++i)
                            {
                                var shieldWeaponEntity = playerActorArray.GetShieldWeaponEntity(i);
                                if (shieldWeaponEntity == Entity.Null)
                                    continue;

                                playerActorArray.curShieldWeaponEntity = shieldWeaponEntity;
                                break;
                            }
                        }
                    }
                }


                playerActorArrayFromEntity[actorOwner.playerEntity] = playerActorArray;
            }
        }


        [BurstCompile]
        [RequireComponentTag(typeof(Player_OnShipDestroyMessage))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct ShipDestroyMessageJob : IJobForEachWithEntity<PlayerActorArray>
        {
            public ComponentType Player_OnShipDestroy_NextFrameMessage;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity playerEntity, int index, ref PlayerActorArray playerActorArray)
            {
                playerActorArray = default;

                endCommandBuffer.RemoveComponent(index, playerEntity, Player_OnShipDestroy_NextFrameMessage);

                //
                /*playerActorArray.shipEntity = Entity.Null;

                //
                if (playerActorArray.shieldEntity != Entity.Null)
                {
                    playerActorArray.shieldEntity = Entity.Null;
                }

                //
                for (var i = PlayerActorArray.WeaponBegin; i <= PlayerActorArray.AssistWeaponEnd; ++i)
                {
                    var weaponEntity = playerActorArray[i];
                    if (weaponEntity == Entity.Null)
                        continue;

                    playerActorArray[i] = default;
                }*/
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var actorOwnerFromEntity = GetComponentDataFromEntity<ActorOwner>(true);
            var playerActorArrayFromEntity = GetComponentDataFromEntity<PlayerActorArray>();
            var slotFromEntity = GetComponentDataFromEntity<Slot>(true);

            inputDeps = new WeaponUninstallJob
            {
                actorOwnerFromEntity = actorOwnerFromEntity,
                playerActorArrayFromEntity = playerActorArrayFromEntity,
                slotFromEntity = slotFromEntity,
            }
            .ScheduleSingle(this, inputDeps);

            inputDeps = new WeaponInstallJob
            {
                actorOwnerFromEntity = actorOwnerFromEntity,
                playerActorArrayFromEntity = playerActorArrayFromEntity,
                slotFromEntity = slotFromEntity,
            }
            .ScheduleSingle(this, inputDeps);


            inputDeps = new ShipDestroyMessageJob
            {
                Player_OnShipDestroy_NextFrameMessage = typeof(Player_OnShipDestroyMessage),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }

}
