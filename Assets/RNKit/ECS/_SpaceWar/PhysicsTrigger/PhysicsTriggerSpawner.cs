using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class PhysicsTriggerSpawner : ActorSpawnerSpaceWar
    {
        [Header("Trigger")]
        public int triggerCount = -1;
        public TriggerResultState triggerResultStateInclude;

        [Header("Physics")]
        public float radius;
        public float force;
        public float tangentForce;
        public float torque;
        public ForceMode mode;

        public float drag;
        public float angularDrag;


        [Header("fx")]
        public TriggerResultState triggerFxResultStateInclude = TriggerResultState.None;

        public const int DestroyFx_TransformIndex = 0;
        public const int Model_TransformIndex = 1;

        public override int[] removeTransformIndexInServer => new int[] { DestroyFx_TransformIndex, Model_TransformIndex, };


        void OnValidate()
        {
            //
            if (triggerFxResultStateInclude == TriggerResultState.Stay)
            {
                Debug.LogError("triggerFxResultStateInclude == TriggerResultState.Stay", this);
                triggerFxResultStateInclude = TriggerResultState.None;
            }


            //
            if (_actorType > ActorTypes.__PhysicsTrigger_Begin__ && _actorType < ActorTypes.__PhysicsTrigger_End__)
                return;

            Debug.LogError($"{_actorType} > ActorTypes.__PhysicsTrigger_Begin__ && {_actorType} < ActorTypes.__PhysicsTrigger_End__", this);

            _actorType = ActorTypes.__PhysicsTrigger_Begin__ + 1;
        }

        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            yield return (CS_D, typeof(OnCreateMessage),                  /**/ null);
            yield return (CS_M, typeof(OnDestroyMessage),                 /**/ null);

            yield return (CS_D, typeof(Actor),                            /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),                       /**/ null);
            yield return (C__D, typeof(ActorId),                          /**/ null);

            yield return (_S_D, typeof(ActorVisibleDistanceOnCD),         /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { isUnlimited = true }));


            yield return (CS_D, typeof(Transform),                        /**/ null);
            yield return (CS_D, typeof(Translation),                      /**/ null);
            yield return (CS_D, typeof(Rotation),                         /**/ null);
            yield return (CS_D, typeof(Transform_In_OnCreate),            /**/ null);


            yield return (_S_D, typeof(OnPhysicsCallMessage),             /**/ null);
            if (triggerCount > 0)
                yield return (_S_D, typeof(Trigger),                      /**/ (e, m) => m.SetComponentData(e, new Trigger { includeResultState = triggerResultStateInclude, count = triggerCount }));


            //yield return (SP, typeof(GOEntity),                         /**/ GOEntity._Init);
            yield return (_S_P, typeof(OnTriggerWithoutInstalledWeapon),  /**/ (e, m) => OnTriggerWithoutInstalledWeapon._initComponent(e, m, triggerFxResultStateInclude == TriggerResultState.Enter, true, triggerFxResultStateInclude == TriggerResultState.Exit, true));
            yield return (C__P, typeof(OnTriggerWithoutInstalledWeapon),  /**/ OnTriggerWithoutInstalledWeapon._removeComponent);
            yield return (_S_D, typeof(PhysicsTriggerResults),            /**/ null);
            yield return (_S_D, typeof(PhysicsResults),                   /**/ null);

            if (radius > 0f)
                yield return (_S_D, typeof(RigidbodyForceByCenter),       /**/ (e, m) => m.SetComponentData(e, new RigidbodyForceByCenter { force = force, tangentForce = tangentForce, radius = radius, torque = torque }));
            else if (force > 0f)
                yield return (_S_D, typeof(RigidbodyForce),               /**/ (e, m) => m.SetComponentData(e, new RigidbodyForce { mode = mode }));

            if (drag > 0f)
                yield return (_S_D, typeof(RigidbodyLinearDragChange),    /**/ (e, m) => m.SetComponentData(e, new RigidbodyLinearDragChange { drag = drag }));
            if (angularDrag > 0f)
                yield return (_S_D, typeof(RigidbodyAngularDragChange),   /**/ (e, m) => m.SetComponentData(e, new RigidbodyAngularDragChange { drag = angularDrag }));


            if (triggerFxResultStateInclude != TriggerResultState.None)
                yield return (_S_D, typeof(PhysicsTriggerFx),             /**/ (e, m) => m.SetComponentData(e, new PhysicsTriggerFx { includeResultState = triggerFxResultStateInclude }));

            yield return (CS_D, typeof(PhysicsTrigger),                   /**/ (e, m) => m.SetComponentData(e, new PhysicsTrigger { force = force }));
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
