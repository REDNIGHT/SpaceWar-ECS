using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class ShipSpawner : ActorSpawnerSpaceWar
    {
        [Header("base")]
        public float HP = 100f;
        const float HPRegain = 0f;

        //public byte weaponMaxCount = 5;
        //public byte assistWeaponMaxCount = 2;

        [Header("velocity")]
        public float forwardScale = 1f;
        public float backScale = 0.35f;
        public float leftRightScale = 0.5f;
        public const float moveComboScale = 0.75f;
        public ShipVelocity velocity0;
        public ShipVelocity velocity1;
        public ShipVelocity velocity2;
        public ShipVelocity velocity3;
        public float dragByLRBVelocity = 1f;

        [Header("accelerate")]
        public bool accelerateForwardOnly;
        public ShipAccelerate accelerate0;
        public ShipAccelerate accelerate1;
        public ShipAccelerate accelerate2;

        [Header("power")]
        public ShipPower power0;
        public ShipPower power1;
        public ShipPower power2;
        public ShipPower power3;
        public float lostInputTime = 1f;

        [Header("shield")]
        public ShipShield shield0;
        public ShipShield shield1;
        public ShipShield shield2;

        [Header("Rigidbody")]
        public float mass = 1f;
        public float linearDrag = 0.25f;
        public float angularDrag = 0.75f;



        public const int Slots_TransformIndex = 0;
        public const int AssistSlots_TransformIndex = 1;
        public const int ShieldRoot_TransformIndex = 2;
        public const int AccelerateFx_TransformIndex = 3;
        public const int DisableInputFx_TransformIndex = 4;
        public const int DestroyFx_TransformIndex = 5;
        public const int Name_TransformIndex = 6;
        public const int MyLocator_TransformIndex = 7;
        public const int TeamLocators_TransformIndex = 8;
        public const int Colliders_TransformIndex = 9;
        public const int Model_TransformIndex = 10;

        public override int[] removeTransformIndexInServer => new int[]
        {
            AccelerateFx_TransformIndex,
            DisableInputFx_TransformIndex,
            DestroyFx_TransformIndex,
            Name_TransformIndex,
            MyLocator_TransformIndex,
            TeamLocators_TransformIndex,
            Model_TransformIndex,
        };

        void OnValidate()
        {
            if (_actorType > ActorTypes.__Ship_Begin__ && _actorType < ActorTypes.__Ship_End__)
                return;

            Debug.LogError($"{_actorType} > ActorTypes.__Ship_Begin__ && {_actorType} < ActorTypes.__Ship_End__  this={this.name}", this);

            _actorType = ActorTypes.__Ship_Begin__ + 1;
        }

        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            yield return (CS_D, typeof(OnCreateMessage),                  /**/ null);
            yield return (CS_M, typeof(OnDestroyMessage),                 /**/ null);

            yield return (CS_D, typeof(Actor),                            /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),                       /**/ null);
            yield return (C__D, typeof(ActorId),                          /**/ null);

            yield return (_S_D, typeof(ActorVisibleDistanceOnCD),         /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { isUnlimited = true }));
            yield return (_S_D, typeof(ActorVisibleDistanceOnSync),       /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnSync { syncType = SyncActorType.RB_Translation_Rotation_Velocity, perFrame = PerFrame._2 }));

            yield return (_S_P, typeof(EntityBehaviour),                  /**/ EntityBehaviour._initComponent);
            yield return (C__P, typeof(EntityBehaviour),                  /**/ EntityBehaviour._removeComponent);

            yield return (CS_D, typeof(Transform),                        /**/ null);
            yield return (_S_D, typeof(Transform_Out),                    /**/ null);//有刚体才需要有Transform_Out 需要把刚体模拟后的坐标传回给Translation,Rotation, 只有服务器需要
            yield return (CS_D, typeof(Translation),                      /**/ null);
            yield return (CS_D, typeof(Rotation),                         /**/ null);
            yield return (_S_D, typeof(Transform_In_OnCreate),            /**/ null);//通过控制刚体来控制Transform 所以这里只需要初始化时给Transform赋值就可以了
            yield return (C__D, typeof(TransformSmooth_In),               /**/ (e, m) => m.SetComponentData(e, new TransformSmooth_In { smoothTime = smoothTime, rotationLerpT = rotationLerpT }));

            yield return (CS_D, typeof(Rigidbody),                        /**/ (e, m) => _initRigidbody(e, m, mass, linearDrag, angularDrag));
            yield return (_S_D, typeof(Rigidbody_Out),                    /**/ null);
            yield return (CS_D, typeof(RigidbodyVelocity),                /**/ null);
            yield return (CS_D, typeof(Rigidbody_In),                     /**/ null);
            //yield return (_S, typeof(RigidbodyAutoSleep),             /**/ null);
            //yield return (_S, typeof(OnPhysicsCallMessage),           /**/ null);
            yield return (_S_D, typeof(RigidbodyForce),                   /**/ null);
            yield return (_S_D, typeof(RigidbodyTorque),                  /**/ null);
            yield return (_S_D, typeof(RigidbodyLinearDragChange),        /**/ null);

            yield return (_S_D, typeof(ControlForceDirection),            /**/ (e, m) => m.SetComponentData(e, new ControlForceDirection { zeroEnable = true }));
            yield return (_S_D, typeof(ControlTorqueAngular),             /**/ (e, m) => m.SetComponentData(e, new ControlTorqueAngular { zeroEnable = true }));

            yield return (CS_D, typeof(ShipPowers),                       /**/ (e, m) => m.SetComponentData(e, new ShipPowers { power0 = power0, power1 = power1, power2 = power2, power3 = power3, lostInputTime = lostInputTime }));
            yield return (CS_D, typeof(ActorAttribute1<_PowerLevel>),     /**/ null);
            yield return (CS_D, typeof(ActorAttribute3<_Power>),          /**/ (e, m) => m.SetComponentData(e, new ActorAttribute3<_Power> { max = power0.max, regain = power0.regain, value = power0.max }));
            yield return (_S_D, typeof(ActorAttribute3Modifys<_Power>),   /**/ null);

            yield return (_S_S, typeof(ShipLostInputState),               /**/ null);

            yield return (_S_D, typeof(ActorAttribute1<_VelocityLevel>),  /**/ null);
            yield return (_S_D, typeof(ActorAttribute1<_ShieldLevel>),    /**/ null);

            yield return (CS_D, typeof(ActorAttribute3<_HP>),             /**/ (e, m) => m.SetComponentData(e, new ActorAttribute3<_HP> { max = HP, regain = HPRegain, value = HP }));
            yield return (_S_D, typeof(ActorAttribute3Modifys<_HP>),      /**/ null);
            yield return (_S_D, typeof(KillersOnActorDeath),              /**/ null);
            yield return (_S_D, typeof(ActorScoreTag),                    /**/ null);
            yield return (_S_D, typeof(ActorLastAttribute),               /**/ null);


            yield return (_S_D, typeof(ShipSlotList),                     /**/ null);
            yield return (C__P, typeof(SlotAngleLimitLines),                    /**/ null);
            yield return (CS_D, typeof(ShipWeaponArray),                  /**/ null);

            yield return (_S_D, typeof(ShipShields),                      /**/ (e, m) => m.SetComponentData(e, new ShipShields { shield0 = shield0, shield1 = shield1, shield2 = shield2 }));

            yield return (_S_D, typeof(ShipMoveInput),                    /**/ null);
            yield return (_S_D, typeof(ShipForceControl),                 /**/ null);
            yield return (_S_D, typeof(ShipTorqueControl),                /**/ null);
            yield return (_S_D, typeof(ShipControlInfo),                  /**/ (e, m) => m.SetComponentData(e,
                new ShipControlInfo
                {
                    forwardScale = forwardScale,
                    backScale = backScale,
                    leftRightScale = leftRightScale,
                    moveComboScale = moveComboScale,
                    dragByLRBVelocity = dragByLRBVelocity,
                    velocity0 = velocity0,
                    velocity1 = velocity1,
                    velocity2 = velocity2,
                    velocity3 = velocity3,
                    accelerateForwardOnly = accelerateForwardOnly,
                    accelerate0 = accelerate0,
                    accelerate1 = accelerate1,
                    accelerate2 = accelerate2
                }));

            yield return (C__P, typeof(ActorAttributePanel),                /**/ null);
            yield return (CS_D, typeof(Ship),                               /**/ null);
        }

        public void AddComponentTypesInMyShip(Entity shipEntity)
        {
            entityManager.AddComponent<ShipMoveInput>(shipEntity);
            entityManager.AddComponent<ShipForceControl>(shipEntity);
            entityManager.AddComponent<ShipTorqueControl>(shipEntity);
            entityManager.AddComponentData(shipEntity, new ShipControlInfo
            {
                forwardScale = forwardScale,
                backScale = backScale,
                leftRightScale = leftRightScale,
                moveComboScale = moveComboScale,
                dragByLRBVelocity = dragByLRBVelocity,
                velocity0 = velocity0,
                velocity1 = velocity1,
                velocity2 = velocity2,
                velocity3 = velocity3,
                accelerateForwardOnly = accelerateForwardOnly,
                accelerate0 = accelerate0,
                accelerate1 = accelerate1,
                accelerate2 = accelerate2
            });

            entityManager.AddComponent<RigidbodyForce>(shipEntity);
            entityManager.AddComponent<RigidbodyTorque>(shipEntity);
            entityManager.AddComponentData(shipEntity, new ControlForceDirection { zeroEnable = true });
            entityManager.AddComponentData(shipEntity, new ControlTorqueAngular { zeroEnable = true });

            //entityManager.SetComponentData(shipEntity, new TransformSmooth_In { smoothTime = 0.25f, rotationLerpT = rotationLerpT });

            entityManager.AddComponent<ActorAttribute1<_PowerLevel>>(shipEntity);
            entityManager.AddComponent<ActorAttribute1<_VelocityLevel>>(shipEntity);
            entityManager.AddComponent<ActorAttribute1<_ShieldLevel>>(shipEntity);
            entityManager.AddComponent<ActorAttribute3Modifys<_Power>>(shipEntity);
        }


        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            ushort m = 0;
            //m |= datas.Hp(entityManager.GetComponentData<ActorAttribute3<_HP>>(actorEntity).value);
            //m |= datas.Power(entityManager.GetComponentData<ActorAttribute3<_Power>>(actorEntity).value);
            m |= datas.Position(entityManager.GetComponentData<Translation>(actorEntity).Value);
            m |= datas.Rotation(entityManager.GetComponentData<Rotation>(actorEntity).Value);

            /*var rbv = entityManager.GetComponentData<RigidbodyVelocity>(actorEntity);
            m |= datas.LinearVelicity(rbv.linear);
            m |= datas.AngularVelicity(rbv.angular);*/
            return m;
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            //entityManager.SetComponentData(actorEntity, new ActorAttribute3<_HP> { max = HP, regain = HPRegain, value = datas.hp });
            //entityManager.SetComponentData(actorEntity, new ActorAttribute3<_Power> { max = power0.max, regain = power0.regain, value = datas.power });

            entityManager.SetComponentData(actorEntity, new Translation { Value = datas.position });
            entityManager.SetComponentData(actorEntity, new Rotation { Value = datas.rotation });

            //entityManager.SetComponentData(actorEntity, new RigidbodyVelocity { linear = datas.linearVelicity, angular = datas.angularVelicity });
        }

        public override void Init(EntityManager _entityManager, Transform root)
        {
            base.Init(_entityManager, root);

            initComponentDatas += (actorEntity, entityManager) =>
            {
                var playerEntity = entityManager.GetComponentData<ActorOwner>(actorEntity).playerEntity;
                Debug.Assert(entityManager.Exists(playerEntity), "entityManager.Exists(playerEntity)");

                var playerActorArray = entityManager.GetComponentData<PlayerActorArray>(playerEntity);
                Debug.Assert(playerActorArray.mainActorEntity == Entity.Null, "playerActorArray.mainEntity == Entity.Null");

                playerActorArray.mainActorEntity = actorEntity;
                entityManager.SetComponentData(playerEntity, playerActorArray);
            };

            if (isServer)
            {
                initComponentDatas += (e, m) => InitSlots(e, m, name, true);
                CreateWeaponSlotArchetype(entityManager);
            }
        }

        public override void DestroyEntityInClient(Entity actorEntity)
        {
            var playerEntity = entityManager.GetComponentData<ActorOwner>(actorEntity).playerEntity;
            if (entityManager.Exists(playerEntity) && entityManager.HasComponent<PlayerActorArray>(playerEntity))//断开链接后飞船的ActorOwner.playerEntity是空的
            {
                //ship被打爆后的处理
                //input会根据这playerActorArray.mainEntity变量输入
                var playerActorArray = entityManager.GetComponentData<PlayerActorArray>(playerEntity);

                playerActorArray.mainActorEntity = Entity.Null;
                entityManager.SetComponentData(playerEntity, playerActorArray);
            }

            base.DestroyEntityInClient(actorEntity);
        }


        //
        static EntityArchetype slotArchetype;
        static EntityArchetype assistWeaponSlotArchetype;
        static void CreateWeaponSlotArchetype(EntityManager _entityManager)
        {
            slotArchetype = _entityManager.CreateArchetype
                (
                    typeof(OnCreateMessage),

                    typeof(Transform),
                    typeof(Transform_Out),
                    typeof(Translation),
                    typeof(Rotation),
                    //typeof(Transform_In),


                    //typeof(SlotUsingState),
                    typeof(Slot)
                );

            assistWeaponSlotArchetype = _entityManager.CreateArchetype
                (
                    typeof(OnCreateMessage),

                    typeof(Transform),
                    typeof(Transform_Out),
                    typeof(Translation),
                    typeof(Rotation),
                    //typeof(Transform_In),


                    //typeof(SlotUsingState),
                    typeof(Slot),
                    typeof(AssistSlot)
                );
        }

        public static void InitSlots(Entity shipEntity, EntityManager entityManager, string shipName, bool assistWeaponSlotsEnable)
        {
            var slotList = entityManager.GetComponentData<ShipSlotList>(shipEntity);
            var shipT = entityManager.GetComponentObject<Transform>(shipEntity);
            var slotsT = shipT.GetChild(Slots_TransformIndex);
            foreach (Transform weaponSlotT in slotsT)
            {
                var slotEntity = entityManager.CreateEntity(slotArchetype);

#if UNITY_EDITOR
                entityManager.SetName(slotEntity, shipName.Replace("_spawner", "") + ":" + shipEntity.Index + "  w_slot:" + slotEntity.Index);
#endif

                slotList.Add(slotEntity);

                var weaponSlotInfo = weaponSlotT.GetComponent<SlotInfo>();

                entityManager.AddComponentObject(slotEntity, weaponSlotT);
                //entityManager.SetComponentData(slotEntity, new Translation { Value = weaponSlotT.position });
                //entityManager.SetComponentData(slotEntity, new Rotation { Value = weaponSlotT.rotation });
                entityManager.SetComponentData(slotEntity, new Slot
                {
                    shipEntity = shipEntity,
                    main = weaponSlotInfo.mainSlot,
                    index = (byte)weaponSlotT.GetSiblingIndex(),
                    halfAngleLimitMin = weaponSlotInfo.angleLimitMin * 0.5f,
                    halfAngleLimitMax = weaponSlotInfo.angleLimitMax * 0.5f,
                    aimType = weaponSlotInfo.aimType,
                });
            }

            //
            if (assistWeaponSlotsEnable)
            {
                var assistWeaponSlotsT = shipT.GetChild(AssistSlots_TransformIndex);
                foreach (Transform weaponSlotT in assistWeaponSlotsT)
                {
                    var assistWeaponSlotEntity = entityManager.CreateEntity(assistWeaponSlotArchetype);

#if UNITY_EDITOR
                    entityManager.SetName(assistWeaponSlotEntity, shipName.Replace("_spawner", "") + ":" + shipEntity.Index + "  a_slot:" + assistWeaponSlotEntity.Index);
#endif

                    slotList.Add(assistWeaponSlotEntity);

                    var weaponSlotInfo = weaponSlotT.GetComponent<SlotInfo>();

                    entityManager.AddComponentObject(assistWeaponSlotEntity, weaponSlotT);
                    //entityManager.SetComponentData(assistWeaponSlotEntity, new Translation { Value = weaponSlotT.position });
                    //entityManager.SetComponentData(assistWeaponSlotEntity, new Rotation { Value = weaponSlotT.rotation });
                    entityManager.SetComponentData(assistWeaponSlotEntity, new Slot
                    {
                        shipEntity = shipEntity,
                        main = false,
                        index = (byte)weaponSlotT.GetSiblingIndex(),
                        halfAngleLimitMin = weaponSlotInfo.angleLimitMin * 0.5f,
                        halfAngleLimitMax = weaponSlotInfo.angleLimitMax * 0.5f,
                        aimType = weaponSlotInfo.aimType,
                    });
                    //entityManager.SetComponentData(assistWeaponSlotEntity, new AssistSlot { });//AssistSlot can not be called with a zero sized component.
                }
            }


            //
            entityManager.SetComponentData(shipEntity, slotList);
        }
    }
}
