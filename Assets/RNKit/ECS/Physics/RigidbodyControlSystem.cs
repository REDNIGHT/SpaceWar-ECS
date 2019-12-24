using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network
{
    public struct ControlForceDirection : IComponentData
    {
        public bool zeroEnable;
        public float force;
        public float maxVelocity;

        public float3 direction;
    }

    /// <summary>
    /// 必须和下面的ControlTorqueAngular一起用
    /// </summary>
    public struct ControlTorqueDirection : IComponentData
    {
        public float3 direction;
    }

    public struct ControlTorqueAngular : IComponentData
    {
        public bool zeroEnable;
        public float torque;
        public float maxTorque;

        public float3 angular;
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class RigidbodyControlSystem : JobComponentSystem
    {
        [BurstCompile]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct TorqueJobA : IJobForEach<ControlTorqueAngular, ControlTorqueDirection, Rotation>
        {
            public void Execute(ref ControlTorqueAngular controlTorqueAngular, [ReadOnly] ref ControlTorqueDirection controlTorqueDirection, [ReadOnly] ref Rotation rotation)
            {
                if (controlTorqueAngular.torque == 0f) return;

                controlTorqueAngular.angular = math.cross(math.forward(rotation.Value), controlTorqueDirection.direction);
            }
        }

        [BurstCompile]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct TorqueJobB : IJobForEach<RigidbodyTorque, RigidbodyVelocity, ControlTorqueAngular>
        {
            public void Execute(ref RigidbodyTorque rigidbodyTorque, [ReadOnly] ref RigidbodyVelocity rigidbodyVelocity, [ReadOnly] ref ControlTorqueAngular controlTorqueAngular)
            {
                if (controlTorqueAngular.torque == 0f) return;

                if (controlTorqueAngular.angular.Equals(float3.zero) && controlTorqueAngular.zeroEnable == false) return;

                rigidbodyTorque.torque += RigidbodyEx.AddForce(controlTorqueAngular.torque, controlTorqueAngular.maxTorque, controlTorqueAngular.angular, rigidbodyVelocity.angular);
            }
        }

        [BurstCompile]
        //[RequireComponentTag(typeof())]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct ForceJob : IJobForEach<RigidbodyForce, RigidbodyVelocity, ControlForceDirection>
        {
            public void Execute(ref RigidbodyForce rigidbodyForce, [ReadOnly] ref RigidbodyVelocity rigidbodyVelocity, [ReadOnly] ref ControlForceDirection controlForceDirection)
            {
                if (controlForceDirection.force == 0f) return;

                if (controlForceDirection.direction.Equals(float3.zero) && controlForceDirection.zeroEnable == false) return;

                var linear = rigidbodyVelocity.linear;
                RigidbodyEx.ClampLinearVelocity(controlForceDirection.maxVelocity, ref linear);

                rigidbodyForce.force += RigidbodyEx.AddForce(controlForceDirection.force, controlForceDirection.maxVelocity, controlForceDirection.direction, linear);
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new TorqueJobA { }.Schedule(this, inputDeps);
            inputDeps = new TorqueJobB { }.Schedule(this, inputDeps);
            inputDeps = new ForceJob { }.Schedule(this, inputDeps);

            return inputDeps;
        }
    }
}
