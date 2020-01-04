using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class WeaponSpawner : ActorSpawnerSpaceWar
    {
        [Header("Base")]
        public WeaponType weaponType;
        /// <summary>
        /// 右键自动开火
        /// </summary>
        public bool autoFire = false;

        //public bool fireByOutOfSlotAngle = false;
        public short hp = 3;

        /// <summary>
        /// -1是无限子弹
        /// </summary>
        public short itemCount = -1;


        [Header("Time")]
        public float fireInputTime = 0.25f;
        public float firePrepare = 0.1f;
        public float fireInterval = 0.1f;
        //public float fireDuration;
        public bool needPrepareMessage;
        /// <summary>
        /// 第二次或以上开火时 是否需要准备时间
        /// </summary>
        public bool needPrepareEachLoop;


        [Header("FireActorType")]
        //[Range((int)ActorTypes.BulletA, (int)ActorTypes.MissileB)]
        public float consumePower = 1f;
        public ActorTypes fireActorType;
        public float consumePowerByMainSlot = 2f;
        public ActorTypes fireActorTypeByMainSlot;
        public float attributeScaleByMainSlot = 1.5f;
        public bool fireDirectionByShip;


        [Header("Rigidbody")]
        public float linearDrag = 0.5f;
        public const float angularDrag = 0.2f;

        public const float mass = 0.25f;

        //public const float followForce = 2f;
        //public const float followMaxVelocity = 20f;

        public const float torque = 0.25f;
        public const float maxTorque = 10f;

        //public const float resetTorque = 0.25f;
        //public const float resetMaxTorque = 10f;


        //
        public const int firePoint_TransformIndex = 0;
        public const int FirePrepareFx_TransformIndex = 1;
        public const int FireFx_TransformIndex = 2;
        public const int DestroyFx_TransformIndex = 3;

        public const int WeaponItemCountFx_TransformIndex = 4;
        public const int WeaponBaseModel_TransformIndex = 5;

        //public const int InstalledFx_TransformIndex = 5;
        //public const int UninstallFx_TransformIndex = 6;

        public const int Model_TransformIndex = 6;

        public override int[] removeTransformIndexInServer => new int[]
        {
            FirePrepareFx_TransformIndex,
            FireFx_TransformIndex,
            DestroyFx_TransformIndex,
            WeaponItemCountFx_TransformIndex,
            WeaponBaseModel_TransformIndex,
            Model_TransformIndex,

            //InstalledFx_TransformIndex,
            //UninstallFx_TransformIndex,
        };


        void OnValidate()
        {
            if (_actorType > ActorTypes.__Weapon_Begin__ && _actorType < ActorTypes.__Weapon_End__)
                return;

            Debug.LogError($"{_actorType} > ActorTypes.__Weapon_Begin__ && {_actorType} < ActorTypes.__Weapon_End__  this={this.name}", this);

            _actorType = ActorTypes.__Weapon_Begin__ + 1;
        }


        private void Awake()
        {
            if (weaponType == WeaponType.Attack)
            {
                Debug.Assert(consumePower > 0f, "consumePower > 0f", this);
                Debug.Assert(consumePowerByMainSlot > 0f, "consumePowerByMainSlot > 0f", this);
                Debug.Assert(fireActorType != ActorTypes.None, "fireActorType != ActorTypes.None", this);
                Debug.Assert(fireActorTypeByMainSlot != ActorTypes.None, "fireActorTypeByMainSlot != ActorTypes.None", this);

                /*Debug.Assert(followForce > 0f, "followForce > 0f", this);
                Debug.Assert(followMaxVelocity > 0f, "followMaxVelocity > 0f", this);
                Debug.Assert(followMaxVelocity > followForce, "followMaxVelocity > followForce", this);*/

                Debug.Assert(torque > 0f, "torque > 0f", this);
                Debug.Assert(maxTorque > 0f, "maxTorque > 0f", this);
                Debug.Assert(maxTorque > torque, "maxTorque > torque", this);
            }
        }
        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            yield return (CS_D, typeof(OnCreateMessage),                          /**/ null);
            yield return (CS_M, typeof(OnDestroyMessage),                         /**/ null);

            yield return (CS_D, typeof(Actor),                                    /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),                               /**/ null);
            yield return (C__D, typeof(ActorId),                                  /**/ null);

            yield return (_S_D, typeof(ActorVisibleDistanceOnCD),                 /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { isUnlimited = true }));

            if (weaponType == WeaponType.Attack || weaponType == WeaponType.Shield)
                yield return (_S_D, typeof(ActorVisibleDistanceOnSync),           /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnSync { syncType = SyncActorType.RB_Translation_Rotation_Velocity, perFrame = PerFrame._2 }));
            else
                yield return (_S_D, typeof(ActorVisibleDistanceOnSync),           /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnSync { syncType = SyncActorType.RB_Translation_Velocity, perFrame = PerFrame._4 }));

            yield return (_S_P, typeof(EntityBehaviour),                          /**/ EntityBehaviour._initComponent);
            yield return (C__P, typeof(EntityBehaviour),                          /**/ EntityBehaviour._removeComponent);

            yield return (CS_D, typeof(Transform),                                /**/ null);
            yield return (_S_D, typeof(Transform_Out),                            /**/ null);//有刚体才需要有Transform_Out 需要把刚体模拟后的坐标传回给Translation,Rotation, 只有服务器需要
            yield return (CS_D, typeof(Translation),                              /**/ null);
            yield return (CS_D, typeof(Rotation),                                 /**/ null);
            yield return (_S_D, typeof(Transform_In_OnCreate),                    /**/ null);//通过控制刚体来控制Transform 所以这里只需要初始化时给Transform赋值就可以了
            yield return (C__D, typeof(TransformSmooth_In),                       /**/ (e, m) => m.SetComponentData(e, new TransformSmooth_In { smoothTime = smoothTime, rotationLerpT = rotationLerpT }));


            yield return (CS_D, typeof(Rigidbody),                                /**/ (e, m) => _initRigidbody(e, m, mass, linearDrag, angularDrag));
            yield return (_S_D, typeof(Rigidbody_Out),                            /**/ null);
            yield return (CS_D, typeof(RigidbodyVelocity),                        /**/ null);
            yield return (CS_D, typeof(Rigidbody_In),                             /**/ null);
            //yield return (_S, typeof(RigidbodyAutoSleep),                       /**/ null);
            yield return (_S_D, typeof(RigidbodyForce),                           /**/ null);

            //yield return (_S_D, typeof(ControlForceDirection),                    /**/ (e, m) => m.SetComponentData(e, new ControlForceDirection { force = followForce, maxVelocity = followMaxVelocity }));
            if (weaponType == WeaponType.Attack || weaponType == WeaponType.Shield)
            {
                yield return (_S_D, typeof(RigidbodyTorque),                      /**/ null);
                yield return (_S_D, typeof(ControlTorqueDirection),               /**/ null);
                yield return (_S_D, typeof(ControlTorqueAngular),                 /**/ (e, m) => m.SetComponentData(e, new ControlTorqueAngular { torque = torque, maxTorque = maxTorque }));
            }


            yield return (_S_S, typeof(WeaponInstalledState),                     /**/ null);
            yield return (CS_M, typeof(OnWeaponInstallMessage),                   /**/ null);
            yield return (CS_M, typeof(OnWeaponUninstallMessage),                 /**/ null);
            yield return (C__P, typeof(_DampedTransform),                         /**/ null);

            //yield return (SM, typeof(WeaponExplosionSelf),                      /**/ null);
            //yield return (SM, typeof(ActorLifetime),                            /**/ null);


            yield return (_S_D, typeof(WeaponInput),                              /**/ null);

            if (weaponType == WeaponType.Attack)
            {
                yield return (_S_D, typeof(WeaponFirePoint),                      /**/ null);
                yield return (_S_D, typeof(WeaponControl),                        /**/ null);
                yield return (_S_D, typeof(WeaponControlInfo),                    /**/ (e, m) => m.SetComponentData(e, new WeaponControlInfo(fireInputTime, firePrepare, fireInterval, needPrepareMessage, needPrepareEachLoop,
                                                                                                                                           consumePower, fireActorType, consumePowerByMainSlot, fireActorTypeByMainSlot, attributeScaleByMainSlot, fireDirectionByShip)));
            }
            else if (weaponType == WeaponType.Shield)
            {
                yield return (_S_D, typeof(WeaponShield),                         /**/ null);
                yield return (CS_M, typeof(OnWeaponControlFirePrepareMessage),    /**/ null);
                //yield return (_M, typeof(OnWeaponControlFireOnMessage),         /**/ null);
                //yield return (_M, typeof(OnWeaponControlFireOffMessage),        /**/ null);
                //yield return (SM, typeof(WeaponControlInFireState),             /**/ null);


                yield return (_S_D, typeof(WeaponFirePoint),                      /**/ null);
                yield return (_S_D, typeof(WeaponControl),                        /**/ null);
                yield return (_S_D, typeof(WeaponControlInfo),                    /**/ (e, m) => m.SetComponentData(e, new WeaponControlInfo(fireInputTime, firePrepare, fireInterval, needPrepareMessage, needPrepareEachLoop,
                                                                                                                                           consumePower, fireActorType, consumePowerByMainSlot, fireActorTypeByMainSlot, attributeScaleByMainSlot, fireDirectionByShip)));
            }

            yield return (CS_D, typeof(WeaponAttribute),                          /**/ (e, m) => m.SetComponentData(e, new WeaponAttribute { hp = hp, itemCount = itemCount }));
            yield return (CS_D, typeof(Weapon),                                   /**/ (e, m) => m.SetComponentData(e, new Weapon { type = weaponType, autoFire = autoFire }));
        }


        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            ushort m = 0;
            //b |= datas.Hp(entityManager.GetComponentData<ActorAttribute3<_HP>>(actorEntity).value);

            m |= datas.Position(entityManager.GetComponentData<Translation>(actorEntity).Value);
            m |= datas.Rotation(entityManager.GetComponentData<Rotation>(actorEntity).Value);

            var rbv = entityManager.GetComponentData<RigidbodyVelocity>(actorEntity);

            /*if (rbv.linear.Equals(float3.zero) == false)
                m |= datas.LinearVelicity(rbv.linear);
            if (rbv.angular.Equals(float3.zero) == false)
                m |= datas.AngularVelicity(rbv.angular);*/

            return m;
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            //entityManager.SetComponentData(actorEntity, new ActorAttribute3<_HP> { max = hp, regain = hpRegain, value = datas.hp });

            entityManager.SetComponentData(actorEntity, new Translation { Value = datas.position });
            entityManager.SetComponentData(actorEntity, new Rotation { Value = datas.rotation });

            //entityManager.SetComponentData(actorEntity, new RigidbodyVelocity { linear = datas.linearVelicity, angular = datas.angularVelicity });
        }

    }
}
