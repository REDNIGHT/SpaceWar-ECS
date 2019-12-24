using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class ShipLostInputFxSpawner : ActorSpawnerSpaceWar
    {
        public override int[] removeTransformIndexInServer => null;

        void OnValidate()
        {
            _actorType = ActorTypes.ShipLostInputFx;
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
                Debug.LogError("prefab != null  ShipLostInputFx用ship现成的ShipLostInputFx节点", this);
            }

            base.Init(entityManager, root);

            if (isServer)
            {
                sEntityArchetype = _entityArchetype;
            }
        }

        protected static EntityArchetype sEntityArchetype;
        public static void createInServer(EntityCommandBuffer.Concurrent middleCommandBuffer, int index,
            Entity shipEntity, in Translation translation, float lostInputTime)
        {
            var entity = middleCommandBuffer.CreateEntity(index, sEntityArchetype);
            middleCommandBuffer.SetComponent(index, entity, new Actor { actorType = (short)ActorTypes.ShipLostInputFx });
            middleCommandBuffer.SetComponent(index, entity, new ActorVisibleDistanceOnCD { unreliableOnCreate = true, isUnlimited = false });

            middleCommandBuffer.SetComponent(index, entity, new ActorCreator { entity = shipEntity });
            middleCommandBuffer.SetComponent(index, entity, translation);
            middleCommandBuffer.SetComponent(index, entity, new ShipLostInputFx { time = lostInputTime });
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
            yield return (CS_D, typeof(ShipLostInputFx),          /**/ null);
        }

        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            ushort m = 0;
            m |= datas.IntValueA(entityManager.GetComponentData<ActorCreator>(actorEntity).entity.Index);
            m |= datas.HalfValueA((half)entityManager.GetComponentData<ShipLostInputFx>(actorEntity).time);
            return m;
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            if (actorClientSystem.actorEntityFromActorId.TryGetValue(datas.intValueA, out Entity shipEntity))
            {
                entityManager.SetComponentData(actorEntity, new ActorCreator { entity = shipEntity });
                entityManager.SetComponentData(actorEntity, new ShipLostInputFx { time = datas.halfValueA });
            }
            else
            {
                Debug.LogError($"actorClientSystem.actorEntityFromActorId.TryGetValue({datas.intValueA}, out Entity shipEntity) == false");
            }
        }
    }
}
