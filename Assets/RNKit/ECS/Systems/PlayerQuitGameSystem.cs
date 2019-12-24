using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace RN.Network
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerQuitGameServerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            //todo...

            //
            //收到QuitGameMessage的链接  删除actor的信息 发送到所有客户端
            /*Entities
                .WithAll<Player>()
                .WithAny<NetworkStreamDisconnectedMessage>()
                .ForEach((Entity serverNetworkEntity, DynamicBuffer<ActorList> actorList) =>
                {
                    actorSpawner.OnQuitGameInServer(serverNetworkEntity, actorList);
                });*/
        }
    }
}
