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
    public class WeaponControlServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        ControlCommandBufferServerSystem controlBarrier;

        //protected override void OnCreate()
        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
            controlBarrier = World.GetExistingSystem<ControlCommandBufferServerSystem>();
        }

        [BurstCompile]
        [RequireComponentTag(typeof(WeaponInstalledState))]
        struct UninstallJob : IJobForEachWithEntity<WeaponInput>
        {
            //public Unity.Mathematics.Random random;
            public ComponentType OnWeaponUninstallMessage;
            public EntityCommandBuffer.Concurrent ctrlCommandBuffer;
            public EntityCommandBuffer.Concurrent endCommandBuffer;

            [ReadOnly] public ComponentDataFromEntity<Weapon_OnShipDestroyMessage> Weapon_OnShipDestroyMessageFromEntity;


            public void Execute(Entity weaponEntity, int index, [ChangedFilter] ref WeaponInput weaponInput)
            {
                var b = false;
                if (weaponInput.fireType == FireType.Uninstall)
                {
                    weaponInput.lastFireType = weaponInput.fireType;
                    weaponInput.fireType = FireType.None;

                    b = true;
                }

                //有ChangedFilter情况下 下面代码也能执行 估计是添加Weapon_OnShipDestroyMessage后 所有IComponentData的版本都会变化
                if (Weapon_OnShipDestroyMessageFromEntity.Exists(weaponEntity))
                {
                    b = true;
                }


                if (b)
                {
                    ctrlCommandBuffer.AddComponent(index, weaponEntity, OnWeaponUninstallMessage);
                    endCommandBuffer.RemoveComponent(index, weaponEntity, OnWeaponUninstallMessage);
                }
            }
        }


        [BurstCompile]
        //[RequireComponentTag(typeof(WeaponInstalledState))]
        struct FirePrepareJob : IJobForEach<WeaponInput, WeaponControl, WeaponInstalledState, WeaponControlInfo>
        {
            [ReadOnly] public ComponentDataFromEntity<ActorAttribute3<_Power>> powerFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;

            public void Execute([ChangedFilter] ref WeaponInput weaponInput, ref WeaponControl weaponControl, ref WeaponInstalledState weaponInstalledState, [ReadOnly]ref WeaponControlInfo weaponControlInfo)
            {
                if (weaponInput.fireType == FireType.Fire)
                {
                    var shipPower = powerFromEntity[weaponInstalledState.shipEntity];
                    if (shipPower.value >= weaponControlInfo.GetConsumePower(weaponInstalledState.slot.main))
                    {
                        float3 position;
                        if (weaponControlInfo.fireDirectionByShip)
                            position = translationFromEntity[weaponInstalledState.shipEntity].Value;
                        else
                            position = translationFromEntity[weaponInstalledState.slotEntity].Value;

                        weaponInstalledState.fireDirection = math.normalize(weaponInput.firePosition - position);
                        weaponInstalledState.calculateLocalRotation = true;


                        weaponControl.DoFire(weaponControlInfo);
                    }

                    //
                    weaponInput.lastFireType = weaponInput.fireType;
                    weaponInput.fireType = FireType.None;
                }
            }
        }


        [BurstCompile]
        [RequireComponentTag(typeof(RigidbodyTorque))]
        [ExcludeComponent(typeof(Weapon_OnShipDestroyMessage))]
        struct FireDirectionJob : IJobForEachWithEntity<WeaponInstalledState, ControlTorqueDirection>
        {
            [ReadOnly] public ComponentDataFromEntity<Rotation> slotRotationFromEntity;
            [ReadOnly] public ComponentDataFromEntity<WeaponControl> weaponControlFromEntity;
            //[ReadOnly] public ComponentDataFromEntity<RigidbodyVelocity> rigidbodyVelocityFromEntity;

            public void Execute(Entity weaponEntity, int index, ref WeaponInstalledState weaponInstalledState, ref ControlTorqueDirection followTorqueDirection)
            {
                var slotRotation = slotRotationFromEntity[weaponInstalledState.slotEntity].Value;

                //
                if (weaponControlFromEntity.Exists(weaponEntity) && weaponControlFromEntity[weaponEntity].inFire)//Velocity和Power是没有WeaponControl的
                {
                    var fireDirection = weaponInstalledState.fireDirection;

                    if (weaponInstalledState.slot.aimType == AimType.AimWithShip)
                    {
                        if (weaponInstalledState.calculateLocalRotation)
                        {
                            weaponInstalledState.calculateLocalRotation = false;

                            var fireRotation = quaternion.LookRotation(fireDirection, new float3(0f, 1f, 0f));

                            var halfAngle = Quaternion.Angle(slotRotation, fireRotation);

                            //超过限制角度的1.5倍 就恢复到slop的角度
                            if (halfAngle > weaponInstalledState.slot.halfAngleLimitMax)
                            {
                                fireRotation = slotRotation;
                            }
                            else if (halfAngle > weaponInstalledState.slot.halfAngleLimitMin)
                            {
                                fireRotation = Quaternion.RotateTowards(slotRotation, fireRotation, weaponInstalledState.slot.halfAngleLimitMin * 0.999f);//0.999f是避免误差 重新算完后的Quaternion.Angle(slotRotation, fireRotation)有可能还是超过slotHalfAngleLimit一丢丢
                            }
                            weaponInstalledState.fireLocalRotation = math.mul(fireRotation, math.inverse(slotRotation));
                        }


                        var rot = math.mul(slotRotation, weaponInstalledState.fireLocalRotation);
                        fireDirection = math.forward(rot);
                    }


                    followTorqueDirection.direction = fireDirection;
                }
                else
                {
                    followTorqueDirection.direction = math.forward(slotRotation);
                }
            }
        }


        /*[BurstCompile]
        //[RequireComponentTag(typeof(WeaponInstalledState))]
        [ExcludeComponent(typeof(Weapon_OnShipDestroyMessage))]
        struct FollowShipJob : IJobForEachWithEntity<ControlForceDirection, WeaponInstalledState>
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;

            public void Execute(Entity weaponEntity, int index, ref ControlForceDirection controlForceDirection, [ReadOnly]ref WeaponInstalledState weaponInstalledState)
            {
                var slotPosition = translationFromEntity[weaponInstalledState.slotEntity].Value;
                var weaponPosition = translationFromEntity[weaponEntity].Value;

                controlForceDirection.direction = slotPosition - weaponPosition;
            }
        }*/

        //[BurstCompile]
        [ExcludeComponent(typeof(Weapon_OnShipDestroyMessage))]
        struct FireJobA : IJobForEachWithEntity<WeaponInput, WeaponControlInfo, WeaponInstalledState, WeaponControl, WeaponAttribute>
        {
            public float fixedDeltaTime;
            public ComponentType OnWeaponControlFirePrepareMessage;
#if false
            public ComponentType OnWeaponControlFireOnMessage;
#endif
            public EntityCommandBuffer.Concurrent ctrlCommandBuffer;
            public EntityCommandBuffer.Concurrent endCommandBuffer;

#if false
            public SampleCommandBuffer<WeaponControlInFireState>.Concurrent weaponControlInFireStateCommandBuffer;
#endif

            public void Execute(Entity weaponEntity, int index,
                [ReadOnly]ref WeaponInput weaponInput, [ReadOnly]ref WeaponControlInfo weaponControlInfo, [ReadOnly]ref WeaponInstalledState weaponInstalledState,
                ref WeaponControl weaponControl, ref WeaponAttribute weaponAttribute)
            {
                var fireEvent = weaponControl.OnFire(fixedDeltaTime, weaponControlInfo);


                if (fireEvent == WeaponControl.FireEvent.Prepare)
                {
                    ctrlCommandBuffer.AddComponent(index, weaponEntity, OnWeaponControlFirePrepareMessage);
                    endCommandBuffer.RemoveComponent(index, weaponEntity, OnWeaponControlFirePrepareMessage);
                }
                else if (fireEvent == WeaponControl.FireEvent.Fire)
                {
                    if (weaponAttribute.itemCount == 0)
                        return;
                    if (weaponAttribute.itemCount > 0)
                        --weaponAttribute.itemCount;


                    ctrlCommandBuffer.AddBuffer<ActorAttribute3Modifys<_Power>>(index, weaponInstalledState.shipEntity).Add(
                        new ActorAttribute3Modifys<_Power>
                        {
                            //player = weaponInstalledState.shipActorOwner.playerEntity,//自己消耗自己的power 就不需要知道是谁消耗了
                            value = -weaponControlInfo.GetConsumePower(weaponInstalledState.slot.main),
                            attribute3ModifyType = Attribute3SubModifyType.ValueOffset
                        });

                    var fireActorType = weaponControlInfo.GetFireActorType(weaponInstalledState.slot.main);
                    var attributeScale = weaponControlInfo.GetAttributeScale(weaponInstalledState.slot.main);
                    if (fireActorType != ActorTypes.None)
                    {
                        var e = ctrlCommandBuffer.CreateEntity(index);
                        ctrlCommandBuffer.AddComponent(index, e, new FireCreateData
                        {
                            actorOwner = weaponInstalledState.shipActorOwner,
                            shipEntity = weaponInstalledState.shipEntity,
                            weaponEntity = weaponEntity,
                            fireActorType = fireActorType,
                            firePosition = weaponInput.firePosition,
                            attributeOffsetScale = attributeScale,
                        });
                    }
#if false
                    else
                    {
                        weaponControlInFireStateCommandBuffer.AddComponent(weaponEntity, new WeaponControlInFireState { duration = weaponControlInfo.fireDuration });

                        commandBuffer.AddComponent(index, weaponEntity, OnWeaponControlFireOnMessage);
                        endCommandBuffer.RemoveComponent(index, weaponEntity, OnWeaponControlFireOnMessage);
                    }
#endif
                }
            }
        }

#if false
        //[BurstCompile]
        //[ExcludeComponent(typeof(WeaponInstalledState))]
        struct FireJobB : IJobForEachWithEntity<WeaponInput, WeaponExplosionSelf, WeaponControlInfo, WeaponControl>
        {
            public float fixedDeltaTime;

            public ComponentType OnWeaponControlFirePrepareMessage;

            public EntityCommandBuffer.Concurrent ctrlCommandBuffer;
            public EntityCommandBuffer.Concurrent endCommandBuffer;


            public void Execute(Entity weaponEntity, int index, [ReadOnly]ref WeaponInput weaponInput, [ReadOnly]ref WeaponExplosionSelf weaponExplosionSelf, [ReadOnly]ref WeaponControlInfo weaponControlInfo, ref WeaponControl weaponControl)
            {
                var fireEvent = weaponControl.OnFire(fixedDeltaTime, weaponControlInfo);

                if (fireEvent == WeaponControl.FireEvent.Prepare)
                {
                    ctrlCommandBuffer.AddComponent(index, weaponEntity, OnWeaponControlFirePrepareMessage);
                    endCommandBuffer.RemoveComponent(index, weaponEntity, OnWeaponControlFirePrepareMessage);
                }
                else if (fireEvent == WeaponControl.FireEvent.Fire)
                {
                    var fireActorType = weaponControlInfo.GetFireActorType(false);
                    if (fireActorType != ActorTypes.None)
                    {
                        var e = ctrlCommandBuffer.CreateEntity(index);
                        ctrlCommandBuffer.AddComponent(index, e, new FireCreateData
                        {
                            actorOwner = weaponExplosionSelf.lastShipActorOwner,
                            shipEntity = weaponExplosionSelf.lastShipEntity,
                            weaponEntity = weaponEntity,
                            fireActorType = fireActorType,
                            firePosition = weaponInput.firePosition,
                        });
                    }
                }
            }
        }
#endif

#if false
        [BurstCompile]
        [RequireComponentTag(typeof(WeaponInstalledState))]
        struct WeaponFireStateJob : IJobForEachWithEntity<WeaponControlInFireState>
        {
            public float fixedDeltaTime;

            public ComponentType WeaponControlInFireState;
            public ComponentType OnWeaponControlFireOffMessage;
            public EntityCommandBuffer.Concurrent commandBuffer;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity weaponEntity, int index, ref WeaponControlInFireState fireState)
            {
                fireState.duration -= fixedDeltaTime;
                if (fireState.duration <= 0)
                {
                    commandBuffer.RemoveComponent(index, weaponEntity, WeaponControlInFireState);

                    commandBuffer.AddComponent(index, weaponEntity, OnWeaponControlFireOffMessage);
                    endCommandBuffer.RemoveComponent(index, weaponEntity, OnWeaponControlFireOffMessage);
                }
            }
        }
#endif

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new UninstallJob
            {
                OnWeaponUninstallMessage = typeof(OnWeaponUninstallMessage),
                ctrlCommandBuffer = controlBarrier.CreateCommandBuffer().ToConcurrent(),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),

                Weapon_OnShipDestroyMessageFromEntity = GetComponentDataFromEntity<Weapon_OnShipDestroyMessage>(true),
            }
            .Schedule(this, inputDeps);

            inputDeps.Complete();//ChangedFilter 需要inputDeps.Complete()

            var translationFromEntity = GetComponentDataFromEntity<Translation>(true);

            inputDeps = new FirePrepareJob
            {
                powerFromEntity = GetComponentDataFromEntity<ActorAttribute3<_Power>>(true),
                translationFromEntity = translationFromEntity,
            }
            .Schedule(this, inputDeps);


            inputDeps = new FireDirectionJob
            {
                slotRotationFromEntity = GetComponentDataFromEntity<Rotation>(true),
                weaponControlFromEntity = GetComponentDataFromEntity<WeaponControl>(true),
            }
            .Schedule(this, inputDeps);

            /*inputDeps = new FollowShipJob
            {
                translationFromEntity = translationFromEntity
            }
            .Schedule(this, inputDeps);*/


            //
            inputDeps = new FireJobA
            {
                fixedDeltaTime = Time.fixedDeltaTime,

                OnWeaponControlFirePrepareMessage = typeof(OnWeaponControlFirePrepareMessage),
                ctrlCommandBuffer = controlBarrier.CreateCommandBuffer().ToConcurrent(),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),

#if false
                OnWeaponControlFireOnMessage = typeof(OnWeaponControlFireOnMessage),
                weaponControlInFireStateCommandBuffer = weaponControlInFireStateCommandBuffer.ToConcurrent(),
#endif
            }
            .Schedule(this, inputDeps);

            /*inputDeps = new FireJobB
            {
                fixedDeltaTime = Time.fixedDeltaTime,

                OnWeaponControlFirePrepareMessage = typeof(OnWeaponControlFirePrepareMessage),
                ctrlCommandBuffer = controlBarrier.CreateCommandBuffer().ToConcurrent(),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);*/

#if false
            inputDeps = new WeaponFireStateJob
            {
                fixedDeltaTime = Time.fixedDeltaTime,

                WeaponControlInFireState = typeof(WeaponControlInFireState),
                OnWeaponControlFireOffMessage = typeof(OnWeaponControlFireOffMessage),
                commandBuffer = controlBarrier.CreateCommandBuffer().ToConcurrent(),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            weaponControlInFireStateCommandBuffer.Playback(EntityManager);
#endif


            controlBarrier.AddJobHandleForProducer(inputDeps);
            //endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}
