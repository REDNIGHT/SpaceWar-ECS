using Unity.Entities;
using Unity.Mathematics;

namespace RN.Network.SpaceWar
{
    public class BulletEntity : System.Attribute { }

    [BulletEntity]
    public struct Bullet : IComponentData
    {
        //加贯穿功能吗?

        public float velocity;
    }
}
