using Unity.Entities;
using Unity.Mathematics;

namespace RN.Network
{
    public struct FindTraceTarget : IComponentData
    {
        public Entity targetEntity;
        public float targetVelocityScale;
        public bool lostTargetEnable;
    }

    public struct TracePoint : IComponentData
    {
        public float3 value;
    }

    public struct TraceDirection : IComponentData
    {
        public float3 value;
    }

    public struct TraceDirectionData : IComponentData
    {
        public bool enable;

        public float targetAngleOffset;
        public float lastTargetAngleOffset;

        public bool cancelOnGotoTargetPoint;
        public float cancelAngle;
    }

}
