using Unity.Entities;
using Unity.Mathematics;

namespace RN.Network.SpaceWar
{
    public class ExplosionEntity : System.Attribute { }

    //通过MessageTrigger{type = typeof(OnPhysicsCallMessage)}和ActorLifetime可以做出持续爆炸效果
    [ExplosionEntity]
    public struct Explosion : IComponentData
    {
        public float radius;
    }

}
