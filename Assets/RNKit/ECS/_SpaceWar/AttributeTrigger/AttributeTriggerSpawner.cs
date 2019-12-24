using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class AttributeTriggerSpawner : ActorSpawnerSpaceWar
    {
        [Header("Trigger")]
        public int triggerCount = -1;

        [Header("Message Trigger")]
        public float delay = 0f;
        public float interval = -1f;
        public int maxCount = -1;

        [Header("hp offset")]
        public float hpMinOffset = -5f;
        public float hpMaxOffset = -10f;

        public const int DestroyFx_TransformIndex = 0;
        public const int Model_TransformIndex = 1;

        public override int[] removeTransformIndexInServer => new int[] { DestroyFx_TransformIndex, Model_TransformIndex, };


        void OnValidate()
        {
            if (_actorType > ActorTypes.__AttributeTrigger_Begin__ && _actorType < ActorTypes.__AttributeTrigger_End__)
                return;

            Debug.LogError($"{_actorType} > ActorTypes.__AttributeTrigger_Begin__ && {_actorType} < ActorTypes.__AttributeTrigger_End__", this);

            _actorType = ActorTypes.__AttributeTrigger_Begin__ + 1;
        }

        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            yield return (CS_D, typeof(OnCreateMessage),              /**/ null);
            yield return (CS_M, typeof(OnDestroyMessage),             /**/ null);

            yield return (CS_D, typeof(Actor),                        /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),                   /**/ null);
            yield return (C__D, typeof(ActorId),                      /**/ null);

            yield return (_S_D, typeof(ActorVisibleDistanceOnCD),     /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { isUnlimited = true }));


            yield return (CS_D, typeof(Transform),                    /**/ null);
            yield return (CS_D, typeof(Translation),                  /**/ null);
            yield return (CS_D, typeof(Rotation),                     /**/ null);
            yield return (CS_D, typeof(Transform_In_OnCreate),        /**/ null);

            yield return (_S_P, typeof(OnTrigger),                    /**/ OnTrigger._initComponent);
            yield return (C__P, typeof(OnTrigger),                    /**/ OnTrigger._removeComponent);
            yield return (_S_D, typeof(PhysicsTriggerResults),        /**/ null);
            if (interval > 0f)
            {
                yield return (_S_D, typeof(CallTrigger),              /**/ (e, m) => m.SetComponentData(e, new CallTrigger(delay, interval, maxCount, typeof(OnPhysicsCallMessage))));
                yield return (CS_M, typeof(OnPhysicsCallMessage),     /**/ null);
            }
            else
            {
                yield return (_S_D, typeof(OnPhysicsCallMessage),     /**/ null);
            }
            if (triggerCount > 0)
                yield return (_S_D, typeof(Trigger),                  /**/ (e, m) => m.SetComponentData(e, new Trigger { count = triggerCount }));


            yield return (_S_D, typeof(ActorAttribute3Offset<_HP>),   /**/ (e, m) => m.SetComponentData(e, new ActorAttribute3Offset<_HP> { maxOffset = hpMaxOffset, minOffset = hpMinOffset, scale = 1f }));
            yield return (CS_D, typeof(AttributeTrigger),             /**/ null);
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
