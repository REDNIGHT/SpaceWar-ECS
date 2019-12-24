using Unity.Entities;
using UnityEngine;

namespace RN.Network
{
    //客户端不允许创建任何entity 全部通过input的数据发送到服务器来创建
    //在inputServerSysten里创建actor entity
    public interface IActorSpawnerMap
    {
        int actorTypeMaxCount { get; }
        ActorSpawner GetActorSpawner(short actorType);


        //
        Entity CreateInServer(short actorType, in ActorOwner actorOwner);
        //void DestroyInServer(Entity actorEntity, short actorType);
        void CreateSerializeInServer(Entity actorEntity, short actorType, int ownerPlayerId, DynamicBuffer<NetworkReliableOutBuffer> outBuffer);


        //
        Entity CreateInClient(short actorType, int actorId, in ActorOwner actorOwner, in ActorCreateDatas dates);
        void DestroyInClient(Entity actorEntity, short actorType);
    }


    public partial class ActorSpawnerMap : MonoBehaviour, IActorSpawnerMap
    {
        public EntityManager entityManager { get; protected set; }


        partial void GetActorTypeName(short actorType, ref string actorTypeName);
        partial void GetActorTypeMaxCount(ref int actorTypeMaxCount);
        public int actorTypeMaxCount { get { var c = 0; GetActorTypeMaxCount(ref c); return c; } }

        public bool forceRemoveFxsInServer = false;
        public ActorSpawner[] actorSpawners;
        ActorSpawner[] actorSpawnerMap;
        public Transform root;

        private void Awake()
        {
            if (root == null)
                root = transform.parent;
        }

        public void OnWorldInitialized(World world)
        {
            this.entityManager = world.EntityManager;


            //
            Debug.Assert(actorSpawnerMap == null, "actorSpawnerMap == null", this);
            actorSpawnerMap = new ActorSpawner[actorTypeMaxCount];

            foreach (var actorSpawner in actorSpawners)
            {
                var actorType = actorSpawner.actorType;
                if (actorSpawnerMap[actorType] != null)
                {
                    var actorTypeName = "";
                    GetActorTypeName(actorType, ref actorTypeName);
                    Debug.LogError($"actorSpawnerMap[{actorTypeName}] != null");
                    Debug.LogError($"actorSpawnerMap[actorType]:{actorSpawnerMap[actorType]}", actorSpawnerMap[actorType]);
                    Debug.LogError($"actorSpawner:{actorSpawner}", actorSpawner);
                    return;
                }

                Debug.Assert(actorSpawner.transform.localScale == Vector3.one,
                    $"actorSpawner.localScale == Vector3.one  actorSpawner.localScale={actorSpawner.transform.localScale}",
                    actorSpawner);
                Debug.Assert(actorSpawner.transform.localRotation == Quaternion.identity,
                    $"actorSpawner.localRotation == Quaternion.identityne  actorSpawner.localRotation={actorSpawner.transform.localRotation}",
                    actorSpawner);

                actorSpawnerMap[actorType] = Instantiate(actorSpawner, transform);
                actorSpawnerMap[actorType].name = actorSpawner.name;
                actorSpawnerMap[actorType].Init(entityManager, root);
                actorSpawnerMap[actorType].gameObject.SetActive(false);

                if (forceRemoveFxsInServer)
                {
                    actorSpawnerMap[actorType].RemoveFxs();
                }
            }
        }

        public ActorSpawner GetActorSpawner(short actorType)
        {
            var actorSpawner = actorSpawnerMap[actorType];
            Debug.Assert(actorSpawner != null, $"actorSpawner != null  actorType={actorType}", this);

            return actorSpawner;
        }

        public Entity CreateInServer(short actorType, in ActorOwner actorOwner)
        {
            var actorSpawner = actorSpawnerMap[actorType];
            Debug.Assert(actorSpawner != null, $"actorSpawner != null  actorType={actorType}", this);

            return actorSpawner.CreateEntity(-1, actorOwner);
        }

        /*public void DestroyInServer(Entity actorEntity, short actorType)
        {
            var actorSpawner = actorSpawnerMap[actorType];
            Debug.Assert(actorSpawner != null, $"actorSpawner != null  actorType={actorType}", this);

            actorSpawner.DestroyEntityInClient(actorEntity);
        }*/





        //
        public void CreateSerializeInServer(Entity actorEntity, short actorType, int ownerPlayerId, DynamicBuffer<NetworkReliableOutBuffer> outBuffer)
        {
            var actorSpawner = actorSpawnerMap[actorType];
            Debug.Assert(actorSpawner != null, $"actorSpawner != null  actorType={actorType}", this);

            actorSpawner.CreateSerializeInServer(actorEntity, ownerPlayerId, outBuffer);
        }

        //
        public Entity CreateInClient(short actorType, int actorId, in ActorOwner actorOwner, in ActorCreateDatas dates)
        {
            var actorSpawner = actorSpawnerMap[actorType];
            Debug.Assert(actorSpawner != null, $"actorSpawner != null  actorType={actorType}", this);

            var actorEntity = actorSpawner.CreateEntity(actorId, actorOwner);

            actorSpawner.OnCreateDeserializeInClient(dates, actorEntity);

            return actorEntity;
        }

        /*public Entity CreateInClient(short actorType, int actorId, in ActorOwner actorOwner, Transform root, EntityCommandBuffer endCommandBuffer)
        {
            var actorSpawner = actorSpawnerMap[actorType];

            return actorSpawner.CreateEntity(actorId, actorOwner, root, endCommandBuffer);
        }*/

        public void DestroyInClient(Entity actorEntity, short actorType)
        {
            var actorSpawner = actorSpawnerMap[actorType];
            Debug.Assert(actorSpawner != null, $"actorSpawner != null  actorType={actorType}", this);

            actorSpawner.DestroyEntityInClient(actorEntity);
        }
    }


}
