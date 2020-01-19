using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network
{
    public struct RigidbodyForce : IComponentData
    {
        public float3 force;
        public ForceMode mode;
    }

    public struct RigidbodyTorque : IComponentData
    {
        public float3 torque;
        public ForceMode mode;
    }


    public struct RigidbodyForceAtPosition : IComponentData
    {
        public float force;
        public float3 direction;
        public ForceMode mode;
    }

    public struct RigidbodyExplosionForce : IComponentData
    {
        public float force;
        public float radius;
        public float upwardsModifier => 0f;//暂时没用
        public ForceMode mode;
    }

    public struct RigidbodyForceByCenter : IComponentData
    {
        public float force;
        public float tangentForce;
        public float torque;
        public float radius;
        public ForceMode mode;

        public static readonly float3 _up = new float3(0f, 1f, 0f);
        public float3 up => _up;
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class RigidbodyForceSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<Rigidbody, RigidbodyForce>()//.WithAllReadOnly<OnPhysicsCallMessage>()
                .ForEach((Rigidbody rigidbody, ref RigidbodyForce rigidbodyForce) =>
                {
                    if (rigidbody.isKinematic)
                        return;

                    rigidbody.AddForce(rigidbodyForce.force, rigidbodyForce.mode);
                    rigidbodyForce = default;
                });

            Entities
                .WithAll<Rigidbody, RigidbodyTorque>()//.WithAllReadOnly<OnPhysicsCallMessage>()
                .ForEach((Rigidbody rigidbody, ref RigidbodyTorque rigidbodyTorque) =>
                {
                    if (rigidbody.isKinematic)
                        return;

                    rigidbody.AddTorque(rigidbodyTorque.torque, rigidbodyTorque.mode);
                    rigidbodyTorque = default;
                });

            Entities
                .WithAllReadOnly<PhysicsRaycastResults, RigidbodyForceAtPosition, OnPhysicsCallMessage>()
                .ForEach((DynamicBuffer<PhysicsRaycastResults> raycastResults, ref RigidbodyForceAtPosition addForceAtPosition) =>
                {
                    for (var i = 0; i < raycastResults.Length; ++i)
                    {
                        var raycastResult = raycastResults[i];
                        if (raycastResult.entity == default)
                            continue;

                        var rigidbody = EntityManager.GetComponentObject<Rigidbody>(raycastResult.entity);

                        if (rigidbody.isKinematic)
                            continue;

                        //var normal = addForceAtPosition.direction.Equals(float3.zero) ? -raycastResult.value.normal : addForceAtPosition.direction;
                        //var normal = addForceAtPosition.direction;
                        //var normal = math.normalize(-raycastResult.normal + addForceAtPosition.direction);

                        Debug.Assert(math.abs(math.length(addForceAtPosition.direction) - 1f) < 0.001f, $"math.length(addForceAtPosition.direction)={math.length(addForceAtPosition.direction)}");
                        rigidbody.AddForceAtPosition
                        (
                            addForceAtPosition.force * addForceAtPosition.direction,
                            raycastResult.point,
                            addForceAtPosition.mode
                        );
                    }
                });

            Entities
                .WithAllReadOnly<PhysicsResults, RigidbodyForce, OnPhysicsCallMessage>()
                .ForEach((DynamicBuffer<PhysicsResults> rigidbodyResults, ref RigidbodyForce rigidbodyForce) =>
                {
                    for (var i = 0; i < rigidbodyResults.Length; ++i)
                    {
                        var rigidbodyResult = rigidbodyResults[i];

                        if (EntityManager.Exists(rigidbodyResult.entity) == false)
                            continue;

                        var rigidbody = EntityManager.GetComponentObject<Rigidbody>(rigidbodyResult.entity);

                        if (rigidbody.isKinematic)
                            continue;

                        rigidbody.AddForce(rigidbodyForce.force, rigidbodyForce.mode);
                        //rigidbody.AddForceAtPosition(rigidbodyForce.force, translation.Value, rigidbodyForce.mode);
                    }
                });

            Entities
                .WithAllReadOnly<PhysicsResults, Translation, RigidbodyExplosionForce, OnPhysicsCallMessage>()
                .ForEach((DynamicBuffer<PhysicsResults> rigidbodyResults, ref Translation translation, ref RigidbodyExplosionForce rigidbodyExplosionForce) =>
                {
                    for (var i = 0; i < rigidbodyResults.Length; ++i)
                    {
                        var rigidbodyResult = rigidbodyResults[i];

                        if (EntityManager.Exists(rigidbodyResult.entity) == false)
                            continue;

                        var rigidbody = EntityManager.GetComponentObject<Rigidbody>(rigidbodyResult.entity);

                        if (rigidbody.isKinematic)
                            continue;

#if false
                        var direction2Center = (float3)rigidbody.position - translation.Value;

                        var distance2Center = math.length(direction2Center);
                        if (distance2Center >= rigidbodyExplosionForce.radius)
                            return;

                        if (rigidbodyExplosionForce.radius > 0f)
                        {
                            distance2Center = 1f - distance2Center / rigidbodyExplosionForce.radius;
                        }


                        direction2Center = math.normalize(direction2Center);

                        var force = rigidbodyExplosionForce.force * direction2Center * distance2Center;

                        //rigidbody.AddForce(force, rigidbodyExplosionForce.mode);
                        rigidbody.AddForceAtPosition(force, translation.Value, rigidbodyExplosionForce.mode);
#else
                        //这爆炸效果带旋转的 还旋转得不稳定
                        rigidbody.AddExplosionForce
                        (
                            rigidbodyExplosionForce.force,
                            translation.Value,
                            rigidbodyExplosionForce.radius,
                            rigidbodyExplosionForce.upwardsModifier,
                            rigidbodyExplosionForce.mode
                        );
#endif
                    }
                });

            Entities
                .WithAllReadOnly<PhysicsResults, Translation, RigidbodyForceByCenter, OnPhysicsCallMessage>()
                .ForEach((DynamicBuffer<PhysicsResults> rigidbodyResults, ref Translation translation, ref RigidbodyForceByCenter rigidbodyForceByCenter) =>
                {
                    for (var i = 0; i < rigidbodyResults.Length; ++i)
                    {
                        var rigidbodyResult = rigidbodyResults[i];

                        if (EntityManager.Exists(rigidbodyResult.entity) == false)
                            continue;

                        var rigidbody = EntityManager.GetComponentObject<Rigidbody>(rigidbodyResult.entity);

                        if (rigidbody.isKinematic)
                            continue;

                        var direction2Center = (float3)rigidbody.position - translation.Value;
                        var distance2Center = math.length(direction2Center);
                        if (rigidbodyForceByCenter.radius > 0f)
                            distance2Center = 1f - distance2Center / rigidbodyForceByCenter.radius;
                        direction2Center = math.normalize(direction2Center);

                        var force = rigidbodyForceByCenter.force * direction2Center * distance2Center;
                        var tangent = rigidbodyForceByCenter.tangentForce * math.cross(direction2Center, rigidbodyForceByCenter.up) * distance2Center;

                        rigidbody.AddForce(force + tangent, rigidbodyForceByCenter.mode);
                        rigidbody.AddTorque(rigidbodyForceByCenter.torque * rigidbodyForceByCenter.up * distance2Center, rigidbodyForceByCenter.mode);
                    }
                });
        }
    }
}

