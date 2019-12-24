using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace RN.Network.SpaceWar
{
    public class AccelerateFxEntity : System.Attribute { }

    [AccelerateFxEntity]
    public struct AccelerateFx : IComponentData
    {
        //public byte velocityLevel;
    }


    /*public struct AccelerateFxMessage : IComponentData
    {
        public Entity shipEntity;
        public Translation translation;
        public byte velocityLevel;
    }*/


    public interface IAccelerateFx
    {
        void OnPlayFx();
    }
}
