
using Unity.Entities;
using Unity.Mathematics;

namespace RN.Network.SpaceWar
{
    public class SceneObjectEntity : System.Attribute { }

    [SceneObjectEntity]
    public struct SceneObjectAutoReset : IComponentData
    {
        public float3 defaultPosition;
        public quaternion defaultRotation;
    }
}
