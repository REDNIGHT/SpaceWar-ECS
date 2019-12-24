using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class WeaponFirePrepareFxSpawner : ActorSpawnerSpaceWar
    {
        public override int[] removeTransformIndexInServer => null;

        void OnValidate()
        {
            _actorType = ActorTypes.WeaponFirePrepareFx;
        }

        ActorSyncCreateClientSystem actorClientSystem;
        public override void Init(EntityManager entityManager, Transform root)
        {
            base.Init(entityManager, root);

            if (isClient)
            {
                actorClientSystem = entityManager.World.GetExistingSystem<ActorSyncCreateClientSystem>();
                Debug.Assert(actorClientSystem != null, $"actorClientSystem != null  root={root}", root);
            }

            if (prefab != null)
            {
                Debug.LogError("prefab != null  WeaponFirePrepareFx用weapon现成的WeaponFirePrepareFx节点", this);
            }

            if (isServer)
            {
                sEntityArchetype = _entityArchetype;
            }
        }

        protected static EntityArchetype sEntityArchetype;
        public static void createInServer(EntityCommandBuffer.Concurrent middleCommandBuffer, int index,
            in Translation translation, Entity weaponEntity, Entity shipEntity, byte shieldLevel)
        {
            var entity = middleCommandBuffer.CreateEntity(index, sEntityArchetype);
            middleCommandBuffer.SetComponent(index, entity, new Actor { actorType = (short)ActorTypes.WeaponFirePrepareFx });
            middleCommandBuffer.SetComponent(index, entity, new ActorVisibleDistanceOnCD { unreliableOnCreate = true, isUnlimited = false });

            middleCommandBuffer.SetComponent(index, entity, translation);
            middleCommandBuffer.SetComponent(index, entity, new WeaponCreator { entity = weaponEntity });
            middleCommandBuffer.SetComponent(index, entity, new ActorCreator { entity = shipEntity });
            middleCommandBuffer.SetComponent(index, entity, new WeaponFirePrepareFx { level = shieldLevel });
        }

        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            yield return (CS_D, typeof(OnCreateMessage),          /**/ null);
            yield return (CS_D, typeof(OnDestroyMessage),         /**/ null);

            yield return (CS_D, typeof(Actor),                    /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),               /**/ null);
            //yield return (_C, typeof(ActorId),                /**/ null);//如果能移动ActorId

            yield return (_S_D, typeof(ActorVisibleDistanceOnCD), /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { unreliableOnCreate = true, isUnlimited = false }));

            yield return (_S_D, typeof(Translation),              /**/ null);
            //yield return (CS, typeof(Rotation),               /**/ null);

            yield return (CS_D, typeof(WeaponCreator),            /**/ null);
            yield return (CS_D, typeof(ActorCreator),             /**/ null);
            yield return (CS_D, typeof(WeaponFirePrepareFx),      /**/ null);
        }

        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            ushort m = 0;
            m |= datas.IntValueA(entityManager.GetComponentData<WeaponCreator>(actorEntity).entity.Index);
            m |= datas.IntValueB(entityManager.GetComponentData<ActorCreator>(actorEntity).entity.Index);

            var weaponFirePrepareFx = entityManager.GetComponentData<WeaponFirePrepareFx>(actorEntity);
            if (weaponFirePrepareFx.level > 0)
                m |= datas.ByteValueA(weaponFirePrepareFx.level);

            //m |= datas.Rotation(entityManager.GetComponentData<Rotation>(actorEntity).Value);
            return m;
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            if (actorClientSystem.actorEntityFromActorId.TryGetValue(datas.intValueA, out Entity creatorEntityA))
                entityManager.SetComponentData(actorEntity, new WeaponCreator { entity = creatorEntityA });
            else
                Debug.LogError($"actorClientSystem.actorEntityFromActorId.TryGetValue({datas.intValueA}, out Entity creatorEntityA) == false");

            if (actorClientSystem.actorEntityFromActorId.TryGetValue(datas.intValueB, out Entity creatorEntityB))
                entityManager.SetComponentData(actorEntity, new ActorCreator { entity = creatorEntityB });
            else
                Debug.LogError($"actorClientSystem.actorEntityFromActorId.TryGetValue({datas.intValueB}, out Entity creatorEntityB) == false");


            entityManager.SetComponentData(actorEntity, new WeaponFirePrepareFx { level = datas.byteValueA });

            //entityManager.SetComponentData(actorEntity, new Rotation { Value = datas.rotation });
        }
    }
}
