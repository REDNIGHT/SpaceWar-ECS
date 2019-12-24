using Unity.Entities;
using UnityEngine;

namespace RN.Network
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public partial class NetworkStreamStateSystem : ComponentSystem
    {
        partial void getErrorInfo(short error, ref string errorStr);

        public event System.Action connectHandle;
        public event System.Action connectedHandle;
        public event System.Action<string/*error*/> disconnectedHandle;

        private NetworkStreamSystem networkStreamSystem;
        protected override void OnCreate()
        {
            networkStreamSystem = World.GetExistingSystem<NetworkStreamSystem>();
        }

        protected override void OnUpdate()
        {
            //connect
            Entities
                .WithAllReadOnly<NetworkConnection, NetworkConnectMessage>()
                .ForEach((Entity _) =>
                {
                    if (connectHandle != null)
                    {
                        connectHandle();
                    }
                });

            //connected
            Entities
                .WithAllReadOnly<NetworkConnection, NetworkConnectedMessage>()
                .ForEach((Entity entity, ref NetworkConnection connection) =>
                {
                    var ep = networkStreamSystem.driver.RemoteEndPoint(connection.value);
                    Debug.Log($"{World.Name} => Connected  entity={entity.Index}  InternalId={connection.value.InternalId}  ip={ep.Port}");

                    //System.Net.IPEndPoint
                    //System.Net.IPAddress

                    if (connectedHandle != null)
                    {
                        connectedHandle();
                    }
                });


            //Disconnected
            Entities
                .WithAllReadOnly<NetworkConnection, NetworkDisconnectedMessage>()
                .ForEach((Entity entity, ref NetworkDisconnectedMessage disconnected, ref NetworkConnection connection) =>
                {
                    var ep = networkStreamSystem.driver.RemoteEndPoint(connection.value);

                    var errorStr = "";
                    getErrorInfo(disconnected.error, ref errorStr);
                    Debug.LogWarning($"{World.Name} => Disconnected  entity={entity.Index}  InternalId={connection.value.InternalId}  ip={ep.Port}  error={errorStr}");

                    if (disconnectedHandle != null)
                    {
                        disconnectedHandle(errorStr);
                    }
                });
        }
    }
}