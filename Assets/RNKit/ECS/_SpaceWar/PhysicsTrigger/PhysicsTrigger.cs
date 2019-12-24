using Unity.Entities;

namespace RN.Network.SpaceWar
{
    public class PhysicsTriggerEntity : System.Attribute { }


    [PhysicsTriggerEntity]
    public struct PhysicsTrigger : IComponentData
    {
        public float force;
    }

    [PhysicsTriggerEntity]
    public struct PhysicsTriggerFx : IComponentData
    {
        public TriggerResultState includeResultState;
    }
}
