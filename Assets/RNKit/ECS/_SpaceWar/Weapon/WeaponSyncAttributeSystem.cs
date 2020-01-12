using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    /// <summary>
    /// 只给自己的client同步属性值
    /// 自己ship的属性 不会发送到其他客户端里
    /// </summary>
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class WeaponSyncAttributeServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(ObserverPosition))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SyncJob : IJobForEach_BC<ActorDatas1Buffer, PlayerActorArray>
        {
            [ReadOnly] public ComponentDataFromEntity<WeaponAttribute> weaponAttributeFromEntity;

            public void Execute(DynamicBuffer<ActorDatas1Buffer> syncActorDatas1Buffer, [ReadOnly]ref PlayerActorArray actorArray)
            {
                for (int i = 0; i < actorArray.maxCount; ++i)
                {
                    var actorEntity = actorArray[i];

                    if (actorEntity != Entity.Null)
                    {
                        //
                        if (weaponAttributeFromEntity.Exists(actorEntity))
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
    public class WeaponSyncAttributeClientSystem : JobComponentSystem
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
            public ComponentDataFromEntity<WeaponAttribute> weaponAttributeFromEntity;

            public void Execute([ReadOnly]DynamicBuffer<ActorDatas1Buffer> actorDatas1Buffer)
            {
                for (int i = 0; i < actorDatas1Buffer.Length; ++i)
                {
                    var actorDatas1 = actorDatas1Buffer[i];
                    if (actorDatas1.synDataType == (sbyte)ActorSynDataTypes.WeaponAttribute)
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
                weaponAttributeFromEntity = GetComponentDataFromEntity<WeaponAttribute>(),
            }
            .ScheduleSingle(this, inputDeps);

            return inputDeps;
        }
    }
}
