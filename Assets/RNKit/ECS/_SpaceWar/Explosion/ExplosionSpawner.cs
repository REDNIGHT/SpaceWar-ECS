using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class ExplosionSpawner : ActorSpawnerSpaceWar
    {
        public float lifetime = 0f;


        [Header("Attribute")]
        public float delay = 0f;
        public float interval = 0.25f;
        public int maxCount = 10;

        [Header("Physics")]
        public float radius = 2.5f;
        public LayerMask layerMask;
        public float force = 5f;

        [Header("hp offset")]
        public float hpMinOffset = -5f;
        public float hpMaxOffset = -10f;

        public override int[] removeTransformIndexInServer => null;

        void OnValidate()
        {
            if (_actorType > ActorTypes.__Explosion_Begin__ && _actorType < ActorTypes.__Explosion_End__)
                return;

            Debug.LogError($"{_actorType} > ActorTypes.__Explosion_Begin__ && {_actorType} < ActorTypes.__Explosion_End__", this);

            _actorType = ActorTypes.__Explosion_Begin__ + 1;
        }


        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            if (lifetime > 0)
            {
                yield return (CS_D, typeof(OnCreateMessage),          /**/ null);
                yield return (CS_M, typeof(OnDestroyMessage),         /**/ null);

                yield return (CS_D, typeof(ActorLifetime),            /**/ (e, m) => m.SetComponentData(e, new ActorLifetime { lifetime = lifetime, value = lifetime }));
            }
            else
            {
                yield return (CS_D, typeof(OnCreateMessage),          /**/ null);
                yield return (CS_D, typeof(OnDestroyWithoutMessage),  /**/ null);
            }

            yield return (CS_D, typeof(Actor),                        /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),                   /**/ null);
            //yield return (_C, typeof(ActorId),                    /**/ null);//如果能移动 就需ActorId


            yield return (_S_D, typeof(ActorVisibleDistanceOnCD),     /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { isUnlimited = false }));
            //如果能移动 就需ActorSyncPerFrame ActorVisibleDistanceOnSync
            //yield return (_S, typeof(ActorVisibleDistanceOnSync), /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnSync { syncType = SyncType.P, perFrame = PerFrame._3, value = actorVisibleDistanceOnSync }));
            //yield return (_S, typeof(ActorSyncPerFrame),          /**/ (e, m) => m.SetComponentData(e, new ActorSyncPerFrame { perFrame = 4 }));

            yield return (C__D, typeof(Transform),                    /**/ null);
            yield return (CS_D, typeof(Translation),                  /**/ null);
            yield return (C__D, typeof(Transform_In_OnCreate),        /**/ null);

            if (lifetime > 0)
                yield return (_S_D, typeof(CallTrigger),              /**/ (e, m) => m.SetComponentData(e, new CallTrigger(delay, interval, maxCount, typeof(OnPhysicsCallMessage))));
            else
                yield return (_S_D, typeof(OnPhysicsCallMessage),     /**/ null);

            yield return (_S_D, typeof(PhysicsOverlapSphere),         /**/ (e, m) => m.SetComponentData(e, new PhysicsOverlapSphere { radius = radius, layerMask = layerMask, linecastFilter = true }));
            yield return (_S_D, typeof(PhysicsResults),               /**/ null);
            yield return (_S_D, typeof(PhysicsOverlapHitPoints),      /**/ null);
            yield return (_S_D, typeof(RigidbodyExplosionForce),      /**/ (e, m) => m.SetComponentData(e, new RigidbodyExplosionForce { force = force, radius = radius }));

            yield return (_S_D, typeof(ActorAttribute3Offset<_HP>),   /**/ (e, m) => m.SetComponentData(e, new ActorAttribute3Offset<_HP> { maxOffset = hpMaxOffset, minOffset = hpMinOffset, scale = 1f }));
            yield return (CS_D, typeof(Explosion),                    /**/ (e, m) => m.SetComponentData(e, new Explosion { radius = radius }));
        }

        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            return datas.Position(entityManager.GetComponentData<Translation>(actorEntity).Value);
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            entityManager.SetComponentData(actorEntity, new Translation { Value = datas.position });
        }
    }
}
