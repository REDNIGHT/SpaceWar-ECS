using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    //这里的规则是进入游戏就可以开始游戏了 不需要等待其他玩家


    //进入游戏后才会有Player 所以不需要下面的状态了
    /*[ServerNetworkEntity]
    public struct PlayerInGameState : IComponentData
    {
    }*/

    [ServerNetworkEntity]
    [ClientNetworkEntity]
    public struct PlayerEnterGameMessage : IComponentData
    {
    }
    /*[ServerNetworkEntity]
    [ClientNetworkEntity]
    public struct PlayerQuitGameMessage : IComponentData
    {
    }*/


    [ServerNetworkEntity]
    [ServerAutoClear]
    public struct PlayerEnterGameNetMessage : IComponentData
    {
    }

    public struct PlayerEnterGameSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.client2server;
        public short Type => (short)NetworkSerializeType.PlayerEnterGame;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.AddComponent(index, entity, new PlayerEnterGameNetMessage { });
        }
    }


    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct PlayerEnterGameResultNetMessage : IComponentData
    {
        public bool success;
    }
    public struct PlayerEnterGameResultSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.PlayerEnterGame;

        public bool success;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.AddComponent(index, entity, new PlayerEnterGameResultNetMessage { success = success });
        }
    }



    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerEnterGameServerSystem : ComponentSystem
    {
        public int playerMaxCount = 64;

        EndCommandBufferSystem endBarrier;
        PlayerWaitingSystem playerWaitingSystem;

        protected override void OnCreate()
        {
            playerWaitingSystem = World.GetExistingSystem<PlayerWaitingSystem>();

            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();

            playerEnterGameNetMessageQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<NetworkConnection>(), ComponentType.ReadOnly<PlayerEnterGameNetMessage>() },
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });
            playerInGameCountQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<Player>() },
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });
        }

        EntityQuery playerEnterGameNetMessageQuery;
        EntityQuery playerInGameCountQuery;

        protected override void OnUpdate()
        {
            //PlayerStateChangeNetMessage
            if (playerEnterGameNetMessageQuery.IsEmptyIgnoreFilter == false)
            {
                var playerInGameCount = playerInGameCountQuery.CalculateEntityCount();
                var endCommandBuffer = endBarrier.CreateCommandBuffer();

                if (playerInGameCount < playerMaxCount)//todo...  如果有两个玩家同时PlayerEnterGameNetMessage 会出现超出最大人数的情况
                {
                    Entities
                        .With(playerEnterGameNetMessageQuery)
                        .ForEach((Entity entity, DynamicBuffer<NetworkReliableOutBuffer> outBuffer) =>
                        {
                            if (EntityManager.HasComponent<Player>(entity) == false)
                            {
                                //
                                //进入游戏后才会有Player 所以不需要下面的PlayerInGameState状态了
                                //PostUpdateCommands.AddComponent(entity, new PlayerInGameState { });

                                PostUpdateCommands.AddComponent(entity, new PlayerEnterGameMessage { });
                                endCommandBuffer.RemoveComponent<PlayerEnterGameMessage>(entity);

                                var s = new PlayerEnterGameResultSerialize { success = true };
                                s._DoSerialize(outBuffer);
                            }
                            else
                            {
                                endCommandBuffer.AddComponent(entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrorsInSystem.PlayerStateA });
                            }
                        });
                }
                else
                {
                    //超出最大人数
                    Entities
                        .With(playerEnterGameNetMessageQuery)
                        .ForEach((Entity entity, DynamicBuffer<NetworkReliableOutBuffer> outBuffer) =>
                        {
                            var s = new PlayerEnterGameResultSerialize { success = false };
                            s._DoSerialize(outBuffer);

                            if (playerWaitingSystem != null)
                            {
                                PostUpdateCommands.AddComponent(entity, new PlayerWaitingMessage { });
                            }
                            else
                            {
                                endCommandBuffer.AddComponent(entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrorsInSystem.PlayerStateB });
                            }
                        });
                }
            }
        }

    }



    //----------------------------------------------------------------------------------------------------------------------


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerEnterGameClientSystem : ComponentSystem
    {
        public event System.Action<PlayerEnterGameResultNetMessage> playerEnterGameResult;

        protected override void OnDestroy()
        {
        }

        EndCommandBufferSystem endBarrier;
        protected void OnInit(Transform root)
        {
            World.GetExistingSystem<NetworkStreamConnectSuccessSystem>().AddSystem(this);

            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }


        public bool EnterGame()
        {
            if (Enabled == false)
            {
                Debug.LogError("Enabled == false  Connecting..."); //连接中...
                return false;
            }

            /*if (GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerInGameState>() },
            }).CalculateLength() > 0)
                return false;*/


            //
            Entities
                .WithAllReadOnly<NetworkConnection>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity, DynamicBuffer<NetworkReliableOutBuffer> outBuffer) =>
                {
                    var s = new PlayerEnterGameSerialize { };
                    s._DoSerialize(outBuffer);
                });

            return true;
        }


        protected override void OnUpdate()
        {
            var endCommandBuffer = endBarrier.CreateCommandBuffer();


            //Result
            Entities
                .WithAllReadOnly<NetworkConnection, PlayerEnterGameResultNetMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity, ref PlayerEnterGameResultNetMessage playerEnterGameResultNetMessage) =>
                {
                    if (playerEnterGameResultNetMessage.success)
                    {
                        //PostUpdateCommands.AddComponent(entity, new PlayerInGameState { });

                        PostUpdateCommands.AddComponent(entity, new PlayerEnterGameMessage { });
                        endCommandBuffer.RemoveComponent<PlayerEnterGameMessage>(entity);
                    }

                    playerEnterGameResult(playerEnterGameResultNetMessage);
                });
        }
    }
}