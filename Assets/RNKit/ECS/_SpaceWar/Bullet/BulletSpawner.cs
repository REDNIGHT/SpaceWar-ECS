using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class BulletSpawner : ActorSpawnerSpaceWar
    {
        public float lifetime = 1f;
        //public ActorTypes createOnDestroy;

        [Header("Physics")]
        public float velocity;
        public float mass = 0.05f;
        public float linearDrag = 0.1f;
        public const float angularDrag = 0.01f;

        [Header("PhysicsCast")]
        public LayerMask layerMask;
        public float radius = 0f;
        public float force = 5f;

        [Header("hp offset")]
        public float hpMinOffset = -5f;
        public float hpMaxOffset = -10f;

        public const int Raycast_TransformIndex = 0;
        public const int Trail_TransformIndex = 1;

        public override int[] removeTransformIndexInServer => new int[] { Trail_TransformIndex, };

        void OnValidate()
        {
            if (_actorType > ActorTypes.__Bullet_Begin__ && _actorType < ActorTypes.__Bullet_End__)
                return;

            Debug.LogError($"{_actorType} > ActorTypes.__Bullet_Begin__ && {_actorType} < ActorTypes.__Bullet_End__", this);

            _actorType = ActorTypes.__Bullet_Begin__ + 1;
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
            yield return (CS_D, typeof(OnCreateMessage),              /**/ null);
            yield return (CS_M, typeof(OnDestroyMessage),             /**/ null);

            yield return (CS_D, typeof(Actor),                        /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),                   /**/ null);
            yield return (C__D, typeof(ActorId),                      /**/ null);

            yield return (CS_D, typeof(ActorLifetime),                /**/ (e, m) => m.SetComponentData(e, new ActorLifetime { lifetime = lifetime, value = lifetime }));

            yield return (_S_D, typeof(ActorVisibleDistanceOnCD),     /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { isUnlimited = false }));
            yield return (_S_D, typeof(ActorVisibleDistanceOnSync),   /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnSync { syncType = SyncActorType.RB_VelocityDirection, perFrame = PerFrame._2 }));

            yield return (_S_P, typeof(EntityBehaviour),              /**/ EntityBehaviour._initComponent);
            yield return (C__P, typeof(EntityBehaviour),              /**/ EntityBehaviour._removeComponent);

            yield return (CS_D, typeof(Transform),                    /**/ null);
            yield return (_S_D, typeof(Transform_Out),                /**/ null);//有刚体才需要有Transform_Out 需要把刚体模拟后的坐标传回给Translation,Rotation, 只有服务器需要
            yield return (CS_D, typeof(Translation),                  /**/ null);
            yield return (CS_D, typeof(Rotation),                     /**/ null);
            yield return (CS_D, typeof(Transform_In_OnCreate),        /**/ null);//通过控制刚体来控制Transform 所以这里只需要初始化时给Transform赋值就可以了

            yield return (CS_D, typeof(Rigidbody),                    /**/ (e, m) => _initRigidbody(e, m, mass, linearDrag, angularDrag));
            yield return (_S_D, typeof(Rigidbody_Out),                /**/ null);
            yield return (CS_D, typeof(RigidbodyVelocity),            /**/ null);
            //yield return (CS, typeof(RigidbodyDrag),              /**/ (e, m) => m.SetComponentData(e, new RigidbodyDrag { linearDrag = linearDrag, angularDrag = angularDrag }));
            yield return (CS_D, typeof(Rigidbody_In),                 /**/ null);

            yield return (_S_D, typeof(OnPhysicsCallMessage),         /**/ null);
            yield return (_S_D, typeof(ChildTransform_Out),               /**/ (e, m) => m.SetComponentData(e, new ChildTransform_Out { childIndex0 = Raycast_TransformIndex, childIndex1 = -1, }));
            yield return (_S_D, typeof(LastPosition),                 /**/ null);

            if (radius <= 0f)
            {
                yield return (_S_D, typeof(PhysicsLinecast),          /**/ (e, m) => m.SetComponentData(e, new PhysicsLinecast { layerMask = layerMask }));
            }
            else
            {
                yield return (_S_D, typeof(PhysicsSphereCast),        /**/ (e, m) => m.SetComponentData(e, new PhysicsSphereCast { radius = radius, layerMask = layerMask }));
            }

            yield return (_S_D, typeof(PhysicsRaycastResults),        /**/ null);
            yield return (_S_D, typeof(RigidbodyForceAtPosition),     /**/ (e, m) => m.SetComponentData(e, new RigidbodyForceAtPosition { force = force }));

            yield return (CS_D, typeof(WeaponCreator),                /**/ null);
            yield return (C__D, typeof(WeaponFireFx),                 /**/ null);


            yield return (_S_D, typeof(ActorAttribute3Offset<_HP>),   /**/ (e, m) => m.SetComponentData(e, new ActorAttribute3Offset<_HP> { maxOffset = hpMaxOffset, minOffset = hpMinOffset, scale = 1f }));

            yield return (CS_D, typeof(Bullet),                       /**/ (e, m) => m.SetComponentData(e, new Bullet { velocity = velocity }));
        }


        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            ushort m = 0;
            m |= datas.IntValueA(entityManager.GetComponentData<WeaponCreator>(actorEntity).entity.Index);
            m |= datas.Position(entityManager.GetComponentData<Translation>(actorEntity).Value);
            m |= datas.Rotation(entityManager.GetComponentData<Rotation>(actorEntity).Value);

            var rigidbodyVelocity = entityManager.GetComponentData<RigidbodyVelocity>(actorEntity);
            m |= datas.LinearVelicity(rigidbodyVelocity.linear);
            m |= datas.AngularVelicity(rigidbodyVelocity.angular);
            return m;
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            if (actorClientSystem.actorEntityFromActorId.TryGetValue(datas.intValueA, out Entity creatorEntity))
                entityManager.SetComponentData(actorEntity, new WeaponCreator { entity = creatorEntity });

            entityManager.SetComponentData(actorEntity, new Translation { Value = datas.position });
            entityManager.SetComponentData(actorEntity, new Rotation { Value = datas.rotation });

            entityManager.SetComponentData(actorEntity, new RigidbodyVelocity { linear = datas.linearVelicity, angular = datas.angularVelicity });
        }
    }
}
