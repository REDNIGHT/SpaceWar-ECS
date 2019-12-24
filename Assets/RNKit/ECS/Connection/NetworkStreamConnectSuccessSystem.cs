using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RN.Network
{
    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    public partial class NetworkStreamConnectSuccessSystem : ComponentSystem
    {
        List<ComponentSystemBase> systemsActiveByConnectSuccess = new List<ComponentSystemBase>();
        public void AddSystem(ComponentSystemBase sys)
        {
            sys.Enabled = false;
            systemsActiveByConnectSuccess.Add(sys);
        }

        protected override void OnUpdate()
        {
            //connected
            Entities
                .WithAllReadOnly<NetworkConnection, NetworkConnectedMessage>()
                .ForEach((Entity entity, ref NetworkConnection connection) =>
                {
                    foreach (var sys in systemsActiveByConnectSuccess)
                    {
                        sys.Enabled = true;
                    }

                    Enabled = false;
                    systemsActiveByConnectSuccess.Clear();
                    systemsActiveByConnectSuccess = null;
                });
        }
    }
}