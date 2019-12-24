using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    /// <summary>
    /// 有ActorLifetime的都会自动删除包括服务器客户端
    /// 除非提前删除才会给客户端发送删除信息    //删除代码在PlayerObserveCreateServerSystem.GetDestroyActorJobA
    /// 
    /// 如果lifetime是0 这entity就只能存在一帧
    /// </summary>
    public struct ActorLifetime : IComponentData
    {
        public float lifetime;
        public float value;

        public float percent => value / lifetime;

        //不能在创建entity后过一段时间才添加ActorLifetime
        //如果需要这功能 就取消下面代码的注释 还有GetDestroyActorJobA的注释
        /*
        /// <summary>
        /// 服务器的entity创建后一段时间才添加ActorLifetime 需要网络同步 因为客户端并不会自动添加ActorLifetime
        /// </summary>
        public bool needSyncOnDestroy;
        */
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorLifetimeSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;

        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }


        [BurstCompile]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct LifetimeJob : IJobForEachWithEntity_EC<ActorLifetime>
        {
            public float fixedDeltaTime;
            public ComponentType OnDestroyMessage;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity entity, int index, ref ActorLifetime lifetime)
            {
                lifetime.value -= fixedDeltaTime;
                if (lifetime.value <= 0f)
                {
                    lifetime.value = 0f;
                    endCommandBuffer.AddComponent(index, entity, OnDestroyMessage);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new LifetimeJob
            {
                fixedDeltaTime = Time.fixedDeltaTime,
                OnDestroyMessage = typeof(OnDestroyMessage),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}