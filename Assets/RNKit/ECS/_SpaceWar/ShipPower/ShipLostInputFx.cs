using Unity.Entities;

namespace RN.Network.SpaceWar
{
    public struct ShipLostInputFx : IComponentData
    {
        public float time;
    }

    public interface IShipLostInputFx
    {
        void OnPlayFx(float time);
    }
}
