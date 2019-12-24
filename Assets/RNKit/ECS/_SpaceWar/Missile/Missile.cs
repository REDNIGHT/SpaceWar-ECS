using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class MissileEntity : System.Attribute { }

    [MissileEntity]
    public struct Missile : IComponentData
    {
    }

    [MissileEntity]
    public struct MissilePhysics : IComponentData
    {
        public bool accelerateByFindTarget;
        public float forceByTarget;
        public float maxVelocityByTarget;
    }

    [MissileEntity]
    public struct MissileAutoExplosionByAngle : IComponentData
    {
    }

    [MissileEntity]
    public struct MissileAutoExplosionByTouche : IComponentData
    {
        public float beginForce;
        public float beginTorque;
    }
}
