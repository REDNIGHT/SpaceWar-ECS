using Unity.Entities;
using Unity.Mathematics;

namespace RN.Network.SpaceWar
{
    public class AttributeModifyEntity : System.Attribute { }

    [AttributeModifyEntity]
    public struct AttributeModifyFx : IComponentData
    {
        public short actorType;//哪个武器打出来的伤害 播放对应特效
        public half hp;
        public half power;

        public Entity targetEntity;
    }
}
