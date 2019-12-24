using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class LaserSpawner : ActorSpawnerSpaceWar
    {
        public float lifetime = 0f;

        [Header("Trigger")]
        public float delay = 0f;
        public float interval = 0.25f;
        public int maxCount = 10;

        [Header("Physics")]
        public float distance = 5f;
        public float radius = 0f;
        public LayerMask layerMask;
        public bool raycastAll;
        public float force = 5f;

        [Header("hp offset")]
        public float hpMinOffset = -5f;
        public float hpMaxOffset = -10f;


        public override int[] removeTransformIndexInServer => null;

        void OnValidate()
        {
            if (_actorType > ActorTypes.__Laser_Begin__ && _actorType < ActorTypes.__Laser_End__)
                return;

            Debug.LogError($"{_actorType} > ActorTypes.__Laser_Begin__ && {_actorType} < ActorTypes.__Laser_End__  this={this.name}", this);

            _actorType = ActorTypes.__Laser_Begin__ + 1;
        }

        ActorSyncCreateClientSystem actorClientSystem;
        public override void Init(EntityManager entityManager, Transform root)
        {
            if (isClient)
            {
                actorClientSystem = entityManager.World.GetExistingSystem<ActorSyncCreateClientSystem>();
            }

            base.Init(entityManager, root);
        }

        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            yield return (CS_D, typeof(OnCreateMessage),                  /**/ null);
            if (lifetime > 0f)
                yield return (CS_M, typeof(OnDestroyMessage),             /**/ null);
            else
                yield return (CS_D, typeof(OnDestroyWithoutMessage),      /**/ null);

            yield return (CS_D, typeof(Actor),                            /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),                       /**/ null);
            if (lifetime > 0)
                yield return (C__D, typeof(ActorId),                      /**/ null); //如果能移动 就需ActorId

            if (lifetime > 0f)
                yield return (CS_D, typeof(ActorLifetime),                /**/ (e, m) => m.SetComponentData(e, new ActorLifetime { lifetime = lifetime, value = lifetime }));

            yield return (_S_D, typeof(ActorVisibleDistanceOnCD),         /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { isUnlimited = false }));

            yield return (CS_D, typeof(Transform),                        /**/ null);
            yield return (CS_D, typeof(Translation),                      /**/ null);
            yield return (CS_D, typeof(Rotation),                         /**/ null);

            yield return (_S_D, typeof(Transform_In),                     /**/ null);
            yield return (C__D, typeof(TransformSmooth_In),               /**/ (e, m) => m.SetComponentData(e, new TransformSmooth_In { smoothTime = smoothTime_NORB, rotationLerpT = rotationLerpT_NORB }));

            if (lifetime > 0)
                yield return (_S_D, typeof(CallTrigger),                  /**/ (e, m) => m.SetComponentData(e, new CallTrigger(delay, interval, maxCount, typeof(OnPhysicsCallMessage))));
            else
                yield return (_S_D, typeof(OnPhysicsCallMessage),         /**/ null);

            if (radius <= 0f)
            {
                if (raycastAll)
                    yield return (_S_D, typeof(PhysicsRaycastAll),        /**/ (e, m) => m.SetComponentData(e, new PhysicsRaycastAll { distance = distance, layerMask = layerMask }));
                else
                    yield return (_S_D, typeof(PhysicsRaycast),           /**/ (e, m) => m.SetComponentData(e, new PhysicsRaycast { distance = distance, layerMask = layerMask }));
            }
            else
            {
                if (raycastAll)
                    yield return (_S_D, typeof(PhysicsSphereCastAll),     /**/ (e, m) => m.SetComponentData(e, new PhysicsSphereCastAll { radius = radius, distance = distance, layerMask = layerMask }));
                else
                    yield return (_S_D, typeof(PhysicsSphereCast),        /**/ (e, m) => m.SetComponentData(e, new PhysicsSphereCast { radius = radius, distance = distance, layerMask = layerMask }));
            }

            yield return (_S_D, typeof(PhysicsRaycastResults),            /**/ null);
            yield return (_S_D, typeof(RigidbodyForceAtPosition),         /**/ (e, m) => m.SetComponentData(e, new RigidbodyForceAtPosition { force = force }));

            yield return (CS_D, typeof(WeaponCreator),                    /**/ null);//WeaponFireFx需要WeaponCreator  LaserControlJobA也需要
            yield return (C__D, typeof(WeaponFireFx),                     /**/ null);

            yield return (_S_D, typeof(ActorAttribute3Offset<_HP>),       /**/ (e, m) => m.SetComponentData(e, new ActorAttribute3Offset<_HP> { maxOffset = hpMaxOffset, minOffset = hpMinOffset, scale = 1f }));

            yield return (CS_D, typeof(Laser),                            /**/ (e, m) => m.SetComponentData(e, new Laser { distance = distance }));
            yield return (_S_D, typeof(Laser_TR_Temp),                    /**/ null);
        }

        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            ushort m = 0;
            m |= datas.IntValueA(entityManager.GetComponentData<WeaponCreator>(actorEntity).entity.Index);
            m |= datas.Position(entityManager.GetComponentData<Translation>(actorEntity).Value);
            m |= datas.Rotation(entityManager.GetComponentData<Rotation>(actorEntity).Value);
            return m;
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            if (actorClientSystem.actorEntityFromActorId.TryGetValue(datas.intValueA, out Entity creatorEntity))
                entityManager.SetComponentData(actorEntity, new WeaponCreator { entity = creatorEntity });

            entityManager.SetComponentData(actorEntity, new Translation { Value = datas.position });
            entityManager.SetComponentData(actorEntity, new Rotation { Value = datas.rotation });
        }
    }
}
