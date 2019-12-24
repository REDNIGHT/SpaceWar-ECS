using Unity.Entities;

namespace RN.Network
{
    [ClientPlayerEntity]
    public struct PlayerWaitingMessage : IComponentData
    {
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerWaitingSystem : ComponentSystem
    {
        protected override void OnDestroy()
        {
        }
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            //todo...  排队进入游戏的功能

            /*var endCommandBuffer = endBarrier.CreateCommandBuffer();
            Entities
                .WithAll<NetworkStreamConnection, PlayerWaitingMessage>()
                .WithNone<NetworkStreamDisconnectedMessage>()
                .ForEach((Entity entity) =>
                {
                    endCommandBuffer.AddComponent(entity, new NetworkStreamDisconnectedMessage { error = (short)DisconnectedErrorsInComponent.PlayerWaiting });
                });*/
        }
    }

}