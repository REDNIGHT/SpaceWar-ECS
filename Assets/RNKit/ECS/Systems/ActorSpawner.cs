using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace RN.Network
{
    public interface IActorFx
    {
        //为了方便测试 下面两函数在server和client里都是执行
        //但是打包后版本只在client里执行
        void OnCreateFx(ActorSpawner actorSpawner);
        void OnDestroyFx(ActorSpawner actorSpawner);
    }

    public abstract class ActorSpawner : MonoBehaviour
    {
        public abstract short actorType { get; }
        public abstract string actorTypeName { get; }

        protected const int CS_M = 0;                   //C & S Message         信息          不会在初始化时被创建    如果被创建也只会存在一帧的时间
        protected const int C__M = 0;                   //client Message        信息          不会在初始化时被创建    如果被创建也只会存在一帧的时间
        protected const int _S_M = 0;                   //server Message        信息          不会在初始化时被创建    如果被创建也只会存在一帧的时间

        protected const int C__S = 0;                   //client State          状态          不会在初始化时被创建    存在时间根据逻辑而定
        protected const int _S_S = 0;                   //server State          状态          不会在初始化时被创建    存在时间根据逻辑而定

        protected const int _S_D = 1;                   //Server                在服务器创建
        protected const int C__D = 2;                   //Client                在客户端创建
        protected /* */ int CS_D { get; private set; }  //Client & Server       在服务器和客户端创建

        protected const int _S_P = 3;                   //Server Prefab         对服务器的Prefab进行处理  type不会参与CreateArchetype
        protected const int C__P = 4;                   //Client Prefab         对客户端的Prefab进行处理  type不会参与CreateArchetype

        public bool isServer { get; private set; }
        public bool isClient => !isServer;

        protected EntityArchetype _entityArchetype;
        public EntityArchetype entityArchetype => _entityArchetype;

        protected event Action<Entity, EntityManager> initComponentDatas;
        protected Type[] managedTypes;
        protected EntityManager entityManager;
        public Transform root { get; private set; }


        protected abstract IEnumerable<(int tag, Type type, Action<Entity, EntityManager> init)> ComponentDescriptions();

        public virtual void Init(EntityManager entityManager, Transform root)
        {
            this.entityManager = entityManager;
            this.root = root;

            //
            isServer = entityManager.World == ServerBootstrap.world;
            var tag = isServer ? _S_D : C__D;
            CS_D = tag;
            var all_cds = ComponentDescriptions();
            var cds = all_cds.Where(x => x.tag == tag && x.type != null).ToArray();

            _entityArchetype = entityManager.CreateArchetype(cds.Select(x => new ComponentType(x.type)).ToArray());

            cds.Where(x => x.init != null).forEach(x => initComponentDatas += x.init);

            managedTypes = cds.Where(x => x.type.IsClass).Select(x => x.type).ToArray();

            if (isServer)
            {
                all_cds.Where(x => x.init != null && x.tag == _S_P).forEach(x => initComponentDatas += x.init);
            }
            else
            {
                all_cds.Where(x => x.init != null && x.tag == C__P).forEach(x => initComponentDatas += x.init);
            }

            clientNeedActorId = all_cds.Any((x) => x.type == typeof(ActorId));
        }


        public Transform prefabInServer;
        public Transform prefabInClient;
        public Transform prefab => isServer ? prefabInServer : prefabInClient;



        internal Entity CreateEntity(int actorId, in ActorOwner actorOwner)
        {
            var actorEntity = entityManager.CreateEntity(entityArchetype);

#if UNITY_EDITOR
            var eName = name.Replace("_spawner", "") + ":" + actorEntity.Index
                + "  id:" + actorId
                + "  pid:" + actorOwner.playerId;
            entityManager.SetName(actorEntity, eName);
#endif

            if (prefab != null)
            {
                var go = CreateGameObject(root);
#if UNITY_EDITOR
                go.name = (isServer ? "s:" : "c:") + go.name + "  e:" + eName;
#endif

                var components = GetComponents(go);

                entityManager.AddComponentObject(actorEntity, go);

                foreach (var component in components)
                {
                    entityManager.AddComponentObject(actorEntity, component);
                }
            }

            entityManager.SetComponentData(actorEntity, actorOwner);

            if (isClient && actorId > 0)
            {
                entityManager.SetComponentData(actorEntity, new ActorId { value = actorId });
            }

            initComponentDatas(actorEntity, entityManager);

            return actorEntity;
        }

        protected IEnumerable<Component> GetComponents(GameObject go)
        {
            foreach (var type in managedTypes)
            {
                var component = go.GetComponent(type);
                if (component == null)
                {
                    Debug.LogError($"component == null  type:{type}  actorType:{actorTypeName}  ins:{go}", go);
                    continue;
                }
                yield return component;
            }
        }

        public virtual void DestroyEntityInClient(Entity actorEntity)
        {
            if (prefab != null)
            {
                DestroyGameObject(actorEntity);
            }

            //MessageSystem里进行删除
            //endCommandBuffer.DestroyEntity(actorEntity);
        }

        protected GameObject CreateGameObject(Transform root)
        {
            var ins = Instantiate(prefab, root);
#if UNITY_EDITOR
            ins.name = prefab.name;
#endif
            ins.gameObject.SetActive(true);

#if !UNITY_EDITOR
            if (isClient)
#endif
            {
                var actorFx = ins.GetComponent<IActorFx>();
                if (actorFx != null)
                {
                    actorFx.OnCreateFx(this);
                }
            }
            return ins.gameObject;
        }

        protected void DestroyGameObject(Entity actorEntity)
        {
            Debug.Assert(isClient == true, "isClient == true");

            var actorGO = entityManager.GetComponentObject<GameObject>(actorEntity);

            var actorFx = actorGO.GetComponent<IActorFx>();
            if (actorFx != null)
            {
                actorFx.OnDestroyFx(this);
                return;
            }

            Destroy(actorGO);
        }


        public void CreateSerializeInServer(Entity actorEntity, int ownerPlayerId, DynamicBuffer<NetworkReliableOutBuffer> outBuffer)
        {
            var s = new ActorCreateSerialize
            {
                ownerPlayerId = ownerPlayerId,
                actorType = actorType,
                actorId = clientNeedActorId ? actorEntity.Index : -1,
            };

            s.dataMask = OnCreateSerializeInServer(ref s.datas, actorEntity);

            s._DoSerialize(outBuffer);

            //Debug.LogWarning($"server =>  playerId:xxx  actorType:{actorType}  actorId:{s.actorId}");
        }


        //需要同步属性或同步删除的entity需要actorId
        //needActorId=false时 客户端的entity自己负责删除
        public bool clientNeedActorId { get; private set; }

        public abstract ushort OnCreateSerializeInServer(ref ActorCreateDatas datas, Entity actorEntity);
        public abstract void OnCreateDeserializeInClient(in ActorCreateDatas datas, Entity actorEntity);


        //
        public abstract int[] removeTransformIndexInServer { get; }

        /// <summary>
        /// 在服务器端 需要删除渲染和没用的组件...
        /// 客户端不需要
        /// </summary>
        public virtual void RemoveFxs()
        {
            if (prefabInServer != null)
            {
                prefabInServer = Instantiate(prefabInServer);
                prefabInServer.gameObject.SetActive(false);
                prefabInServer.transform.parent = transform;
                prefabInServer.name = prefabInServer.name.Replace("(Clone)", "");

                _RemoveFxsInServer();
            }
        }

        public void _RemoveFxsInServer()
        {
            //
            if (removeTransformIndexInServer != null)
            {
                List<Transform> removes = new List<Transform>();
                foreach (var index in removeTransformIndexInServer)
                {
                    Debug.Assert(index < prefabInServer.childCount, $"index={index}  prefabInServer.childCount={prefabInServer.childCount}  behaviour={prefabInServer.name}", prefabInServer);
                    removes.Add(prefabInServer.GetChild(index));
                }
                foreach (var r in removes)
                {
                    Destroy(r.gameObject);
                }
            }


            //
            var fx = prefabInServer.GetComponent<IActorFx>() as MonoBehaviour;
            if (fx != null)
                Destroy(fx);


            //
            foreach (var t in prefabInServer.GetComponentsInChildren<Transform>(true))
            {
                if (t == null)
                    continue;

                //
                var ps = t.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    //Debug.LogWarning($"prefabInServer={prefabInServer.name}  destory:{ps}  t.parent={t.parent}", t.parent);
                    Destroy(ps.gameObject);
                    continue;
                }

                var r = t.GetComponent<Renderer>();
                if (r != null)
                {
                    //Debug.LogWarning($"prefabInServer={prefabInServer.name}  destory:{r}  t.parent={t.parent}", t.parent);
                    Destroy(r.gameObject);
                    continue;
                }

                var mf = t.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    //Debug.LogWarning($"prefabInServer={prefabInServer.name}  destory:{mf}  t.parent={t.parent}", t.parent);
                    Destroy(mf.gameObject);
                    continue;
                }
            }
        }
    }
}