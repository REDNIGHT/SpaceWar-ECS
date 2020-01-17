using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class BatterySpawner : ActorSpawnerSpaceWar
    {
        [Header("base")]
        public float HP = 50f;
        public float HPRegain = 1f;
        public const float power = 1000f;
        public const float powerRegain = 10f;
        
        [Header("find target")]
        public float targetVelocityScale;

        [Header("Rigidbody")]
        public bool followParent = true;
        public const float force = 2f;
        public const float maxVelocity = 10f;
        public const float torque = 0.1f;
        public const float maxTorque = 10f;

        public const float mass = 5f;
        public const float linearDrag = 0.2f;
        public const float angularDrag = 0.2f;


        //
        public const int Slots_TransformIndex = 0;
        public const int OnTrigger_TransformIndex = 1;
        public const int Colliders_TransformIndex = 2;
        public const int DestroyFx_TransformIndex = 3;
        public const int Model_TransformIndex = 4;
        //public const int DisableInputFx_TransformIndex = ShipSpawner.DisableInputFx_TransformIndex;

        public override int[] removeTransformIndexInServer => new int[] { DestroyFx_TransformIndex, Model_TransformIndex, };


        void OnValidate()
        {
            if (_actorType > ActorTypes.__Battery_Begin__ && _actorType < ActorTypes.__Battery_End__)
                return;

            Debug.LogError($"{_actorType} > ActorTypes.__Battery_Begin__ && {_actorType} < ActorTypes.__Battery_End__  this={this.name}", this);

            _actorType = ActorTypes.__Battery_Begin__ + 1;
        }

        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            yield return (CS_D, typeof(OnCreateMessage),                /**/ null);
            yield return (CS_M, typeof(OnDestroyMessage),               /**/ null);

            yield return (CS_D, typeof(Actor),                          /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),                     /**/ null);
            yield return (C__D, typeof(ActorId),                        /**/ null);

            yield return (_S_D, typeof(ActorVisibleDistanceOnCD),       /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { isUnlimited = true }));
            yield return (_S_D, typeof(ActorVisibleDistanceOnSync),     /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnSync { syncType = SyncActorType.RB_Translation_Rotation_Velocity, perFrame = PerFrame._5 }));

            yield return (_S_P, typeof(EntityBehaviour),                /**/ EntityBehaviour._initComponent);
            yield return (C__P, typeof(EntityBehaviour),                /**/ EntityBehaviour._removeComponent);

            yield return (CS_D, typeof(Transform),                      /**/ null);
            yield return (CS_D, typeof(Transform_Out),                  /**/ null);
            yield return (CS_D, typeof(Translation),                    /**/ null);
            yield return (CS_D, typeof(Rotation),                       /**/ null);
            yield return (_S_D, typeof(Transform_In),                   /**/ null);//这里可以忽略parent的坐标
            yield return (C__D, typeof(TransformSmooth_In),             /**/ (e, m) => m.SetComponentData(e, new TransformSmooth_In { smoothTime = smoothTime, rotationLerpT = rotationLerpT }));

            yield return (CS_D, typeof(Rigidbody),                      /**/ (e, m) => _initRigidbody(e, m, mass, linearDrag, angularDrag));
            yield return (_S_D, typeof(Rigidbody_Out),                  /**/ null);
            yield return (CS_D, typeof(RigidbodyVelocity),              /**/ null);
            yield return (CS_D, typeof(Rigidbody_In),                   /**/ null);

            if (followParent)
            {
                yield return (_S_D, typeof(ParentTransform_Out),            /**/ null);
                yield return (_S_D, typeof(ParentRotation_Out),             /**/ null);
                yield return (_S_D, typeof(RigidbodyForce),                 /**/ null);
                yield return (_S_D, typeof(RigidbodyTorque),                /**/ null);
            }

            yield return (_S_P, typeof(OnTrigger),                      /**/ (e, m) => OnTrigger._initComponent(m.GetComponentObject<Transform>(e).GetChild(OnTrigger_TransformIndex).GetComponent<OnTrigger>(), e, m, false, true, false, false));
            yield return (C__P, typeof(OnTrigger),                      /**/ (e, m) => OnTrigger._removeComponent(m.GetComponentObject<Transform>(e).GetChild(OnTrigger_TransformIndex), e, m));
            yield return (_S_D, typeof(PhysicsTriggerResults),          /**/ null);
            yield return (_S_D, typeof(FindTraceTarget),                /**/ (e, m) => m.SetComponentData(e, new FindTraceTarget { lostTargetEnable = true, targetVelocityScale = targetVelocityScale }));
            yield return (_S_D, typeof(TracePoint),                     /**/ null);

            yield return (_S_D, typeof(ControlForceDirection),          /**/ (e, m) => m.SetComponentData(e, new ControlForceDirection { zeroEnable = true, force = force, maxVelocity = maxVelocity }));
            yield return (_S_D, typeof(ControlTorqueDirection),         /**/ null);
            yield return (_S_D, typeof(ControlTorqueAngular),           /**/ (e, m) => m.SetComponentData(e, new ControlTorqueAngular { zeroEnable = true, torque = torque, maxTorque = maxTorque }));

            yield return (CS_D, typeof(ActorAttribute3<_HP>),           /**/ (e, m) => m.SetComponentData(e, new ActorAttribute3<_HP> { max = HP, regain = HPRegain, value = HP }));
            yield return (_S_D, typeof(ActorAttribute3Modifys<_HP>),    /**/ null);
            yield return (_S_D, typeof(KillersOnActorDeath),            /**/ null);
            yield return (_S_D, typeof(ActorScoreTag),                  /**/ null);

            yield return (CS_D, typeof(ActorAttribute3<_Power>),        /**/ (e, m) => m.SetComponentData(e, new ActorAttribute3<_Power> { max = power, regain = powerRegain, value = power }));
            yield return (_S_D, typeof(ActorAttribute3Modifys<_Power>), /**/ null);

            yield return (_S_D, typeof(ShipSlotList),                   /**/ null);
            yield return (_S_D, typeof(ShipWeaponArray),                /**/ null);
            yield return (CS_D, typeof(Ship),                           /**/ null);
            yield return (CS_D, typeof(Battery),                        /**/ null);
        }

        public override void Init(EntityManager _entityManager, Transform root)
        {
            base.Init(_entityManager, root);

            if (isServer)
            {
                initComponentDatas += (e, m) => ShipSpawner.InitSlots(e, m, name, false);
            }
        }



        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            ushort m = 0;
            m |= datas.Position(entityManager.GetComponentData<Translation>(actorEntity).Value);
            m |= datas.Rotation(entityManager.GetComponentData<Rotation>(actorEntity).Value);
            return m;
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            entityManager.SetComponentData(actorEntity, new Translation { Value = datas.position });
            entityManager.SetComponentData(actorEntity, new Rotation { Value = datas.rotation });
        }
    }
}
