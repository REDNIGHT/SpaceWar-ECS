using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public struct ActorLastAttribute : IComponentData
    {
        public float hp;
        public float power;
    }

    /// <summary>
    /// 只给自己的client同步属性值
    /// 自己ship的属性 不会发送到其他客户端里
    /// </summary>
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorSyncAttributeServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(ObserverPosition))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SyncJob : IJobForEach_BC<ActorDatas1Buffer, PlayerActorArray>
        {
            [ReadOnly] public ComponentDataFromEntity<ActorAttribute3<_HP>> hpFromEntity;
            [ReadOnly] public ComponentDataFromEntity<ActorAttribute3<_Power>> powerFromEntity;
            [ReadOnly] public ComponentDataFromEntity<ActorLastAttribute> actorLastAttributeFromEntity;
            [ReadOnly] public ComponentDataFromEntity<WeaponAttribute> weaponAttributeFromEntity;

            public void Execute(DynamicBuffer<ActorDatas1Buffer> syncActorDatas1Buffer, [ReadOnly]ref PlayerActorArray actorArray)
            {
                for (int i = 0; i < actorArray.maxCount; ++i)
                {
                    var actorEntity = actorArray[i];


                    if (actorEntity != Entity.Null)
                    {
                        //
                        if (hpFromEntity.Exists(actorEntity))
                        {
                            var hp = hpFromEntity[actorEntity];
                            var power = powerFromEntity[actorEntity];
                            var lastAttribute = actorLastAttributeFromEntity[actorEntity];

                            if (hp.value != lastAttribute.hp || power.value != lastAttribute.power)
                            {
                                lastAttribute.hp = hp.value;
                                lastAttribute.power = power.value;

                                syncActorDatas1Buffer.Add(new ActorDatas1Buffer
                                {
                                    actorId = actorEntity.Index,
                                    synDataType = (sbyte)ActorSynDataTypes.Hp_Power,
                                    halfValueA = (half)hp.value,
                                    halfValueB = (half)power.value,
                                });
                            }
                        }


                        //
                        else if (weaponAttributeFromEntity.Exists(actorEntity))
                        {
                            var weaponAttribute = weaponAttributeFromEntity[actorEntity];

                            if (weaponAttribute.hp != weaponAttribute.lastHp || weaponAttribute.itemCount != weaponAttribute.lastItemCount)
                            {
                                syncActorDatas1Buffer.Add(new ActorDatas1Buffer
                                {
                                    actorId = actorEntity.Index,
                                    synDataType = (sbyte)ActorSynDataTypes.WeaponAttribute,
                                    shortValueA = weaponAttribute.hp,
                                    shortValueB = weaponAttribute.itemCount,
                                });
                            }
                        }
                    }
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(WeaponInstalledState))]
        struct WeaponAttributeSaveJob : IJobForEach<WeaponAttribute>
        {
            public void Execute(ref WeaponAttribute weaponAttribute)
            {
                weaponAttribute.lastHp = weaponAttribute.hp;
                weaponAttribute.lastItemCount = weaponAttribute.itemCount;
            }
        }

        public int perFrame = 5;
        int curFrame = 0;
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ++curFrame;
            if (curFrame < perFrame)
                return inputDeps;
            curFrame = 0;

            //
            inputDeps = new SyncJob
            {
                hpFromEntity = GetComponentDataFromEntity<ActorAttribute3<_HP>>(true),
                powerFromEntity = GetComponentDataFromEntity<ActorAttribute3<_Power>>(true),
                actorLastAttributeFromEntity = GetComponentDataFromEntity<ActorLastAttribute>(true),

                weaponAttributeFromEntity = GetComponentDataFromEntity<WeaponAttribute>(true),
            }
            .Schedule(this, inputDeps);

            inputDeps = new WeaponAttributeSaveJob
            {
            }
            .Schedule(this, inputDeps);

            return inputDeps;
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorSyncAttributeClientSystem : JobComponentSystem
    {
        ActorSyncCreateClientSystem actorClientSystem;

        protected override void OnCreate()
        {
            actorClientSystem = World.GetExistingSystem<ActorSyncCreateClientSystem>();
        }

        [BurstCompile]
        //[RequireComponentTag(typeof(NetworkConnection))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SyncActorDatas1ListJob : IJobForEach_B<ActorDatas1Buffer>
        {
            [ReadOnly] public NativeHashMap<int, Entity> actorEntityFromActorId;
            public ComponentDataFromEntity<ActorAttribute3<_HP>> hpFromEntity;
            public ComponentDataFromEntity<ActorAttribute3<_Power>> powerFromEntity;
            public ComponentDataFromEntity<WeaponAttribute> weaponAttributeFromEntity;

            public void Execute([ReadOnly]DynamicBuffer<ActorDatas1Buffer> actorDatas1Buffer)
            {
                for (int i = 0; i < actorDatas1Buffer.Length; ++i)
                {
                    var actorDatas1 = actorDatas1Buffer[i];
                    if (actorDatas1.synDataType == (sbyte)ActorSynDataTypes.Hp_Power)
                    {
                        if (actorEntityFromActorId.TryGetValue(actorDatas1.actorId, out Entity actorEntity))
                        {
                            var hp = hpFromEntity[actorEntity];
                            hp.value = actorDatas1.halfValueA;
                            hpFromEntity[actorEntity] = hp;

                            if (powerFromEntity.Exists(actorEntity))
                            {
                                var power = powerFromEntity[actorEntity];
                                power.value = actorDatas1.halfValueB;
                                powerFromEntity[actorEntity] = power;
                            }
                        }
                    }
                    else if (actorDatas1.synDataType == (sbyte)ActorSynDataTypes.WeaponAttribute)
                    {
                        if (actorEntityFromActorId.TryGetValue(actorDatas1.actorId, out Entity actorEntity))
                        {
                            weaponAttributeFromEntity[actorEntity] = new WeaponAttribute { hp = actorDatas1.shortValueA, itemCount = actorDatas1.shortValueB };
                        }
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new SyncActorDatas1ListJob
            {
                actorEntityFromActorId = actorClientSystem.actorEntityFromActorId,
                hpFromEntity = GetComponentDataFromEntity<ActorAttribute3<_HP>>(),
                powerFromEntity = GetComponentDataFromEntity<ActorAttribute3<_Power>>(),
                weaponAttributeFromEntity = GetComponentDataFromEntity<WeaponAttribute>(),
            }
            .ScheduleSingle(this, inputDeps);

            return inputDeps;
        }
    }
}
