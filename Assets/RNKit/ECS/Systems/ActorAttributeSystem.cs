using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RN.Network
{
    //基础属性
    //ActorAttribute3Base<T> 是消耗型基础属性
    //ActorAttribute1Base<T> 是基础属性

    //最终属性
    //ActorAttribute3<T> 是消耗型属性 例如hp mp...
    //ActorAttribute1<T> 是普通属性 例如力量 敏捷 攻击力...

    //属性修改 例如伤害 技能提供的属性修改
    //属性修改都是技能或buff里算出来的结果 而技能或buff都是单独的entity
    //下面struct都是IBufferElementData 每帧都会清空
    //ActorAttribute3Modify<T>
    //ActorAttribute3MaxModify<T>
    //ActorAttribute3RegainModify<T>
    //ActorAttribute1Modify<T>

    //
    //消耗型属性
    #region  ActorAttribute3
    public struct ActorAttribute3<T> : IComponentData
    {
        public float max;           //最大值
        public float regain;        //每秒恢复 PerSeconds
        public float value;         //当前值
        public float present => value / max;
    }
    public struct ActorAttribute3Base<T> : IComponentData
    {
        public float max;
        public float regain;
    }
    public enum Attribute3SubModifyType : byte
    {
        ValueOffset,
        //ValueCurrent,

        MaxOffset,
        RegainOffset,
    }

    //自身使用技能的消耗直接扣除对应的值就可以
    //如果是其他玩家的操作或buff 就必须用ActorAttributeModify这个来修改属性值
    [AutoClear]
    public struct ActorAttribute3Modifys<T> : IBufferElementData
    {
        public Entity player;       //哪个玩家释放的
        //public Entity onwerActor; //哪个角色释放的
        public short type;          //哪个技能释放的

        public float value;

        public Attribute3SubModifyType attribute3ModifyType;
    }

    public struct ActorAttribute3Offset<T> : IComponentData
    {
        public float minOffset;
        public float maxOffset;

        public float scale;

        public float GetValue(float percent)
        {
            var v = minOffset + (maxOffset - minOffset) * percent;
            v *= scale;
            return v;
        }

        /*public float GetOffset()
        {
            var v = Random. maxOffset;
            v *= scale;
            return v;
        }*/
    }
    #endregion

    //普通属性
    #region  ActorAttribute1
    public struct ActorAttribute1Base<T> : IComponentData
    {
        public float value;
    }

    [ServerAutoClear]
    public struct ActorAttribute1Modify<T> : IBufferElementData
    {
        public Entity player;  //哪个玩家释放的
        //public Entity onwerActor; //哪个角色释放的
        public short type;            //那个技能释放的

        public float value;
    }
    //这个是普通属性 没什么特别处理的
    public struct ActorAttribute1<T> : IComponentData
    {
        public float value;
    }
    #endregion



    //[DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorAttribute1ServerSystem<T> : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        protected override void OnDestroy()
        {
        }
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        [BurstCompile]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct Attribute1Job : IJobForEachWithEntity_EBCC<ActorAttribute1Modify<T>, ActorAttribute1Base<T>, ActorAttribute1<T>>
        {
            public void Execute(Entity actorEntity, int index, [ChangedFilter] DynamicBuffer<ActorAttribute1Modify<T>> actorAttribute1Modifys, [ReadOnly] ref ActorAttribute1Base<T> attribute1Base, ref ActorAttribute1<T> attribute1)
            {
                attribute1.value = attribute1Base.value;
                if (actorAttribute1Modifys.Length > 0)
                {
                    for (var i = 0; i < actorAttribute1Modifys.Length; ++i)
                    {
                        attribute1.value += actorAttribute1Modifys[i].value;
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new Attribute1Job { }.Schedule(this, inputDeps);
        }
    }


    //[DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorAttribute3ServerSystem<T> : JobComponentSystem
    {
        public bool zeroToActorDeath = false;

        public enum ModifyHandleType
        {
            Default,
            Sample,
            None,
        }

        public ModifyHandleType modifyHandleType;

        public bool regainEnable;


        EndCommandBufferSystem endBarrier;
        protected override void OnDestroy()
        {
        }
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        [BurstCompile]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        public struct Attribute3ModifysJob : IJobForEachWithEntity_EBCC<ActorAttribute3Modifys<T>, ActorAttribute3Base<T>, ActorAttribute3<T>>
        {
            public void Execute(Entity entity, int index, [ChangedFilter, ReadOnly] DynamicBuffer<ActorAttribute3Modifys<T>> attribute3Modifys, [ReadOnly]ref ActorAttribute3Base<T> attribute3Base, ref ActorAttribute3<T> attribute3)
            {
                //max
                attribute3.max = attribute3Base.max;
                for (var i = 0; i < attribute3Modifys.Length; ++i)
                {
                    if (attribute3Modifys[i].attribute3ModifyType == Attribute3SubModifyType.MaxOffset)
                    {
                        attribute3.max += attribute3Modifys[i].value;
                    }
                }

                //regain
                attribute3.regain = attribute3Base.regain;
                for (var i = 0; i < attribute3Modifys.Length; ++i)
                {
                    if (attribute3Modifys[i].attribute3ModifyType == Attribute3SubModifyType.RegainOffset)
                    {
                        attribute3.regain += attribute3Modifys[i].value;
                    }
                }
                if (attribute3.regain < 0f)
                    attribute3.regain = 0f;


                //value
                for (var i = 0; i < attribute3Modifys.Length; ++i)
                {
                    if (attribute3Modifys[i].attribute3ModifyType == Attribute3SubModifyType.ValueOffset)
                    {
                        attribute3.value += attribute3Modifys[i].value;
                    }
                }
                if (attribute3.value > attribute3.max)
                    attribute3.value = attribute3.max;
            }
        }

        [BurstCompile]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        public struct SampleAttribute3ModifysJob : IJobForEachWithEntity_EBC<ActorAttribute3Modifys<T>, ActorAttribute3<T>>
        {
            public void Execute(Entity entity, int index, [ChangedFilter, ReadOnly] DynamicBuffer<ActorAttribute3Modifys<T>> attribute3Modifys, ref ActorAttribute3<T> attribute3)
            {
                for (var i = 0; i < attribute3Modifys.Length; ++i)
                {
                    if (attribute3Modifys[i].attribute3ModifyType == Attribute3SubModifyType.ValueOffset)
                    {
                        attribute3.value += attribute3Modifys[i].value;
                    }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    else
                    {
                        throw new System.Exception($"attribute3Modifys[i].attribute3ModifyType={attribute3Modifys[i].attribute3ModifyType}");
                    }
#endif
                }
                if (attribute3.value > attribute3.max)
                    attribute3.value = attribute3.max;
            }
        }

        [BurstCompile]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        public struct Attribute3ModifysAndZeroToActorDeathJob : IJobForEachWithEntity_EBBCC<KillersOnActorDeath, ActorAttribute3Modifys<T>, ActorAttribute3Base<T>, ActorAttribute3<T>>
        {
            public ComponentType OnDestroyMessage;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity entity, int index, DynamicBuffer<KillersOnActorDeath> actorDeathByKillers, [ChangedFilter, ReadOnly] DynamicBuffer<ActorAttribute3Modifys<T>> attribute3Modifys, ref ActorAttribute3Base<T> attribute3Base, ref ActorAttribute3<T> attribute3)
            {
                //max
                attribute3.max = attribute3Base.max;
                for (var i = 0; i < attribute3Modifys.Length; ++i)
                {
                    if (attribute3Modifys[i].attribute3ModifyType == Attribute3SubModifyType.MaxOffset)
                    {
                        attribute3.max += attribute3Modifys[i].value;
                    }
                }

                //regain
                attribute3.regain = attribute3Base.regain;
                for (var i = 0; i < attribute3Modifys.Length; ++i)
                {
                    if (attribute3Modifys[i].attribute3ModifyType == Attribute3SubModifyType.RegainOffset)
                    {
                        attribute3.regain += attribute3Modifys[i].value;
                    }
                }
                if (attribute3.regain < 0f)
                    attribute3.regain = 0f;

                //value
                for (var i = 0; i < attribute3Modifys.Length; ++i)
                {
                    var attribute3Modify = attribute3Modifys[i];
                    if (attribute3Modify.attribute3ModifyType == Attribute3SubModifyType.ValueOffset)
                    {
                        attribute3.value += attribute3Modify.value;

                        //
                        if (attribute3Modify.value < 0f)
                        {
                            actorDeathByKillers.Add(new KillersOnActorDeath { playerEntity = attribute3Modify.player, type = attribute3Modify.type });
                        }
                    }
                }
                if (attribute3.value > attribute3.max)
                    attribute3.value = attribute3.max;

                //dead
                if (attribute3.value <= 0f)
                {
                    endCommandBuffer.AddComponent(index, entity, OnDestroyMessage);
                }
            }
        }

        [BurstCompile]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        public struct SampleAttribute3ModifysAndZeroToActorDeathJob : IJobForEachWithEntity_EBBC<KillersOnActorDeath, ActorAttribute3Modifys<T>, ActorAttribute3<T>>
        {
            public ComponentType OnDestroyMessage;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity entity, int index, DynamicBuffer<KillersOnActorDeath> actorDeathByKillers, [ChangedFilter, ReadOnly] DynamicBuffer<ActorAttribute3Modifys<T>> attribute3Modifys, ref ActorAttribute3<T> attribute3)
            {
                //value
                for (var i = 0; i < attribute3Modifys.Length; ++i)
                {
                    var attribute3Modify = attribute3Modifys[i];
                    if (attribute3Modify.attribute3ModifyType == Attribute3SubModifyType.ValueOffset)
                    {
                        attribute3.value += attribute3Modify.value;

                        //
                        if (attribute3Modify.value < 0f)
                        {
                            actorDeathByKillers.Add(new KillersOnActorDeath { playerEntity = attribute3Modify.player, type = attribute3Modify.type });
                        }
                    }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    else
                    {
                        throw new System.Exception($"attribute3Modifys[i].attribute3ModifyType={attribute3Modifys[i].attribute3ModifyType}");
                    }
#endif
                }
                if (attribute3.value > attribute3.max)
                    attribute3.value = attribute3.max;

                //dead
                if (attribute3.value <= 0f)
                {
                    endCommandBuffer.AddComponent(index, entity, OnDestroyMessage);
                }
            }
        }

        [BurstCompile]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct Attribute3RegainJob : IJobForEach<ActorAttribute3<T>>
        {
            public float fixedDeltaTime;

            public void Execute(ref ActorAttribute3<T> attribute3)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (attribute3.regain < 0f)
                    throw new System.Exception("attribute3.regain < 0f");
#endif
                //value
                attribute3.value += attribute3.regain * fixedDeltaTime;
                attribute3.value = math.clamp(attribute3.value, 0f, attribute3.max);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (zeroToActorDeath)
            {
                if (modifyHandleType == ModifyHandleType.Sample)
                {
                    inputDeps = new SampleAttribute3ModifysAndZeroToActorDeathJob
                    {
                        OnDestroyMessage = typeof(OnDestroyMessage),
                        endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
                    }
                    .Schedule(this, inputDeps);
                }
                else if (modifyHandleType == ModifyHandleType.Default)
                {
                    inputDeps = new Attribute3ModifysAndZeroToActorDeathJob
                    {
                        OnDestroyMessage = typeof(OnDestroyMessage),
                        endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
                    }
                    .Schedule(this, inputDeps);
                }

                endBarrier.AddJobHandleForProducer(inputDeps);
            }
            else
            {
                if (modifyHandleType == ModifyHandleType.Sample)
                {
                    inputDeps = new SampleAttribute3ModifysJob { }.Schedule(this, inputDeps);
                }
                else if (modifyHandleType == ModifyHandleType.Default)
                {
                    inputDeps = new Attribute3ModifysJob { }.Schedule(this, inputDeps);
                }
            }

            if (regainEnable)
            {
                inputDeps = new Attribute3RegainJob
                {
                    fixedDeltaTime = Time.fixedDeltaTime,
                }
                .Schedule(this, inputDeps);
            }

            return inputDeps;
        }
    }
}