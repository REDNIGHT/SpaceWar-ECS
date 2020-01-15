using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class MissileSpawner : ActorSpawnerSpaceWar
    {
        public float lifetime = 5f;
        public float hp = 25f;
        public ActorTypes explosionType;
        public float3 explosionOffset;

        [Header("findTarget")]
        public bool findTarget;
        public float targetVelocityScale = 0.25f;

        [Header("gotoPosition")]
        public bool gotoTargetPoint;
        public bool cancelOnGotoTargetPoint;
        public float cancelAngle = 5f;

        [Header("AutoExplosionByRaycast")]
        public LayerMask layerMask;

        [Header("AutoExplosionByAngle")]
        public bool autoExplosionByAngle = false;


        [Header("AutoExplosionByTouch")]
        public bool autoExplosionByTouch;
        public float beginForce;
        public float beginTorque;


        [Header("MissilePhysics")]
        public float force = 0.2f;
        public float maxVelocity = 12f;
        public float torque = 0.25f;
        public float maxTorque = 12.5f;

        public bool accelerateByFindTarget = false;
        public float forceByTarget = 0.3f;
        public float maxVelocityByTarget = 18f;


        //
        public const float mass = 0.1f;
        public float linearDrag = 0.01f;
        public float angularDrag = 0.1f;


        //
        public const int OnTrigger_TransformIndex = 0;
        public const int Raycast_TransformIndex = 1;
        public const int Fx_TransformIndex = 2;
        public const int Model_TransformIndex = 3;

        public override int[] removeTransformIndexInServer => new int[] { Fx_TransformIndex, Model_TransformIndex, };

        void OnValidate()
        {
            if (_actorType > ActorTypes.__Missile_Begin__ && _actorType < ActorTypes.__Missile_End__)
                return;

            Debug.LogError($"{_actorType} > ActorTypes.__Missile_Begin__ && {_actorType} < ActorTypes.__Missile_End__", this);

            _actorType = ActorTypes.__Missile_Begin__ + 1;
        }

        ActorSyncCreateClientSystem actorClientSystem;
        public override void Init(EntityManager entityManager, Transform root)
        {
            base.Init(entityManager, root);

            if (isClient)
            {
                actorClientSystem = entityManager.World.GetExistingSystem<ActorSyncCreateClientSystem>();
            }
        }

        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            yield return (CS_D, typeof(OnCreateMessage),                  /**/ null);
            yield return (CS_M, typeof(OnDestroyMessage),                 /**/ null);

            yield return (CS_D, typeof(Actor),                            /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),                       /**/ null);
            yield return (C__D, typeof(ActorId),                          /**/ null);

            yield return (CS_D, typeof(ActorLifetime),                    /**/ (e, m) => m.SetComponentData(e, new ActorLifetime { lifetime = lifetime, value = lifetime }));

            yield return (_S_D, typeof(ActorVisibleDistanceOnCD),         /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { isUnlimited = false }));
            yield return (_S_D, typeof(ActorVisibleDistanceOnSync),       /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnSync
            { syncType = SyncActorType.RB_Translation_Rotation_Velocity, perFrame = autoExplosionByTouch ? PerFrame._5 : PerFrame._3 }));

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
            if (autoExplosionByTouch == false)
            {
                yield return (_S_D, typeof(RigidbodyForce),               /**/ null);
                yield return (_S_D, typeof(RigidbodyTorque),              /**/ null);
            }

            //yield return (_S, typeof(ActorAttribute3Base<_HP>),       /**/ (e, m) => m.SetComponentData(e, new ActorAttribute3Base<_HP> { max = HP, regain = HPRegain }));
            yield return (_S_D, typeof(ActorAttribute3<_HP>),             /**/ (e, m) => m.SetComponentData(e, new ActorAttribute3<_HP> { max = hp, regain = 0f, value = hp }));
            yield return (_S_D, typeof(ActorAttribute3Modifys<_HP>),       /**/ null);
            yield return (_S_D, typeof(KillersOnActorDeath),              /**/ null);

            yield return (_S_D, typeof(ActorCreateOnDestroy),             /**/ (e, m) => m.SetComponentData(e, new ActorCreateOnDestroy { actorType = (short)explosionType, offset = explosionOffset }));

            yield return (CS_D, typeof(WeaponCreator),                    /**/ null);
            yield return (C__D, typeof(WeaponFireFx),                     /**/ null);


            if (findTarget)
            {
                yield return (_S_P, typeof(OnTrigger),                    /**/ (e, m) => OnTrigger._initComponent(m.GetComponentObject<Transform>(e).GetChild(OnTrigger_TransformIndex).GetComponent<OnTrigger>(), e, m, false, true, false, false));
                yield return (C__P, typeof(OnTrigger),                    /**/ (e, m) => OnTrigger._removeComponent(m.GetComponentObject<Transform>(e).GetChild(OnTrigger_TransformIndex), e, m));
                yield return (_S_D, typeof(PhysicsTriggerResults),        /**/ null);

                yield return (_S_D, typeof(FindTraceTarget),              /**/ (e, m) => m.SetComponentData(e, new FindTraceTarget { targetVelocityScale = targetVelocityScale }));
                yield return (_S_D, typeof(TracePoint),                   /**/ null);
                yield return (_S_D, typeof(TraceDirectionData),           /**/ (e, m) => m.SetComponentData(e, new TraceDirectionData { enable = false, targetAngleOffset = float.MaxValue, lastTargetAngleOffset = float.MaxValue }));
                yield return (_S_D, typeof(TraceDirection),               /**/ null);

                yield return (_S_D, typeof(ControlForceDirection),        /**/ (e, m) => m.SetComponentData(e, new ControlForceDirection { force = force, maxVelocity = maxVelocity, }));
                yield return (_S_D, typeof(ControlTorqueDirection),       /**/ null);
                yield return (_S_D, typeof(ControlTorqueAngular),         /**/ (e, m) => m.SetComponentData(e, new ControlTorqueAngular { torque = torque, maxTorque = maxTorque, }));
            }
            else if (gotoTargetPoint)
            {
                yield return (_S_D, typeof(TracePoint),                   /**/ null);
                yield return (_S_D, typeof(TraceDirectionData),           /**/ (e, m) => m.SetComponentData(e, new TraceDirectionData { enable = true, cancelOnGotoTargetPoint = cancelOnGotoTargetPoint, cancelAngle = cancelAngle, targetAngleOffset = float.MaxValue, lastTargetAngleOffset = float.MaxValue }));
                yield return (_S_D, typeof(TraceDirection),               /**/ null);

                yield return (_S_D, typeof(ControlForceDirection),        /**/ (e, m) => m.SetComponentData(e, new ControlForceDirection { force = force, maxVelocity = maxVelocity, }));
                yield return (_S_D, typeof(ControlTorqueDirection),       /**/ null);
                yield return (_S_D, typeof(ControlTorqueAngular),         /**/ (e, m) => m.SetComponentData(e, new ControlTorqueAngular { torque = torque, maxTorque = maxTorque, }));
            }
            else if (autoExplosionByTouch == false)
            {
                yield return (_S_D, typeof(ControlForceDirection),        /**/ (e, m) => m.SetComponentData(e, new ControlForceDirection { force = force, maxVelocity = maxVelocity, }));
                yield return (_S_D, typeof(ControlTorqueDirection),       /**/ null);
                yield return (_S_D, typeof(ControlTorqueAngular),         /**/ (e, m) => m.SetComponentData(e, new ControlTorqueAngular { torque = torque, maxTorque = maxTorque, }));
            }

            if (layerMask != 0)
            {
                yield return (_S_D, typeof(OnPhysicsCallMessage),         /**/ null);
                yield return (_S_D, typeof(ChildTransform_Out),               /**/ (e, m) => m.SetComponentData(e, new ChildTransform_Out { childIndex0 = Raycast_TransformIndex, childIndex1 = -1, }));
                yield return (_S_D, typeof(LastPosition),                 /**/ null);

                yield return (_S_D, typeof(PhysicsLinecast),              /**/ (e, m) => m.SetComponentData(e, new PhysicsLinecast { layerMask = layerMask }));
                yield return (_S_D, typeof(PhysicsRaycastResults),        /**/ null);
            }

            if (autoExplosionByAngle)
            {
                yield return (_S_D, typeof(MissileAutoExplosionByAngle),  /**/ null);
            }

            if (autoExplosionByTouch)
            {
                yield return (_S_P, typeof(OnTrigger),                    /**/ (e, m) => OnTrigger._initComponent(m.GetComponentObject<Transform>(e).GetChild(OnTrigger_TransformIndex).GetComponent<OnTrigger>(), e, m, false, true, false, false));
                yield return (C__P, typeof(OnTrigger),                    /**/ (e, m) => OnTrigger._removeComponent(m.GetComponentObject<Transform>(e).GetChild(OnTrigger_TransformIndex), e, m));
                yield return (_S_D, typeof(PhysicsTriggerResults),        /**/ null);

                yield return (_S_D, typeof(MissileAutoExplosionByTouche), /**/ (e, m) => m.SetComponentData(e, new MissileAutoExplosionByTouche { beginForce = beginForce, beginTorque = beginTorque, }));
            }


            yield return (_S_D, typeof(MissilePhysics),                   /**/ (e, m) => m.SetComponentData(e, new MissilePhysics
            {
                accelerateByFindTarget = accelerateByFindTarget,
                forceByTarget = forceByTarget,
                maxVelocityByTarget = maxVelocityByTarget,
            }));

            yield return (_S_D, typeof(Missile),                          /**/ null);
        }


        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            ushort m = 0;
            m |= datas.IntValueA(entityManager.GetComponentData<WeaponCreator>(actorEntity).entity.Index);

            //m |= datas.Hp(entityManager.GetComponentData<ActorAttribute3<_HP>>(actorEntity).value);

            m |= datas.Position(entityManager.GetComponentData<Translation>(actorEntity).Value);
            m |= datas.Rotation(entityManager.GetComponentData<Rotation>(actorEntity).Value);

            var rbv = entityManager.GetComponentData<RigidbodyVelocity>(actorEntity);
            m |= datas.LinearVelicity(rbv.linear);

            if (rbv.angular.Equals(float3.zero) == false)
                m |= datas.AngularVelicity(rbv.angular);
            return m;
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            if (actorClientSystem.actorEntityFromActorId.TryGetValue(datas.intValueA, out Entity creatorEntity))
                entityManager.SetComponentData(actorEntity, new WeaponCreator { entity = creatorEntity });

            //entityManager.SetComponentData(actorEntity, new ActorAttribute3<_HP> { max = hp, regain = 0f, value = datas.hp });

            entityManager.SetComponentData(actorEntity, new Translation { Value = datas.position });
            entityManager.SetComponentData(actorEntity, new Rotation { Value = datas.rotation });

            entityManager.SetComponentData(actorEntity, new RigidbodyVelocity { linear = datas.linearVelicity, angular = datas.angularVelicity });
        }
    }
}
