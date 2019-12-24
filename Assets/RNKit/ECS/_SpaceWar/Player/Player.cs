using Unity.Entities;

namespace RN.Network.SpaceWar
{
    [ServerNetworkEntity]
    public struct Player_OnShipDestroyMessage : IComponentData
    {
    }
}
