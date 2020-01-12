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
    /// </summary>
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipSyncAttributeServerSystem : JobComponentSystem
    {
        /// <summary>
        /// 把附近角色的属性发送到客户端
        /// </summary>
        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SyncJobA : IJobForEach_BB<ActorDatas1Buffer, ObserverSyncVisibleActorBuffer>
        {
            [ReadOnly] public ComponentDataFromEntity<ControlForceDirection> controlForceDirectionFromEntity;
            [ReadOnly] public ComponentDataFromEntity<ControlTorqueAngular> controlTorqueAngularFromEntity;
            [ReadOnly] public ComponentDataFromEntity<ShipForceControl> shipForceControlFromEntity;

            public void Execute(DynamicBuffer<ActorDatas1Buffer> syncActorDatas1Buffer, [ReadOnly]DynamicBuffer<ObserverSyncVisibleActorBuffer> synVisibleActor)
            {
                for (int i = 0; i < synVisibleActor.Length; ++i)
                {
                    var actorEntity = synVisibleActor[i].actorEntity;

                    if (shipForceControlFromEntity.Exists(actorEntity))
                    {
                        var force = controlForceDirectionFromEntity[actorEntity].force;
                        var accelerateFire = shipForceControlFromEntity[actorEntity].accelerateFire;

                        var controlTorqueAngular = controlTorqueAngularFromEntity[actorEntity];
                        var torque = controlTorqueAngular.torque * controlTorqueAngular.angular.y;

                        syncActorDatas1Buffer.Add(new ActorDatas1Buffer
                        {
                            actorId = actorEntity.Index,
                            synDataType = (sbyte)(accelerateFire ? ActorSynDataTypes.Ship_AccelerateAttribute : ActorSynDataTypes.Ship_ForceAttribute),
                            halfValueA = (half)force,
                            halfValueB = (half)torque,
                        });
                    }
                }
            }
        }


        /// <summary>
        /// 自己ship的属性 发送到自己的client
        /// 自己ship的属性 不会发送到其他客户端里
        /// </summary>
        [BurstCompile]
        [RequireComponentTag(typeof(ObserverPosition))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SyncJobB : IJobForEach_BC<ActorDatas1Buffer, PlayerActorArray>
        {
            [ReadOnly] public ComponentDataFromEntity<ActorAttribute3<_HP>> hpFromEntity;
            [ReadOnly] public ComponentDataFromEntity<ActorAttribute3<_Power>> powerFromEntity;
            [ReadOnly] public ComponentDataFromEntity<ActorLastAttribute> actorLastAttributeFromEntity;

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
                                    synDataType = (sbyte)ActorSynDataTypes.Ship_Hp_Power,
                                    halfValueA = (half)hp.value,
                                    halfValueB = (half)power.value,
                                });
                            }
                        }
                    }
                }
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
            inputDeps = new SyncJobA
            {
                controlForceDirectionFromEntity = GetComponentDataFromEntity<ControlForceDirection>(true),
                controlTorqueAngularFromEntity = GetComponentDataFromEntity<ControlTorqueAngular>(true),
                shipForceControlFromEntity = GetComponentDataFromEntity<ShipForceControl>(true),
            }
            .Schedule(this, inputDeps);

            //
            inputDeps = new SyncJobB
            {
                hpFromEntity = GetComponentDataFromEntity<ActorAttribute3<_HP>>(true),
                powerFromEntity = GetComponentDataFromEntity<ActorAttribute3<_Power>>(true),
                actorLastAttributeFromEntity = GetComponentDataFromEntity<ActorLastAttribute>(true),
            }
            .Schedule(this, inputDeps);

            return inputDeps;
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipSyncAttributeClientSystem : JobComponentSystem
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
            public ComponentDataFromEntity<ShipForceAttribute> forceAttributeFromEntity;

            public void Execute([ReadOnly]DynamicBuffer<ActorDatas1Buffer> actorDatas1Buffer)
            {
                for (int i = 0; i < actorDatas1Buffer.Length; ++i)
                {
                    var actorDatas1 = actorDatas1Buffer[i];
                    if (actorDatas1.synDataType == (sbyte)ActorSynDataTypes.Ship_Hp_Power)
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


                    else if (actorDatas1.synDataType == (sbyte)ActorSynDataTypes.Ship_ForceAttribute)
                    {
                        if (actorEntityFromActorId.TryGetValue(actorDatas1.actorId, out Entity actorEntity))
                        {
                            forceAttributeFromEntity[actorEntity] = new ShipForceAttribute { accelerate = false, force = actorDatas1.halfValueA, torque = actorDatas1.halfValueB };
                        }
                    }
                    else if (actorDatas1.synDataType == (sbyte)ActorSynDataTypes.Ship_AccelerateAttribute)
                    {
                        if (actorEntityFromActorId.TryGetValue(actorDatas1.actorId, out Entity actorEntity))
                        {
                            forceAttributeFromEntity[actorEntity] = new ShipForceAttribute { accelerate = true, force = actorDatas1.halfValueA, torque = actorDatas1.halfValueB };
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
                forceAttributeFromEntity = GetComponentDataFromEntity<ShipForceAttribute>(),
            }
            .ScheduleSingle(this, inputDeps);

            return inputDeps;
        }
    }
}
