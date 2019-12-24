using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace RN.Network
{
    [ActorEntity]
    public struct Actor : IComponentData
    {
        public short actorType;
    }

    /// <summary>
    /// 这actor属于那个player创建的
    /// server创建的角色 Owner是null
    /// </summary>
    [ActorEntity]
    public struct ActorOwner : IComponentData
    {
        public Entity playerEntity;

        /// <summary>
        /// 在服务器playerId就是Entity.Index 在客户端playerId就不是 用的还是服务器发送过来的id
        /// </summary>
        public int playerId;

        public static readonly ActorOwner Null = default;
    }

    [ActorEntity]
    public struct ActorCreator : IComponentData
    {
        public Entity entity;
        //public int creatorId;
    }

    //只在客户端用
    [ActorEntity]
    public struct ActorId : IComponentData
    {
        public int value;
    }

    //
    [ClientNetworkEntity]
    [ClientAutoClear]
    //[InternalBufferCapacity(32)]
    public struct ActorCreateSerializeNetMessage : IBufferElementData
    {
        public int ownerPlayerId;
        public short actorType;
        public int actorId;//entity Index

        public ushort dataMask;
        public ActorCreateDatas datas;
    }

    //[StructLayout(LayoutKind.Explicit)]
    public partial struct ActorCreateDatas
    {
    }
    public partial struct ActorCreateSerialize : NetworkSerialize.ISerializer//在ActorSpawner.OnActorSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorCreate;

        public int ownerPlayerId;
        public short actorType;
        public int actorId;//entity Index
        public ushort dataMask;

        public ActorCreateDatas datas;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage
                (index, entity
                , new ActorCreateSerializeNetMessage { ownerPlayerId = ownerPlayerId, actorType = actorType, actorId = actorId, dataMask = dataMask, datas = datas }
                , commandBuffer);

            //Debug.Log($"ActorCreateSerialize  actorType={(SpaceWar.ActorTypes)actorType}");
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorSyncCreateServerSystem : ComponentSystem
    {
        IActorSpawnerMap actorSpawnerMap;

        protected void OnInit(Transform root)
        {
            actorSpawnerMap = root.GetComponentInChildren<IActorSpawnerMap>();
            Debug.Assert(actorSpawnerMap != null, $"actorSpawnerMap != null  root={root}", root);
        }

        public EntityQuery enterGameQuery;
        protected override void OnCreate()
        {
            enterGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerEnterGameMessage>(), typeof(NetworkReliableOutBuffer) },
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });
        }

        protected override void OnDestroy()
        {
            actorSpawnerMap = null;
        }

        protected override void OnUpdate()
        {
            //收到EnterGameMessage的链接 发送所有的actor信息到当前进入游戏的客户端
            if (enterGameQuery.IsEmptyIgnoreFilter == false)
            {
                using (var enterGamePlayer_outBuffers = enterGameQuery.ToBufferArray<NetworkReliableOutBuffer>(EntityManager, Allocator.TempJob))
                {
                    //发送必须new的actor信息到刚进入游戏的客户端  例如主角是肯定要发送的  例如子弹导弹等不重要并且会自动销毁的就不会发送给客户端
                    Entities
                        .WithAllReadOnly<Actor, ActorOwner, ActorVisibleDistanceOnCD>()
                        .ForEach((Entity actorEntity, ref Actor actor, ref ActorOwner actorOwner, ref ActorVisibleDistanceOnCD visibleDistanceOnCD) =>
                        {
                            for (int i = 0; i < enterGamePlayer_outBuffers.Length; ++i)
                            {
                                var outBuffer = enterGamePlayer_outBuffers[i];

                                if (visibleDistanceOnCD.isUnlimited)
                                {
                                    actorSpawnerMap.CreateSerializeInServer(actorEntity, actor.actorType, actorOwner.playerId, outBuffer);
                                }
                            }
                        });
                }
            }


            //发送刚创建的actor到所有客户端
            //Create Actors 2 All Clients
            Entities
                .WithAll<NetworkReliableOutBuffer, NetworkUnreliableOutBuffer>().WithAllReadOnly<Player, ObserverCreateVisibleActorBuffer>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((DynamicBuffer<ObserverCreateVisibleActorBuffer> createVisibleActorBuffer, DynamicBuffer<NetworkReliableOutBuffer> reliableOutBuffer, DynamicBuffer<NetworkUnreliableOutBuffer> unreliableOutBuffer) =>
                {
                    for (int i = 0; i < createVisibleActorBuffer.Length; ++i)
                    {
                        var actorEntity = createVisibleActorBuffer[i].actorEntity;
                        var actor = createVisibleActorBuffer[i].actor;
                        var ownerPlayerId = createVisibleActorBuffer[i].ownerPlayerId;//networkId就是PlayerId
                        var unreliable = createVisibleActorBuffer[i].unreliableOnCreate;

                        actorSpawnerMap.CreateSerializeInServer(actorEntity, actor.actorType, ownerPlayerId, unreliable ? unreliableOutBuffer.Reinterpret<NetworkReliableOutBuffer>() : reliableOutBuffer);
                    }
                });

        }
    }


    //----------------------------------------------------------------------------------------------------------------------

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorSyncCreateClientSystem : ComponentSystem
    {
        public int actorCountInHashMap = 1024;
        IActorSpawnerMap actorSpawnerMap;

        NativeHashMap<int, Entity> _actorEntityFromActorId;
        public NativeHashMap<int, Entity> actorEntityFromActorId => _actorEntityFromActorId;
        //public NativeArray<int> ActorEntitys => actorEntityFromPlayerId.GetKeyArray(Allocator.Temp);

        protected void OnInit(Transform root)
        {
            actorSpawnerMap = root.GetComponentInChildren<IActorSpawnerMap>();
            Debug.Assert(actorSpawnerMap != null, $"actorSpawnerMap != null  root={root}", root);
        }
        protected override void OnDestroy()
        {
            actorSpawnerMap = null;

            _actorEntityFromActorId.Dispose();
        }

        EndCommandBufferSystem endBarrier;
        PlayerClientSystem playerClientSystem;
        protected override void OnCreate()
        {
            playerClientSystem = World.GetExistingSystem<PlayerClientSystem>();
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();


            _actorEntityFromActorId = new NativeHashMap<int, Entity>(actorCountInHashMap, Allocator.Persistent);
        }

        /*public Entity CreateInClient(short actorType, int actorId, in ActorOwner actorOwner)
        {
            return actorSpawners.CreateInClient(actorType, actorId, actorOwner);
        }*/


        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<NetworkConnection, NetworkConnectedMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddComponent<ActorCreateSerializeNetMessage>(entity);
                });



            var endCommandBuffer = endBarrier.CreateCommandBuffer();

            //
            //接收服务器上发来的所有ActorCreateNetMessage
            var q = Entities
                .WithAllReadOnly<NetworkConnection, ActorCreateSerializeNetMessage>()
                .WithNone<NetworkDisconnectedMessage>();
            using (var actorCreateNetMessageList = q.AllBufferElementToList<ActorCreateSerializeNetMessage>(Allocator.Temp))
            using (var entitys = q.ToEntityQuery().ToEntityArray(Allocator.TempJob))
            {
                if (entitys.Length > 1)
                    Debug.LogError("entitys.Length > 1");

                for (int i = 0; i < actorCreateNetMessageList.Length; ++i)
                {
                    var actorType = actorCreateNetMessageList[i].actorType;
                    var playerId = actorCreateNetMessageList[i].ownerPlayerId;
                    var actorId = actorCreateNetMessageList[i].actorId;
                    var dates = actorCreateNetMessageList[i].datas;

                    Entity playerEntity;

                    if (playerId <= 0)//服务器自己创建的actor
                    {
                        playerEntity = Entity.Null;
                    }
                    else
                    {
                        if (playerClientSystem.playerEntityFromPlayerId.TryGetValue(playerId, out playerEntity) == false)
                        {
                            Debug.LogError($"{World.Name} => playerEntityFromPlayerId.TryGetValue(playerId={playerId}, out Entity playerEntity) == false");
                            endCommandBuffer.AddComponent<NetworkDisconnectedMessage>(entitys[0]);
                            break;
                        }
                    }

                    //Debug.LogWarning($"{World.Name} =>  playerId={playerId}  actorType={actorType}  actorId={actorId}  playerEntity={playerEntity}");

                    var actorEntity = actorSpawnerMap.CreateInClient(actorType, actorId, new ActorOwner { playerEntity = playerEntity, playerId = playerId }, dates);


                    //ActorSpawner.needActorId => false 时 id是-1
                    if (actorId <= 0)
                        continue;


                    //
                    if (_actorEntityFromActorId.TryAdd(actorId, actorEntity) == false)
                    {
                        //actorId是连续的 客户端的删除也有可能延迟 所以这里有可能会出现旧的actorEntity还没删除 所以这里可以直接覆盖掉
                        //还有一种情况是服务器的entity只有一阵的生命周期 而客户端的entity的生命周期找过一阵  服务器的id已经回收并且重新分配给新的entity  解决方案是把这类型的ActorSpawner.needActorId => false
                        Debug.LogWarning($"{World.Name} => actorEntityFromPlayerId.TryAdd(index, entity) == false  playerId={playerId}  actorType={actorType}  actorId={actorId}");


                        _actorEntityFromActorId[actorId] = actorEntity;
                    }
                }
            }

        }
    }
}