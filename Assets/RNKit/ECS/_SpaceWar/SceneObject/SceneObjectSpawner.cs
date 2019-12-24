using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    //server 在场景中已经创建好的动态物体 例如刚体
    public class SceneObjectSpawner : ActorSpawnerSpaceWar
    {
        public override int[] removeTransformIndexInServer => null;

        void OnValidate()
        {
            if (_actorType > ActorTypes.__SceneObject_Begin__ && _actorType < ActorTypes.__SceneObject_End__)
                return;

            Debug.LogError($"{_actorType} > ActorTypes.__SceneObject_Begin__ && {_actorType} < ActorTypes.__SceneObject_End__  this={this.name}", this);

            _actorType = ActorTypes.__SceneObject_Begin__ + 1;
        }

        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            return new (int tag, Type type, Action<Entity, EntityManager> init)[]
            {
                (CS_D, typeof(OnCreateMessage),               /**/ null),
                (CS_M, typeof(OnDestroyMessage),              /**/ null),

                (CS_D, typeof(Actor),                         /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType })),
                (CS_D, typeof(ActorOwner),                    /**/ null),
                (C__D, typeof(ActorId),                       /**/ null),

                (_S_D, typeof(ActorVisibleDistanceOnCD),      /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { isUnlimited = true })),
                (_S_D, typeof(ActorVisibleDistanceOnSync),    /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnSync { syncType = SyncActorType.RB_Translation_Rotation_Velocity, perFrame = PerFrame._3 })),

                (_S_P, typeof(EntityBehaviour),               /**/ EntityBehaviour._initComponent),
                (C__P, typeof(EntityBehaviour),               /**/ EntityBehaviour._removeComponent),

                (CS_D, typeof(Transform),                     /**/ null),
                (_S_D, typeof(Transform_Out),                 /**/ null),//有刚体才需要有Transform_Out 需要把刚体模拟后的坐标传回给Translation,Rotation, 只有服务器需要
                (CS_D, typeof(Translation),                   /**/ null),
                (CS_D, typeof(Rotation),                      /**/ null),
                (_S_D, typeof(Transform_In_OnCreate),         /**/ null),//通过控制刚体来控制Transform 所以这里只需要初始化时给Transform赋值就可以了
                (C__D, typeof(TransformSmooth_In),            /**/ (e, m) => m.SetComponentData(e, new TransformSmooth_In { smoothTime = smoothTime, rotationLerpT = rotationLerpT })),

                (CS_D, typeof(Rigidbody),                     /**/ null),
                (_S_D, typeof(Rigidbody_Out),                 /**/ null),
                (CS_D, typeof(RigidbodyVelocity),             /**/ null),
                (CS_D, typeof(Rigidbody_In),                  /**/ null),
                //(_S, typeof(RigidbodyAutoSleep),          /**/ null),

                //(CS, typeof(IEntityInit),                 /**/ null),
                (_S_D, typeof(SceneObjectAutoReset),          /**/ null),
                };
        }

        /*static void initSceneObject(Entity e, EntityManager m)
        {
            var t = m.GetComponentObject<Transform>(e);
            m.SetComponentData(e, new SceneObjectAutoReset { defaultPosition = t.position, defaultRotation = t.rotation });

            Debug.Log($"initSceneObject t.position={t.position}");

            m.SetComponentData(e, new Translation { Value = t.position });
            m.SetComponentData(e, new Rotation { Value = t.rotation });
        }*/

        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            ushort m = 0;
            m |= datas.Position(entityManager.GetComponentData<Translation>(actorEntity).Value);
            m |= datas.Rotation(entityManager.GetComponentData<Rotation>(actorEntity).Value);

            /*var rbv = entityManager.GetComponentData<RigidbodyVelocity>(actorEntity);
            if (rbv.linear.Equals(float3.zero) == false)
                m |= datas.LinearVelicity(rbv.linear);
            if (rbv.angular.Equals(float3.zero) == false)
                m |= datas.AngularVelicity(rbv.angular);*/

            return m;
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            entityManager.SetComponentData(actorEntity, new Translation { Value = datas.position });
            entityManager.SetComponentData(actorEntity, new Rotation { Value = datas.rotation });

            //entityManager.SetComponentData(actorEntity, new RigidbodyVelocity { linear = datas.linearVelicity, angular = datas.angularVelicity });
        }
    }
}
