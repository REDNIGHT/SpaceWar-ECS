using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network
{
    [ActorEntity]
    public struct LastPosition : IComponentData
    {
        public float3 curValue;
        public float3 lastValue;
    }

    /*public class LastPositionByChildTransformOnCreateSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<ChildTransform>().WithAllReadOnly<Transform, LastPosition, OnCreateMessage>()
                .ForEach((Transform transform, ref ChildTransform childTransform) =>
                {
                    var childT = transform.GetChild(childTransform.childIndex0);
                    if (childTransform.childIndex1 >= 0)
                        childT = childT.GetChild(childTransform.childIndex0);

                    childTransform.value = childT.position;
                });
        }
    }*/

    public class LastPositionByChildTransformSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(OnPhysicsCallMessage), typeof(OnCreateMessage))]
        struct SaveJobA : IJobForEach<LastPosition, ChildTransform_Out>
        {
            public void Execute(ref LastPosition lastPosition, ref ChildTransform_Out childTransform)
            {
                lastPosition.lastValue = childTransform.value;
                lastPosition.curValue = childTransform.value;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OnPhysicsCallMessage))]
        [ExcludeComponent(typeof(OnCreateMessage))]
        struct SaveJobB : IJobForEach<LastPosition, ChildTransform_Out>
        {
            public void Execute(ref LastPosition lastPosition, ref ChildTransform_Out childTransform)
            {
                lastPosition.lastValue = lastPosition.curValue;
                lastPosition.curValue = childTransform.value;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new SaveJobA { }.Schedule(this, inputDeps);
            inputDeps = new SaveJobB { }.Schedule(this, inputDeps);
            return inputDeps;
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class RaycastByLastPositionSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(OnPhysicsCallMessage))]
        [ExcludeComponent(typeof(OnCreateMessage))]
        struct LinecastJobA : IJobForEach<PhysicsLinecast, LastPosition>
        {
            public void Execute(ref PhysicsLinecast linecast, [ReadOnly]ref LastPosition lastPosition)
            {
                linecast.start = lastPosition.lastValue;
                linecast.end = lastPosition.curValue;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OnPhysicsCallMessage), typeof(LastPosition))]
        [ExcludeComponent(typeof(OnCreateMessage))]
        struct LinecastJobB : IJobForEach<PhysicsLinecast, RigidbodyForceAtPosition>
        {
            public void Execute([ReadOnly]ref PhysicsLinecast linecast, ref RigidbodyForceAtPosition rigidbodyForceAtPosition)
            {
                rigidbodyForceAtPosition.direction = math.normalize(linecast.end - linecast.start);
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OnPhysicsCallMessage))]
        [ExcludeComponent(typeof(OnCreateMessage))]
        struct SphereCastJobA : IJobForEach<PhysicsSphereCast, LastPosition>
        {
            public void Execute(ref PhysicsSphereCast sphereCast, [ReadOnly]ref LastPosition lastPosition)
            {
                var direction = lastPosition.curValue - lastPosition.lastValue;
                var distance = math.length(direction);
                //direction = math.normalize(direction);

                sphereCast.ray = new Ray { origin = lastPosition.lastValue, direction = direction };
                sphereCast.distance = distance;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OnPhysicsCallMessage), typeof(LastPosition))]
        [ExcludeComponent(typeof(OnCreateMessage))]
        struct SphereCastJobB : IJobForEach<PhysicsSphereCast, RigidbodyForceAtPosition>
        {
            public void Execute([ReadOnly]ref PhysicsSphereCast sphereCast, ref RigidbodyForceAtPosition rigidbodyForceAtPosition)
            {
                rigidbodyForceAtPosition.direction = sphereCast.ray.direction;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new LinecastJobA { }.Schedule(this, inputDeps);

            inputDeps = new LinecastJobB { }.Schedule(this, inputDeps);

            inputDeps = new SphereCastJobA { }.Schedule(this, inputDeps);

            inputDeps = new SphereCastJobB { }.Schedule(this, inputDeps);

            return inputDeps;
        }
    }

}
