using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RN.Network
{
    [ServerAutoClear]
    public struct PhysicsRaycastResults : IBufferElementData
    {
        public Entity entity;

        public float3 point;

        public float3 normal;

        public float distance;
    }


    /// <summary>
    /// 和PhysicsResults一一对应
    /// PhysicsOverlapSphereSystem里添加内容的
    /// </summary>
    [ServerAutoClear]
    public struct PhysicsOverlapHitPoints : IBufferElementData
    {
        public float3 value;
    }

    /*[ServerAutoClear]
    public struct PhysicsColliderResults : IBufferElementData
    {
        public Entity colliderEntity;
    }*/

    /// <summary>
    /// 不会出现重复或空的rb 上面PhysicsColliderResults会出现重复的Collider  因为一个rb是可以有多个Collider的 或者Collider没有rb
    /// </summary>
    [ServerAutoClear]
    public struct PhysicsResults : IBufferElementData
    {
        public Entity entity;
    }


    public struct OnPhysicsCallMessage : IComponentData
    {
    }
}
