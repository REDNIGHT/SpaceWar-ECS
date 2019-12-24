using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    public interface IPlayerActorArray
    {
        int maxCount { get; }
        Entity this[int index] { get; set; }

        Entity mainActorEntity { get; set; }
    }


    //这buffer会存储重要的actor 会自动销毁的就不要存到这里
    //需要input的actor也可以放这里
    //hp等属性需要同步的也可以放这里
    [ServerNetworkEntity]
    [ClientPlayerEntity]
    public partial struct PlayerActorArray : IComponentData, IPlayerActorArray
    {
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public partial class PlayerActorArrayServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
            enterGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerEnterGameMessage>()/*, ComponentType.ReadOnly<Player>()*/ },
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });
        }
        EntityQuery enterGameQuery;


        [BurstCompile]
        [RequireComponentTag(typeof(NetworkDisconnectedMessage), typeof(Player))]
        struct QuitGameJob : IJobForEachWithEntity<PlayerActorArray>
        {
            public ComponentType OnDestroyMessage;
            //public ComponentType PlayerActorArray;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity _/*playerEntity*/, int index, ref PlayerActorArray playerActorArray)
            {
                if (playerActorArray.mainActorEntity != Entity.Null)
                {
                    endCommandBuffer.AddComponent(index, playerActorArray.mainActorEntity, OnDestroyMessage);

                    playerActorArray.mainActorEntity = Entity.Null;
                }

                //commandBuffer.RemoveComponent(index, playerEntity, PlayerActorArray);
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //enter Game
            if (enterGameQuery.IsEmptyIgnoreFilter == false)
            {
                EntityManager.AddComponent<PlayerActorArray>(enterGameQuery);
            }


            //quit Game
            inputDeps = new QuitGameJob
            {
                //PlayerActorArray = typeof(PlayerActorArray),
                OnDestroyMessage = typeof(OnDestroyMessage),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public partial class PlayerActorArrayClientSystem : ComponentSystem
    {
        partial void GetPlayerActorListMaxCount(ref int playerActorListMaxCount);

        PlayerClientSystem playerClientSystem;
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
            playerClientSystem = World.GetExistingSystem<PlayerClientSystem>();
        }

        protected override void OnUpdate()
        {
            var endCommandBuffer = endBarrier.CreateCommandBuffer();

            Entities
                .WithAllReadOnly<NetworkConnection, PlayerCreateNetMessages>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity, DynamicBuffer<PlayerCreateNetMessages> playerCreateNetMessages) =>
                {
                    foreach (var playerCreateNetMessage in playerCreateNetMessages)
                    {
                        if (playerClientSystem.playerEntityFromPlayerId.TryGetValue(playerCreateNetMessage.id, out Entity playerEntity) == false)
                        {
                            Debug.LogError($"{World.Name} => playerEntityFromPlayerId.TryGetValue(playerCreateNetMessage.id={playerCreateNetMessage.id}, out Entity playerEntity) == false");
                            endCommandBuffer.AddComponent<NetworkDisconnectedMessage>(entity);
                            break;
                        }

                        PostUpdateCommands.AddComponent<PlayerActorArray>(playerEntity);
                    }
                });
        }
    }
}
