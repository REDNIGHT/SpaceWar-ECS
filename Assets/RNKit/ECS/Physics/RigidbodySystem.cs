using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RN.Network
{
    public struct RigidbodyVelocity : IComponentData
    {
        public float3 linear;
        public float3 angular;
    }

    /*public struct RigidbodyDrag : IComponentData
    {
        public float linearDrag;
        public float angularDrag;
    }*/

    public struct Rigidbody_Out : IComponentData
    {
    }
    public struct Rigidbody_In : IComponentData
    {
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class RigidbodyOutSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<RigidbodyVelocity>().WithAllReadOnly<Rigidbody_Out, Rigidbody>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Rigidbody rigidbody, ref RigidbodyVelocity rigidbodyVelocity) =>
                {
                    rigidbodyVelocity.linear = rigidbody.velocity;
                    rigidbodyVelocity.angular = rigidbody.angularVelocity;
                });

            /*Entities
                .WithAll<RigidbodyDrag>().WithAllReadOnly<Rigidbody_Out, Rigidbody>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Rigidbody rigidbody, ref RigidbodyDrag rigidbodyDrag) =>
                {
                    rigidbodyDrag.linearDrag = rigidbody.drag;
                    rigidbodyDrag.angularDrag = rigidbody.angularDrag;
                });*/
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class RigidbodyInSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<Rigidbody>().WithAllReadOnly<Rigidbody_In, RigidbodyVelocity>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Rigidbody rigidbody, ref RigidbodyVelocity rigidbodyVelocity) =>
                {
                    rigidbody.velocity = rigidbodyVelocity.linear;
                    rigidbody.angularVelocity = rigidbodyVelocity.angular;
                });

            /*Entities
                .WithAll<Rigidbody>().WithAllReadOnly<Rigidbody_In, RigidbodyDrag>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Rigidbody rigidbody, ref RigidbodyDrag rigidbodyDrag) =>
                {
                    rigidbody.drag = rigidbodyDrag.linearDrag;
                    rigidbody.angularDrag = rigidbodyDrag.angularDrag;
                });*/
        }
    }
}