using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace RN.Network
{
    [ServerNetworkEntity]
    public struct PlayerActorType : IComponentData
    {
        public byte value;
    }
    [ServerNetworkEntity]
    [ServerAutoClear]
    public struct PlayerActorSelectNetMessage : IComponentData
    {
        public byte value;
    }
    public struct PlayerActorSelectSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.client2server;
        public short Type => (short)NetworkSerializeType.PlayerActorSelect;

        public byte value;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.AddComponent(index, entity, new PlayerActorSelectNetMessage { value = value });
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerActorSelectServerSystem : ComponentSystem
    {
        public byte actorTypeBegin = 1;
        public byte actorTypeEnd = 2;


        EndCommandBufferSystem endBarrier;
        protected void OnInit(Transform root)
        {
            //
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();

            enterGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerEnterGameMessage>()/*, ComponentType.ReadOnly<Player>()*/ },
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });
        }
        public EntityQuery enterGameQuery;

        protected override void OnUpdate()
        {
            if (enterGameQuery.IsEmptyIgnoreFilter == false)
            {
                using (var enterGamePlayerEntitys = enterGameQuery.ToEntityArray(Allocator.TempJob))
                {
                    for (var i = 0; i < enterGamePlayerEntitys.Length; ++i)
                    {
                        if (EntityManager.HasComponent<PlayerActorType>(enterGamePlayerEntitys[i]) == false)
                        {
                            EntityManager.AddComponentData(enterGamePlayerEntitys[i], new PlayerActorType { value = actorTypeBegin });
                            //endBarrier.curCommandBuffer.AddComponent(enterGamePlayerEntitys[i], new PlayerActorType { value = 1 });
                        }
                    }
                }
            }


            //
            var endCommandBuffer = endBarrier.CreateCommandBuffer();
            Entities
                .WithAllReadOnly<NetworkConnection, PlayerActorSelectNetMessage>()//可以没有Player时修改PlayerActorType
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity playerEntity, ref PlayerActorSelectNetMessage playerActorSelectNetMessage) =>
                {
                    if (playerActorSelectNetMessage.value > actorTypeEnd || playerActorSelectNetMessage.value < actorTypeBegin)
                    {
                        endCommandBuffer.AddComponent(playerEntity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrorsInSystem.PlayerActorSelect });
                        return;
                    }

                    var playerActorType = new PlayerActorType { value = playerActorSelectNetMessage.value };

                    if (EntityManager.HasComponent<PlayerActorType>(playerEntity))
                    {
                        EntityManager.SetComponentData(playerEntity, playerActorType);
                    }
                    else
                    {
                        EntityManager.AddComponentData(playerEntity, playerActorType);
                        //endBarrier.curCommandBuffer.AddComponent(playerEntity, playerActorType);
                    }
                });
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerActorSelectClientSystem : ComponentSystem
    {
        protected void OnInit(Transform root)
        {
            World.GetExistingSystem<NetworkStreamConnectSuccessSystem>().AddSystem(this);
        }

        const string MyActorTypeKey = "MyActorType";
        public byte myActorType => (byte)PlayerPrefs.GetInt(MyActorTypeKey, 1);
        public void clearMyActorType() => PlayerPrefs.DeleteKey(MyActorTypeKey);

        public void ChangeMyActorType(byte actorType)
        {
            Entities
                .WithAllReadOnly<NetworkConnection>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((DynamicBuffer<NetworkReliableOutBuffer> outBuffer) =>
                {
                    var s = new PlayerActorSelectSerialize { value = actorType };
                    s._DoSerialize(outBuffer);

                    PlayerPrefs.SetInt(MyActorTypeKey, actorType);
                });
        }

        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<NetworkConnection, NetworkConnectedMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity _) =>
                {
                    //--------------
                    Enabled = false;
                    //--------------


                    if (PlayerPrefs.HasKey(MyActorTypeKey))
                    {
                        var actorType = PlayerPrefs.GetInt(MyActorTypeKey);
                        ChangeMyActorType((byte)actorType);
                    }
                });
        }
    }
}