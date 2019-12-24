using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class AttributeModifyFxSpawner : ActorSpawnerSpaceWar
    {
        public float lifetime = 1f;
        public float upVelocity = 1f;
        public float drag = 0.1f;

        [System.Serializable]
        public struct HitFxInfo
        {
            public ActorTypes actorType;

            public Transform hitFxPrefab;
        }
        public Transform defaultHitFxPrefab;
        public HitFxInfo[] hitFxInfos;
        Dictionary<ActorTypes, Transform> hitFxInfoMap = new Dictionary<ActorTypes, Transform>();

        private void Awake()
        {
            foreach (var fxInfo in hitFxInfos)
            {
                if (hitFxInfoMap.ContainsKey(fxInfo.actorType) == true)
                {
                    Debug.LogError($"hitFxInfoMap.ContainsKey{fxInfo.actorType} == true");
                }

                hitFxInfoMap[fxInfo.actorType] = fxInfo.hitFxPrefab;
            }

            _actorType = (ActorTypes)actorType;
        }

        //public static new short actorType => (short)ActorTypes.AttributeModifyCountFx;

        public override int[] removeTransformIndexInServer => null;

        void OnValidate()
        {
            _actorType = ActorTypes.AttributeModifyCountFx;

            for (var i = 0; i < hitFxInfos.Length; ++i)
            {
                var actorType = hitFxInfos[i].actorType;

                if (actorType > ActorTypes.__Bullet_Begin__ && actorType < ActorTypes.__Bullet_End__)
                    continue;
                if (actorType > ActorTypes.__Laser_Begin__ && actorType < ActorTypes.__Laser_End__)
                    continue;
                if (actorType > ActorTypes.__Explosion_Begin__ && actorType < ActorTypes.__Explosion_End__)
                    continue;
                if (actorType > ActorTypes.__AttributeTrigger_Begin__ && actorType < ActorTypes.__AttributeTrigger_End__)
                    continue;
                //if (actorType > ActorTypes.__PhysicsTrigger_Begin__ && actorType < ActorTypes.__PhysicsTrigger_End__)//物理陷阱不提供伤害
                //    continue;

                Debug.LogError($"hitFxInfos[{i}].actorType != {hitFxInfos[i].actorType}", this);

                hitFxInfos[i].actorType = ActorTypes.None;
            }
        }

        ActorSyncCreateClientSystem actorClientSystem;
        public override void Init(EntityManager entityManager, Transform root)
        {
            if (isClient)
            {
                actorClientSystem = entityManager.World.GetExistingSystem<ActorSyncCreateClientSystem>();
            }

            if (prefabInServer != null)
            {
                Debug.LogError("prefabInServer != null  server 不需要prefab");
            }

            base.Init(entityManager, root);

            if (isServer)
            {
                sEntityArchetype = _entityArchetype;
            }
        }

        protected static EntityArchetype sEntityArchetype;
        public static void createInServer(EntityCommandBuffer.Concurrent endCommandBuffer, int index,
            short actorType, in Translation translation, float hp, float power, Entity targetEntity)
        {
            var entity = endCommandBuffer.CreateEntity(index, sEntityArchetype);
            endCommandBuffer.SetComponent(index, entity, new Actor { actorType = (short)ActorTypes.AttributeModifyCountFx });
            endCommandBuffer.SetComponent(index, entity, new ActorVisibleDistanceOnCD { unreliableOnCreate = true, isUnlimited = false });

            endCommandBuffer.SetComponent(index, entity, translation);
            endCommandBuffer.SetComponent(index, entity, new AttributeModifyFx { actorType = actorType, hp = (half)hp, power = (half)power, targetEntity = targetEntity });
        }

        protected override IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions()
        {
            yield return (CS_D, typeof(OnCreateMessage),          /**/ null);
            yield return (_S_D, typeof(OnDestroyMessage),         /**/ null);//server 只存在一帧

            yield return (CS_D, typeof(Actor),                    /**/ (e, m) => m.SetComponentData(e, new Actor { actorType = actorType }));
            yield return (CS_D, typeof(ActorOwner),               /**/ null);
            //yield return (_C, typeof(ActorId),                /**/ null);

            //AttributeModifyCountClientSystem需要ActorLifetime   
            yield return (C__D, typeof(ActorLifetime),            /**/ (e, m) => m.SetComponentData(e, new ActorLifetime { value = lifetime }));

            yield return (_S_D, typeof(ActorVisibleDistanceOnCD), /**/ (e, m) => m.SetComponentData(e, new ActorVisibleDistanceOnCD { unreliableOnCreate = true, isUnlimited = false }));

            yield return (C__D, typeof(Transform),                /**/ null);
            yield return (CS_D, typeof(Translation),              /**/ null);
            yield return (C__D, typeof(Transform_In),             /**/ null);
            yield return (C__D, typeof(TextMesh),                 /**/ null);

            yield return (C__D, typeof(ActorVelocity),            /**/ (e, m) => m.SetComponentData(e, new ActorVelocity { value = new float3(0f, upVelocity, 0f), drag = drag }));

            yield return (CS_D, typeof(AttributeModifyFx),        /**/ null);
        }


        //
        public override ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity)
        {
            var c = entityManager.GetComponentData<AttributeModifyFx>(actorEntity);

            ushort m = 0;
            Debug.Assert(c.actorType <= byte.MaxValue, "c.actorType <= byte.MaxValue", this);
            m |= datas.ByteValueA((byte)c.actorType);

            if (c.targetEntity != Entity.Null)
                m |= datas.IntValueB(c.targetEntity.Index);

            if (c.hp != 0f)
                m |= datas.HalfValueA(c.hp);
            if (c.power != 0f)
                m |= datas.HalfValueB(c.power);

            m |= datas.Position(entityManager.GetComponentData<Translation>(actorEntity).Value);
            return m;
        }

        public override void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity)
        {
            Entity targetEntity = Entity.Null;
            if (datas.intValueB > 0 && actorClientSystem.actorEntityFromActorId.TryGetValue(datas.intValueB, out targetEntity))
            {
                entityManager.SetComponentData(actorEntity, new AttributeModifyFx { actorType = (short)datas.intValueA, hp = datas.halfValueA, power = datas.halfValueB, targetEntity = targetEntity });
            }
            else
            {
                entityManager.SetComponentData(actorEntity, new AttributeModifyFx { actorType = (short)datas.intValueA, hp = datas.halfValueA, power = datas.halfValueB });
            }
            entityManager.SetComponentData(actorEntity, new Translation { Value = datas.position });


            //
            var textMesh = entityManager.GetComponentObject<TextMesh>(actorEntity);
            int hp = (int)datas.halfValueA;
            int power = (int)datas.halfValueB;
            var hpStr = hp > 0 ? "+" + hp : hp.ToString();
            var powerStr = power > 0 ? "+" + power : power.ToString();

            if (hp != 0 && power != 0)
            {
                textMesh.text = $"{hpStr}  {powerStr}";
            }
            else if (hp != 0)
            {
                textMesh.text = $"{hpStr}";
            }
            else if (power != 0)
            {
                textMesh.text = $"{powerStr}";
            }
            else
            {
                textMesh.text = "";
            }

            textMesh.transform.forward = Camera.main.transform.forward;




            //
            //if (datas.intValueA != 0)
            {
                if (hitFxInfoMap.TryGetValue((ActorTypes)datas.byteValueA, out var fxPrefab) == false)
                {
                    fxPrefab = defaultHitFxPrefab;
                }

                Transform fx = null;
                if (fxPrefab != null)
                {
                    float3 offset = root.position;
                    fx = GameObject.Instantiate(fxPrefab, offset + datas.position, Quaternion.identity, root);
                }

                if (targetEntity != Entity.Null)
                {
                    Debug.Assert(fx != null, "fx != null", this);

                    var targetT = entityManager.GetComponentObject<Transform>(targetEntity);
                    fx.parent = targetT;
                    fx.localPosition = Vector3.zero;
                    fx.localRotation = Quaternion.identity;
                }
            }
        }
    }
}
