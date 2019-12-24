
using Unity.Entities;
using Unity.Mathematics;

namespace RN.Network.SpaceWar
{
    public class AttributeTriggerEntity : System.Attribute { }

    //
    [AttributeTriggerEntity]
    public struct AttributeTrigger : IComponentData
    {
    }

}
