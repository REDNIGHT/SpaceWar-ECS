using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    public struct OnCallMessage : IComponentData
    {
    }

    public struct CallTrigger : IComponentData
    {
        //public readonly float delay;
        public readonly float interval;
        public readonly int maxCount;
        public ComponentType type;


        public float time;
        public uint count;


        public CallTrigger(float delay, float interval, int maxCount, ComponentType type)
        {
            //delay = _delay;
            this.interval = interval;
            this.maxCount = maxCount;
            this.type = type;

            time = delay;
            count = 0;
        }

        public static CallTrigger CallOnce(float delay, ComponentType type)
        {
            return new CallTrigger(delay, 0f, 1, type);
        }

        public static CallTrigger AddCallMessage(float delay, ComponentType type)
        {
            return new CallTrigger(delay, -1, -1, type);
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    class CallTriggerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        [BurstCompile]
        struct MessageTriggerJob : IJobForEachWithEntity<CallTrigger>
        {
            public float fixedDeltaTime;
            public ComponentType CallTrigger;
            public EntityCommandBuffer.Concurrent commandBuffer;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity entity, int index, ref CallTrigger messageTrigger)
            {
                messageTrigger.time -= fixedDeltaTime;
                if (messageTrigger.time <= 0f && (messageTrigger.maxCount == -1 || messageTrigger.count < messageTrigger.maxCount))
                {
                    if (messageTrigger.interval >= 0f)
                    {
                        messageTrigger.time += messageTrigger.interval;
                        ++messageTrigger.count;

                        //
                        commandBuffer.AddComponent(index, entity, messageTrigger.type);
                        endCommandBuffer.RemoveComponent(index, entity, messageTrigger.type);
                    }
                    else if (messageTrigger.maxCount == -1)
                    {
                        commandBuffer.AddComponent(index, entity, messageTrigger.type);
                        endCommandBuffer.RemoveComponent(index, entity, CallTrigger);
                    }
                }
                else
                {
                    if (messageTrigger.maxCount != -1 && messageTrigger.count >= messageTrigger.maxCount)
                    {
                        endCommandBuffer.RemoveComponent(index, entity, CallTrigger);
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            using (var commandBuffer = new EntityCommandBuffer(Allocator.TempJob))
            {
                inputDeps = new MessageTriggerJob
                {
                    fixedDeltaTime = Time.fixedDeltaTime,
                    CallTrigger = typeof(CallTrigger),
                    commandBuffer = commandBuffer.ToConcurrent(),
                    endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
                }
                .Schedule(this, inputDeps);

                inputDeps.Complete();
                commandBuffer.Playback(EntityManager);
            }

            //endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}
