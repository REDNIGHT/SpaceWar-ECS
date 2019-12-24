using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class BatteryControlServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(Battery))]
        struct FollowJob : IJobForEach<ControlForceDirection, ControlTorqueDirection, Translation, ParentTransform_Out, ParentRotation_Out>
        {
            public void Execute(ref ControlForceDirection controlForceDirection, ref ControlTorqueDirection followTorqueDirection
                , [ReadOnly]ref Translation translation, [ReadOnly]ref ParentTransform_Out parentTransform, [ReadOnly]ref ParentRotation_Out parentRotation)
            {
                controlForceDirection.direction = parentTransform.value - translation.Value;
                followTorqueDirection.direction = math.forward(parentRotation.value);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new FollowJob { }.Schedule(this, inputDeps);
        }
    }
}
