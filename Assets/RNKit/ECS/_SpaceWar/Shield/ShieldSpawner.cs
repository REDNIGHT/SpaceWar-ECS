using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class ShieldSpawner : ActorSpawnerSpaceWar
    {
        public override int[] removeTransformIndexInServer => null;

        void OnValidate()
        {
            _actorType = ActorTypes.Shield;
        }

        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            yield return (CS_D, typeof(OnCreateMessage),                  /**/ null);
            yield return (CS_M, typeof(OnDestroyMessage),                 /**/ null);

            yield return (CS_D, typeof(Actor),                            /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),                       /**/ null);
            yield return (C__D, typeof(ActorId),                          /**/ null);

            yield return (CS_D, typeof(ActorLifetime),                    /**/ null);

            yield return (_S_D, typeof(ActorVisibleDistanceOnCD),         /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { isUnlimited = false }));
            //ShieldOnUpdateClientSystem已经代替下面的组件的作用
            //yield return (_S, typeof(ActorVisibleDistanceOnSync),     /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnSync { syncType = SyncActorType.R, perFrame = PerFrame._3 }));

            yield return (_S_P, typeof(EntityBehaviour),                  /**/ null);//通过OnCreateMessage初始化
            //yield return (CP, typeof(EntityBehaviour),                /**/ EntityBehaviour._removeComponent);

            //下面Transform 会在这条件下.WithAll<Shield, OnCreateMessage>()初始化  
            yield return (CS_D, typeof(Transform),                        /**/ null);
            yield return (_S_D, typeof(Transform_Out),                    /**/ null);//Transform 是ship的子节点
            yield return (_S_D, typeof(Translation),                      /**/ null);
            yield return (CS_D, typeof(Rotation),                         /**/ null);
            yield return (_S_D, typeof(Transform_In),                     /**/ null);
            //ShieldOnUpdateClientSystem已经代替下面的组件的作用
            //yield return (_C, typeof(TransformSmooth_In),             /**/ (e, m) => m.SetComponentData(e, new TransformSmooth_In { smoothTime = smoothTime_NORB, rotationLerpT = rotationLerpT_NORB }));


            yield return (C__D, typeof(ShieldRoot),                       /**/ null);


            //yield return (_S, typeof(ActorAttribute3Base<_HP>),       /**/ (e, m) => m.SetComponentData(e, new ActorAttribute3Base<_HP> { max = hp, regain = hpRegain }));
            yield return (CS_D, typeof(ActorAttribute3<_HP>),             /**/ null);
            yield return (_S_D, typeof(ActorAttribute3Modifys<_HP>),      /**/ null);
            yield return (_S_D, typeof(KillersOnActorDeath),              /**/ null);

            yield return (CS_D, typeof(WeaponCreator),                    /**/ null);
            yield return (CS_D, typeof(ActorCreator),                     /**/ null);
            yield return (CS_D, typeof(Shield),                           /**/ null);
            yield return (_S_D, typeof(Shield_R_Temp),                    /**/ null);
        }

        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            ushort m = 0;
            m |= datas.IntValueA(entityManager.GetComponentData<WeaponCreator>(actorEntity).entity.Index);
            m |= datas.IntValueB(entityManager.GetComponentData<ActorCreator>(actorEntity).entity.Index);

            m |= datas.HalfValueA((half)entityManager.GetComponentData<ActorLifetime>(actorEntity).value);
            //m |= datas.Hp(entityManager.GetComponentData<ActorAttribute3<_HP>>(actorEntity).value);
            m |= datas.Rotation(entityManager.GetComponentData<Rotation>(actorEntity).Value);
            m |= datas.HalfValueB((half)entityManager.GetComponentData<Shield>(actorEntity).curLevel);
            return m;
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            if (actorClientSystem.actorEntityFromActorId.TryGetValue(datas.intValueA, out Entity creatorEntityA))
                entityManager.SetComponentData(actorEntity, new WeaponCreator { entity = creatorEntityA });
            if (actorClientSystem.actorEntityFromActorId.TryGetValue(datas.intValueB, out Entity creatorEntityB))
                entityManager.SetComponentData(actorEntity, new ActorCreator { entity = creatorEntityB });

            entityManager.SetComponentData(actorEntity, new ActorLifetime { lifetime = datas.halfValueA, value = datas.halfValueA });
            //entityManager.SetComponentData(actorEntity, new ActorAttribute3<_HP> { max = datas.hp, regain = 0f, value = datas.hp });
            entityManager.SetComponentData(actorEntity, new Rotation { Value = datas.rotation });
            entityManager.SetComponentData(actorEntity, new Shield { curLevel = (short)datas.halfValueB });
        }

        ActorSyncCreateClientSystem actorClientSystem;
        public override void Init(EntityManager _entityManager, Transform root)
        {
            if (isClient)
            {
                actorClientSystem = _entityManager.World.GetExistingSystem<ActorSyncCreateClientSystem>();
            }

            if (prefab != null)
            {
                Debug.LogError("prefab != null  Shield用ship现成的Shield节点", this);
            }

            base.Init(_entityManager, root);
        }
    }
}
