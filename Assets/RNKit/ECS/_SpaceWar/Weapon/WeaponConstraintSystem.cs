using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    public class WeaponConstraintServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponInstallMessage))]
        struct WeaponInstallJob : IJobForEach<WeaponInstalledState, ActorVisibleDistanceOnSync>
        {
            public void Execute(ref WeaponInstalledState weaponInstalledState, ref ActorVisibleDistanceOnSync actorVisibleDistanceOnSync)
            {
                weaponInstalledState.syncType = actorVisibleDistanceOnSync.syncType;
                if (actorVisibleDistanceOnSync.syncType == SyncActorType.RB_Translation_Rotation_Velocity)
                {
                    actorVisibleDistanceOnSync.syncType = SyncActorType.RB_Rotation_Velocity;
                }
                else
                {
                    actorVisibleDistanceOnSync.syncType = SyncActorType.Disable;
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponUninstallMessage))]
        struct WeaponUninstallJob : IJobForEach<WeaponInstalledState, ActorVisibleDistanceOnSync>
        {
            public void Execute([ReadOnly]ref WeaponInstalledState weaponInstalledState, ref ActorVisibleDistanceOnSync actorVisibleDistanceOnSync)
            {
                actorVisibleDistanceOnSync.syncType = weaponInstalledState.syncType;
            }
        }

        /*[BurstCompile]
        [ExcludeComponent(typeof(OnWeaponUninstallMessage))]
        struct WeaponUpdateJob : IJobForEachWithEntity<WeaponInstalledState, RigidbodyVelocity>
        {
            public ComponentDataFromEntity<Translation> translationFromEntity;
            public void Execute(Entity entity, int index, [ReadOnly] ref WeaponInstalledState weaponInstalledState, ref RigidbodyVelocity rigidbodyVelocity)
            {
                rigidbodyVelocity.linear = default;

                translationFromEntity[entity] = translationFromEntity[weaponInstalledState.slotEntity];
            }
        }*/

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new WeaponInstallJob { }.Schedule(this, inputDeps);
            inputDeps = new WeaponUninstallJob { }.Schedule(this, inputDeps);
            return inputDeps;

            //var inputDepsA = new WeaponUpdateJob { translationFromEntity = GetComponentDataFromEntity<Translation>() }.ScheduleSingle(this, inputDeps);
            //return JobHandle.CombineDependencies(inputDepsA, inputDeps);
        }
    }

    [DisableAutoCreation]
    public class WeaponConstraintUpdateServerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            /*Entities
                .WithAllReadOnly<WeaponInstalledState>()
                .WithAll<Translation, RigidbodyVelocity>()
                .WithNone<OnWeaponUninstallMessage>()
                .ForEach((ref Translation translation, ref RigidbodyVelocity rigidbodyVelocity, ref WeaponInstalledState weaponInstalledState) =>
                {
                    rigidbodyVelocity.linear = default;

                    translation = EntityManager.GetComponentData<Translation>(weaponInstalledState.slotEntity);
                });*/

            Entities
                .WithAllReadOnly<WeaponInstalledState>()
                .WithAll<Transform, Rigidbody>()
                .WithNone<OnWeaponUninstallMessage>()
                .ForEach((ref WeaponInstalledState weaponInstalledState, Transform transform, Rigidbody rigidbody) =>
                {
                    rigidbody.velocity = default;

                    transform.position = EntityManager.GetComponentObject<Transform>(weaponInstalledState.slotEntity).position;
                });
        }
    }



    public class _DampedTransform : MonoBehaviour
    {
        public Transform shipT;
        public Transform target;
        public Transform weaponBaseT;
    }

    [DisableAutoCreation]
    public class WeaponConstraintClientSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<OnWeaponInstallMessage, Weapon, WeaponInstalledState, Transform>()
                .ForEach((Entity weaponEntity, Transform weaponT, ref WeaponInstalledState weaponInstalledState, ref Weapon weapon) =>
                {
                    //把weapon约束到slot上
                    var shipT = EntityManager.GetComponentObject<Transform>(weaponInstalledState.shipEntity);

                    var slotsT = weapon.type == WeaponType.Attack ? shipT.GetChild(ShipSpawner.Slots_TransformIndex) : shipT.GetChild(ShipSpawner.AssistSlots_TransformIndex);

                    var slotT = slotsT.GetChild(weaponInstalledState.slot.index);

                    var dampedTransform = weaponT.gameObject.GetComponent<_DampedTransform>();
                    if (dampedTransform == null)
                    {
                        dampedTransform = weaponT.gameObject.AddComponent<_DampedTransform>();
                    }

                    dampedTransform.shipT = shipT;
                    dampedTransform.target = slotT;
                    EntityManager.AddComponentObject(weaponEntity, dampedTransform);


                    //
                    var maskT = weaponT.GetChild(WeaponSpawner.Mask_TransformIndex);
                    maskT.gameObject.SetActive(false);
                });

            Entities
                .WithAllReadOnly<OnWeaponUninstallMessage, WeaponInstalledState, Weapon, Transform>()
                .WithAll<TransformSmooth_In>()
                .ForEach((Entity weaponEntity, Transform weaponT, ref WeaponInstalledState weaponInstalledState, ref Weapon weapon, ref TransformSmooth_In transformSmooth_In) =>
                {
                    //解除weapon的约束
                    var dampedTransform = weaponT.gameObject.GetComponent<_DampedTransform>();
                    dampedTransform.shipT = null;
                    dampedTransform.target = null;

                    EntityManager.RemoveComponent<_DampedTransform>(weaponEntity);
                    transformSmooth_In.smoothTime = ActorSpawnerSpaceWar.smoothTime;


                    //把weapon base 绑回weaponT上
                    if (weapon.type == WeaponType.Attack)
                    {
                        var shipT = EntityManager.GetComponentObject<Transform>(weaponInstalledState.shipEntity);

                        var slotT = shipT.GetChild(ShipSpawner.Slots_TransformIndex).GetChild(weaponInstalledState.slot.index);

                        var weaponBaseT = dampedTransform.weaponBaseT;
                        if (weaponBaseT != null)
                        {
                            dampedTransform.weaponBaseT = null;

                            weaponBaseT.SetParent(weaponT, false);
                            weaponBaseT.SetSiblingIndex(WeaponSpawner.BaseModel_TransformIndex);
                        }
                    }


                    //
                    var maskT = weaponT.GetChild(WeaponSpawner.Mask_TransformIndex);
                    maskT.gameObject.SetActive(true);
                });
        }
    }

    [DisableAutoCreation]
    public class WeaponConstraintUpdateClientSystem : ComponentSystem
    {
        const float beginSmoothTime = 0.5f;
        const float fixedDeltaTimeScaleA = 7.5f;
        const float fixedDeltaTimeScaleB = 15f;
        Vector3 offset;
        protected void OnInit(Transform root)
        {
            if (ServerBootstrap.world != World)
            {
                offset = root.position;
            }
        }

        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<WeaponInstalledState, Weapon, _DampedTransform>()
                .WithAll<Translation, TransformSmooth_In>()
                .WithNone<OnDestroyMessage>()
                .ForEach((_DampedTransform dampedTransform, ref Translation translation, ref TransformSmooth_In transformSmooth_In, ref Weapon weapon, ref WeaponInstalledState weaponInstalledState) =>
                {
                    translation.Value = dampedTransform.target.position - offset;

                    if (transformSmooth_In.smoothTime > 0f)
                    {
                        transformSmooth_In.smoothTime -= Time.fixedDeltaTime * fixedDeltaTimeScaleA;

                        if (transformSmooth_In.smoothTime <= 0f)
                        {
                            //
                            transformSmooth_In.smoothTime = Mathf.Clamp(transformSmooth_In.smoothTime, 0f, beginSmoothTime);




                            //把weapon base 绑到slot上
                            if (weapon.type == WeaponType.Attack)
                            {
                                //把weapon约束到slot上
                                var shipT = EntityManager.GetComponentObject<Transform>(weaponInstalledState.shipEntity);

                                var slotT = shipT.GetChild(ShipSpawner.Slots_TransformIndex).GetChild(weaponInstalledState.slot.index);

                                var weaponT = dampedTransform.transform;

                                var weaponBaseT = weaponT.GetChild(WeaponSpawner.BaseModel_TransformIndex);


                                weaponBaseT.SetParent(slotT, false);
                                dampedTransform.weaponBaseT = weaponBaseT;
                            }
                        }
                    }
                });

            Entities
                .WithAllReadOnly<WeaponInstalledState, TransformSmooth_In, _DampedTransform, Transform, Rigidbody>()
                .WithNone<OnDestroyMessage>()
                .ForEach((ref TransformSmooth_In transformSmooth_In, _DampedTransform dampedTransform, Transform transform, Rigidbody rigidbody) =>
                {
                    if (transformSmooth_In.smoothTime > 0f)
                    {
                        transform.position = Vector3.Lerp(transform.position, dampedTransform.target.position, Time.fixedDeltaTime * fixedDeltaTimeScaleB);
                        return;
                    }

                    rigidbody.velocity = default;

                    transform.position = dampedTransform.target.position;
                });
        }
    }
}
