using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class AccelerateFxSpawner : ActorSpawnerSpaceWar
    {
        public override int[] removeTransformIndexInServer => null;

        void OnValidate()
        {
            _actorType = ActorTypes.AccelerateFx;
        }

        ActorSyncCreateClientSystem actorClientSystem;
        public override void Init(EntityManager entityManager, Transform root)
        {
            if (isClient)
            {
                actorClientSystem = entityManager.World.GetExistingSystem<ActorSyncCreateClientSystem>();
            }

            if (prefab != null)
            {
                Debug.LogError("prefab != null  AccelerateFx用ship现成的AccelerateFx节点", this);
            }

            base.Init(entityManager, root);

            if (isServer)
            {
                sEntityArchetype = _entityArchetype;
            }
        }

        protected static EntityArchetype sEntityArchetype;
        public static void createInServer(EntityCommandBuffer.Concurrent middleCommandBuffer, int index,
            Entity shipEntity, in Translation translation)
        {
            var entity = middleCommandBuffer.CreateEntity(index, sEntityArchetype);
            middleCommandBuffer.SetComponent(index, entity, new Actor { actorType = (short)ActorTypes.AccelerateFx });
            middleCommandBuffer.SetComponent(index, entity, new ActorVisibleDistanceOnCD { unreliableOnCreate = true, isUnlimited = false });

            middleCommandBuffer.SetComponent(index, entity, new ActorCreator { entity = shipEntity });
            middleCommandBuffer.SetComponent(index, entity, translation);
        }

        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            yield return (CS_D, typeof(OnCreateMessage),          /**/ null);
            yield return (CS_D, typeof(OnDestroyMessage),         /**/ null);

            yield return (CS_D, typeof(Actor),                    /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),               /**/ null);
            //yield return (_C, typeof(ActorId),                /**/ null);

            yield return (_S_D, typeof(ActorVisibleDistanceOnCD), /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { unreliableOnCreate = true, isUnlimited = false }));

            yield return (_S_D, typeof(Translation),              /**/ null);//虽然坐标不需要发送到客户端 但是在服务器这边需要判断可视距离

            yield return (CS_D, typeof(ActorCreator),             /**/ null);
            yield return (CS_D, typeof(AccelerateFx),             /**/ null);
        }

        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            ushort m = 0;
            m |= datas.IntValueA(entityManager.GetComponentData<ActorCreator>(actorEntity).entity.Index);
            return m;
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            if (actorClientSystem.actorEntityFromActorId.TryGetValue(datas.intValueA, out Entity creatorEntity))
                entityManager.SetComponentData(actorEntity, new ActorCreator { entity = creatorEntity });
        }
    }
}
