
using Unity.Entities;
using Unity.Mathematics;

namespace RN.Network.SpaceWar
{
    public class LaserEntity : System.Attribute { }

    [LaserEntity]
    public struct Laser : IComponentData
    {
        public float distance;
        public float startOffset;
    }

    [LaserEntity]
    public struct Laser_TR_Temp : IComponentData
    {
        public float3 position;
        public quaternion rotation;
    }
}
