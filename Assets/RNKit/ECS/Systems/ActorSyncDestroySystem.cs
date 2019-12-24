using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    //[ClientNetworkEntity]
    [ClientAutoClear]
    public struct ActorDestroySerializeNetMessage : IBufferElementData
    {
        public int actorId;//entity Index
    }
    public struct ActorDestroySerialize : NetworkSerializeUnsafe.ISerializer//在ActorSpawner.OnActorSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorDestroy;

        public int actorId;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new ActorDestroySerializeNetMessage { actorId = actorId }, commandBuffer);
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorSyncDestroyServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SyncDestroyJob : IJobForEach_BB<ObserverDestroyVisibleActorBuffer, NetworkReliableOutBuffer>
        {
            public void Execute(DynamicBuffer<ObserverDestroyVisibleActorBuffer> observerDestroyVisibleActorBuffer, DynamicBuffer<NetworkReliableOutBuffer> outBuffer)
            {
                for (int i = 0; i < observerDestroyVisibleActorBuffer.Length; ++i)
                {
                    var actorEntity = observerDestroyVisibleActorBuffer[i].actorEntity;

                    var s = new ActorDestroySerialize { actorId = actorEntity.Index };
                    s._DoSerialize(outBuffer);
                    //actorServerSystem.actorSpawner.OnDestroySerializeInServer(actorEntity, broadcastBuffer);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new SyncDestroyJob { }.Schedule(this, inputDeps);
        }
    }


    //----------------------------------------------------------------------------------------------------------------------

    [DisableAutoCreation]
    [AlwaysUpdateSystem]//OnUpdate()在开始的时候没有执行  是可能因为ComponentSystem.ShouldRunSystem()的规则 
    public class ActorSyncDestroyClientSystem : ComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        ActorSyncCreateClientSystem actorClientSystem;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
            actorClientSystem = World.GetExistingSystem<ActorSyncCreateClientSystem>();

            actorDestroyQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<ActorId>(), ComponentType.ReadOnly<OnDestroyMessage>() },
            });
            clearActorQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<Actor>() },
            });
        }

        EntityQuery actorDestroyQuery;
        EntityQuery clearActorQuery;
        protected override void OnUpdate()
        {
            /*
            Entities
                .WithAll<NetworkConnection, NetworkConnectedMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddComponent<ActorDestroySerializeNetMessage>(entity);
                });
            //没有用这段代码而是用下面代码 是可能因为ComponentSystem.ShouldRunSystem()的规则 开始一段时间跳过了OnUpdate()
            */
            Entities
                .WithAllReadOnly<NetworkConnection>()
                .WithNone<ActorDestroySerializeNetMessage>()
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddComponent<ActorDestroySerializeNetMessage>(entity);
                });


            //
            //接收服务器上发来的所有ActorDestroyNetMessage
            using (var actorDestroyNetMessages
                = Entities
                .WithAllReadOnly<NetworkConnection, ActorDestroySerializeNetMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .AllBufferElementToList<ActorDestroySerializeNetMessage>(Allocator.Temp))
            {
                for (int i = 0; i < actorDestroyNetMessages.Length; ++i)
                {
                    var actorId = actorDestroyNetMessages[i].actorId;

                    if (actorClientSystem.actorEntityFromActorId.TryGetValue(actorId, out Entity actorEntity))
                    {
                        EntityManager.AddComponent<OnDestroyMessage>(actorEntity);

                        actorClientSystem.actorEntityFromActorId.Remove(actorId);
                    }
                }
            }



            //
            if (actorDestroyQuery.IsEmptyIgnoreFilter == false)
            {
                using (var actorIds = actorDestroyQuery.ToComponentDataArray<ActorId>(Allocator.TempJob))
                {
                    for (var i = 0; i < actorIds.Length; ++i)
                    {
                        var actorId = actorIds[i].value;

                        /*
                        //客户端只能删除有ActorLifetime的actor  其他都必须通过服务器删除  //这个规则不需要了
                        if (EntityManager.HasComponent<ActorLifetime>(actorEntity) == false)
                        {
                            Debug.LogError($"{World.Name} => EntityManager.HasComponent<ActorLifetime>(actorEntity) == false  actorEntity:{EntityManager.GetName(actorEntity)}");
                        }
                        */

                        actorClientSystem.actorEntityFromActorId.Remove(actorId);
                    };
                }
            }



            //
            {
                //离开游戏场景后的处理
                //客户端断开后的处理
                var clearActor = false;
                Entities
                    .WithAllReadOnly<NetworkConnection>()
                    .WithAny<NetworkDisconnectedMessage>()
                    .ForEach((Entity _) =>
                    {
                        clearActor = true;
                    });


                //删除所有ActorElement
                if (clearActor)
                {
                    EntityManager.AddComponent<OnDestroyMessage>(clearActorQuery);

                    actorClientSystem.actorEntityFromActorId.Clear();
                }
            }
        }
    }
}