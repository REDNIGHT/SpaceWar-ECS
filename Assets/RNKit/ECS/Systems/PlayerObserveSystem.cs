using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network
{
    //
    //通过input设置下面的position
    [ServerNetworkEntity]
    public struct ObserverPosition : IComponentData
    {
        public float3 value;
    }
    [ServerNetworkEntity]
    public struct ObserverVisibleDistance : IComponentData
    {
        public float valueSq;
        public bool all => valueSq < 0f;
    }


    //
    public enum SyncActorType : byte
    {
        /// <summary>
        /// 禁止同步
        /// </summary>
        Disable,

        /// <summary>
        /// 只同步坐标
        /// 不是刚体
        /// </summary>
        Translation,

        /// <summary>
        /// 只同步旋转
        /// 不是刚体
        /// </summary>
        Rotation,

        /// <summary>
        /// 同步坐标和旋转
        /// 不是刚体
        /// </summary>
        Translation_Rotation,

        /// <summary>
        /// 直线运动的刚体  rotation, drag或linear Velocity修改时会同步到客户端
        /// 必须是刚体
        /// 在没有任何变化时 不会有同步数据发送
        /// </summary>
        RB_VelocityDirection,

        /// <summary>
        /// 不规则运动的刚体 同步坐标,线速度
        /// 必须是刚体
        /// </summary>
        RB_Translation_Velocity,

        /// <summary>
        /// 不规则旋转运动的刚体 同步旋转和角速度
        /// 必须是刚体
        /// </summary>
        RB_Rotation_Velocity,

        /// <summary>
        /// 不规则运动的刚体 同步坐标,旋转,线速度和角速度
        /// 必须是刚体
        /// </summary>
        RB_Translation_Rotation_Velocity,
    }

    public enum PerFrame : byte
    {
        _1 = 1,
        _2,
        _3,
        _4,
        _5,
        Max,
    }

    [ActorEntity]
    public struct ActorVisibleDistanceOnCD : IComponentData
    {
        public bool unreliableOnCreate;                 //发送创建数据时 选择不稳定发送 这种方式创建的角色只能是一次性的 不能有后续的数据发送到这角色上 
        //public float value;                           //初始化时的可见距离     -1是无限距离   主角必须是-1  EnterGameMessage时只会发生-1的actor到客户端
        public bool isUnlimited;
    }
    [ActorEntity]
    public struct ActorVisibleDistanceOnSync : IComponentData
    {
        public SyncActorType syncType;
        public PerFrame perFrame;//1 <= perFrame < 5

        public bool changed;
        //public bool isUnlimited => false;

        public Translation lastTranslation;
        public Rotation lastRotation;

        public RigidbodyVelocity lastRigidbodyVelocity;
        public float linearDistancesq2Changed => 0.0001f;
        public float angularDistancesq2Changed => 0.01f;
        public float distancesq2Changed => 0.5f;
        public float angle2Changed => 30f;

        public float lastVelocityOffset;
        public float lastDistanceOffset;
        public float distanceOffset2Changed => 5f;
    }

    [System.Obsolete("...")]//不限制可见距离同步数据//还不如直接通过网络同步数据
    [ActorEntity]
    public struct ActorUnlimitedDistanceOnSync : IComponentData
    {
        public PerFrame perFrame;//1 <= perFrame < 5
    }

    //
    //可见集合
    [ServerNetworkEntity]
    [ServerAutoClear]
    public struct ObserverCreateVisibleActorBuffer : IBufferElementData
    {
        public int ownerPlayerId;
        public Entity actorEntity;
        public Actor actor;
        public bool unreliableOnCreate;
    }

    [ServerNetworkEntity]
    [ServerAutoClear]
    public struct ObserverSyncVisibleActorBuffer : IBufferElementData
    {
        public SyncActorType synActorType;
        public Entity actorEntity;
    }

    [ServerNetworkEntity]
    [ServerAutoClear]
    public struct ObserverDestroyVisibleActorBuffer : IBufferElementData
    {
        public Entity actorEntity;
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerObserveCreateServerSystem : ComponentSystem
    {
        public float visibleDistance = 64f;
        float3 defaultPosition;
        protected void OnInit(Transform root)
        {
            var observerDefaultPosition = root.transform.Find("observerDefaultPosition");
            if (observerDefaultPosition == null)
            {
                Debug.LogError("observerDefaultPosition == null");
                return;
            }

            defaultPosition = observerDefaultPosition.localPosition;
        }
        protected override void OnDestroy()
        {
        }
        protected override void OnCreate()
        {
            enterGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerEnterGameMessage>()/*, ComponentType.ReadOnly<Player>()*/ },
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });
        }
        EntityQuery enterGameQuery;

        protected override void OnUpdate()
        {
            if (enterGameQuery.IsEmptyIgnoreFilter == false)
            {
                using (var enterGamePlayerEntitys = enterGameQuery.ToEntityArray(Allocator.TempJob))
                {
                    for (var i = 0; i < enterGamePlayerEntitys.Length; ++i)
                    {
                        var enterGamePlayerEntity = enterGamePlayerEntitys[i];
                        EntityManager.AddComponentData(enterGamePlayerEntity, new ObserverPosition { value = defaultPosition });
                        EntityManager.AddComponentData(enterGamePlayerEntity, new ObserverVisibleDistance { valueSq = visibleDistance * visibleDistance });
                        EntityManager.AddComponent<ObserverCreateVisibleActorBuffer>(enterGamePlayerEntity);
                        EntityManager.AddComponent<ObserverSyncVisibleActorBuffer>(enterGamePlayerEntity);
                        EntityManager.AddComponent<ObserverDestroyVisibleActorBuffer>(enterGamePlayerEntity);
                    }
                }
            }
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerObserveServerSystem : JobComponentSystem
    {
        [BurstCompile]
        //[RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct CreateVisibleDistanceJob : IJobForEach_BCC<ObserverCreateVisibleActorBuffer, ObserverPosition, ObserverVisibleDistance>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> actorEntitys;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Actor> actors;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<ActorOwner> actorOwnerPlayers;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> translations;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<ActorVisibleDistanceOnCD> actorVisibleDistanceOnCDs;
            public void Execute(DynamicBuffer<ObserverCreateVisibleActorBuffer> createVisibleActorList, [ReadOnly] ref ObserverPosition observerPosition, [ReadOnly] ref ObserverVisibleDistance observerVisibleDistance)
            {
                for (int i = 0; i < actorEntitys.Length; ++i)
                {
                    var actorEntity = actorEntitys[i];
                    var actor = actors[i];
                    var actorOwner = actorOwnerPlayers[i];
                    var translation = translations[i];
                    var actorVisibleDistanceOnCD = actorVisibleDistanceOnCDs[i];

                    if (actorVisibleDistanceOnCD.isUnlimited || observerVisibleDistance.all || math.distancesq(observerPosition.value, translation.Value) < observerVisibleDistance.valueSq)
                    {
                        createVisibleActorList.Add(new ObserverCreateVisibleActorBuffer
                        {
                            ownerPlayerId = actorOwner.playerId,
                            actorEntity = actorEntity,
                            actor = actor,
                            unreliableOnCreate = actorVisibleDistanceOnCD.unreliableOnCreate,
                        });
                    }
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Actor), typeof(OnCreateMessage))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct CreateChangedJob : IJobForEachWithEntity<ActorVisibleDistanceOnSync>
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Rotation> rotationFromEntity;
            [ReadOnly] public ComponentDataFromEntity<RigidbodyVelocity> rigidbodyVelocityFromEntity;

            public void Execute(Entity entity, int index, ref ActorVisibleDistanceOnSync actorVisibleDistanceOnSync)
            {
                if (actorVisibleDistanceOnSync.syncType == SyncActorType.Translation)
                {
                    actorVisibleDistanceOnSync.lastTranslation = translationFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.Rotation)
                {
                    actorVisibleDistanceOnSync.lastRotation = rotationFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.Translation_Rotation)
                {
                    actorVisibleDistanceOnSync.lastTranslation = translationFromEntity[entity];
                    actorVisibleDistanceOnSync.lastRotation = rotationFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.RB_VelocityDirection)
                {
                    actorVisibleDistanceOnSync.lastRigidbodyVelocity = rigidbodyVelocityFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.RB_Translation_Velocity)
                {
                    actorVisibleDistanceOnSync.lastTranslation = translationFromEntity[entity];
                    actorVisibleDistanceOnSync.lastRigidbodyVelocity = rigidbodyVelocityFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.RB_Rotation_Velocity)
                {
                    actorVisibleDistanceOnSync.lastRotation = rotationFromEntity[entity];
                    actorVisibleDistanceOnSync.lastRigidbodyVelocity = rigidbodyVelocityFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.RB_Translation_Rotation_Velocity)
                {
                    actorVisibleDistanceOnSync.lastTranslation = translationFromEntity[entity];
                    actorVisibleDistanceOnSync.lastRotation = rotationFromEntity[entity];
                    actorVisibleDistanceOnSync.lastRigidbodyVelocity = rigidbodyVelocityFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.Disable)
                {
                    //do nothing...
                }
                else
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    throw new System.NotImplementedException();
#endif
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Actor))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct ChangedJob : IJobForEachWithEntity<ActorVisibleDistanceOnSync>
        {
            [ReadOnly] public NativeArray<bool> syncTypeEnables;

            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Rotation> rotationFromEntity;
            [ReadOnly] public ComponentDataFromEntity<RigidbodyVelocity> rigidbodyVelocityFromEntity;

            public void Execute(Entity entity, int index, ref ActorVisibleDistanceOnSync actorVisibleDistanceOnSync)
            {
                if (syncTypeEnables.Length > 0)
                {
                    if (syncTypeEnables[(int)actorVisibleDistanceOnSync.perFrame] == false)
                        return;
                }

                if (actorVisibleDistanceOnSync.changed)
                    return;


                //
                if (actorVisibleDistanceOnSync.syncType == SyncActorType.Translation)
                {
                    if (actorVisibleDistanceOnSync.lastTranslation.Value.Equals(translationFromEntity[entity].Value))
                        return;

                    actorVisibleDistanceOnSync.lastTranslation = translationFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.Rotation)
                {
                    if (actorVisibleDistanceOnSync.lastRotation.Value.Equals(rotationFromEntity[entity].Value))
                        return;

                    actorVisibleDistanceOnSync.lastRotation = rotationFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.Translation_Rotation)
                {
                    if (actorVisibleDistanceOnSync.lastTranslation.Value.Equals(translationFromEntity[entity].Value)
                    && actorVisibleDistanceOnSync.lastRotation.Value.Equals(rotationFromEntity[entity].Value))
                        return;

                    actorVisibleDistanceOnSync.lastTranslation = translationFromEntity[entity];
                    actorVisibleDistanceOnSync.lastRotation = rotationFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.RB_VelocityDirection)
                {
                    var dot = math.dot(math.normalize(actorVisibleDistanceOnSync.lastRigidbodyVelocity.linear), math.normalize(rigidbodyVelocityFromEntity[entity].linear));

                    if (dot > 0.999f)
                    {
                        var velocityOffset = math.distance(actorVisibleDistanceOnSync.lastRigidbodyVelocity.linear, rigidbodyVelocityFromEntity[entity].linear);
                        var distanceOffset = velocityOffset - actorVisibleDistanceOnSync.lastVelocityOffset;
                        var offset = distanceOffset - actorVisibleDistanceOnSync.lastDistanceOffset;

                        actorVisibleDistanceOnSync.lastVelocityOffset = velocityOffset;
                        actorVisibleDistanceOnSync.lastDistanceOffset = distanceOffset;

                        if (math.abs(offset) < actorVisibleDistanceOnSync.distanceOffset2Changed)
                            return;
                    }

                    //
                    actorVisibleDistanceOnSync.lastRigidbodyVelocity = rigidbodyVelocityFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.RB_Translation_Velocity)
                {
                    var rigidbodyVelocity = rigidbodyVelocityFromEntity[entity];
                    if (math.distancesq(actorVisibleDistanceOnSync.lastRigidbodyVelocity.linear, rigidbodyVelocity.linear) < actorVisibleDistanceOnSync.linearDistancesq2Changed
                    && math.distancesq(actorVisibleDistanceOnSync.lastTranslation.Value, translationFromEntity[entity].Value) < actorVisibleDistanceOnSync.distancesq2Changed)
                        return;

                    actorVisibleDistanceOnSync.lastTranslation = translationFromEntity[entity];
                    actorVisibleDistanceOnSync.lastRigidbodyVelocity = rigidbodyVelocityFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.RB_Rotation_Velocity)
                {
                    var rigidbodyVelocity = rigidbodyVelocityFromEntity[entity];
                    if (math.distancesq(actorVisibleDistanceOnSync.lastRigidbodyVelocity.angular, rigidbodyVelocity.angular) < actorVisibleDistanceOnSync.angularDistancesq2Changed
                    && Quaternion.Angle(actorVisibleDistanceOnSync.lastRotation.Value, rotationFromEntity[entity].Value) < actorVisibleDistanceOnSync.angle2Changed)
                        return;

                    actorVisibleDistanceOnSync.lastRotation = rotationFromEntity[entity];
                    actorVisibleDistanceOnSync.lastRigidbodyVelocity = rigidbodyVelocityFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.RB_Translation_Rotation_Velocity)
                {
                    var rigidbodyVelocity = rigidbodyVelocityFromEntity[entity];
                    if (math.distancesq(actorVisibleDistanceOnSync.lastRigidbodyVelocity.linear, rigidbodyVelocity.linear) < actorVisibleDistanceOnSync.linearDistancesq2Changed
                    && math.distancesq(actorVisibleDistanceOnSync.lastRigidbodyVelocity.angular, rigidbodyVelocity.angular) < actorVisibleDistanceOnSync.angularDistancesq2Changed
                    && math.distancesq(actorVisibleDistanceOnSync.lastTranslation.Value, translationFromEntity[entity].Value) < actorVisibleDistanceOnSync.distancesq2Changed
                    && Quaternion.Angle(actorVisibleDistanceOnSync.lastRotation.Value, rotationFromEntity[entity].Value) < actorVisibleDistanceOnSync.angle2Changed)
                        return;

                    actorVisibleDistanceOnSync.lastTranslation = translationFromEntity[entity];
                    actorVisibleDistanceOnSync.lastRotation = rotationFromEntity[entity];
                    actorVisibleDistanceOnSync.lastRigidbodyVelocity = rigidbodyVelocityFromEntity[entity];
                }
                else if (actorVisibleDistanceOnSync.syncType == SyncActorType.Disable)
                {
                    //do nothing...
                    return;
                }
                else
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    throw new System.NotImplementedException();
#endif
                }


                //
                actorVisibleDistanceOnSync.changed = true;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Actor))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct ResetChangedJob : IJobForEach<ActorVisibleDistanceOnSync>
        {
            public void Execute(ref ActorVisibleDistanceOnSync actorVisibleDistanceOnSync)
            {
                actorVisibleDistanceOnSync.changed = false;
            }
        }


        [BurstCompile]
        //[RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SyncVisibleDistanceJob : IJobForEach_BCC<ObserverSyncVisibleActorBuffer, ObserverPosition, ObserverVisibleDistance>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> actorEntitys;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<ActorVisibleDistanceOnSync> actorVisibleDistanceOnSyncs;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> translations;
            [ReadOnly] public NativeArray<bool> syncTypeEnables;
            public void Execute(DynamicBuffer<ObserverSyncVisibleActorBuffer> syncVisibleActorList, [ReadOnly] ref ObserverPosition observerPosition, [ReadOnly] ref ObserverVisibleDistance observerVisibleDistance)
            {
                for (int i = 0; i < actorEntitys.Length; ++i)
                {
                    var actorEntity = actorEntitys[i];

                    //
                    var actorVisibleDistanceOnSync = actorVisibleDistanceOnSyncs[i];

                    //
                    if (syncTypeEnables[(int)actorVisibleDistanceOnSync.perFrame] == false)
                        continue;

                    if (actorVisibleDistanceOnSync.changed == false)
                        continue;

                    //
                    var translation = translations[i];

                    if (observerVisibleDistance.all || math.distancesq(observerPosition.value, translation.Value) < observerVisibleDistance.valueSq)
                    {
                        syncVisibleActorList.Add(new ObserverSyncVisibleActorBuffer { synActorType = actorVisibleDistanceOnSync.syncType, actorEntity = actorEntity });
                    }
                }
            }
        }

        public struct Out
        {
            public Entity entity;
            public SyncActorType syncType;
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Actor))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct ForceGetSyncVisibleDistanceJob : IJobForEachWithEntity<ActorVisibleDistanceOnSync, ActorVisibleDistanceOnCD>
        {
            public NativeQueue<Out>.ParallelWriter outs;
            public void Execute(Entity entity, int index, [ReadOnly]ref ActorVisibleDistanceOnSync actorVisibleDistanceOnSync, [ReadOnly] ref ActorVisibleDistanceOnCD visibleDistanceOnCD)
            {
                if (visibleDistanceOnCD.isUnlimited && actorVisibleDistanceOnSync.changed)
                {
                    outs.Enqueue(new Out { entity = entity, syncType = actorVisibleDistanceOnSync.syncType });
                }
            }
        }

        [BurstCompile]
        //[RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct ForceSyncVisibleDistanceJob : IJobForEach_B<ObserverSyncVisibleActorBuffer>
        {
            [ReadOnly] public NativeArray<Out> ins;
            public void Execute(DynamicBuffer<ObserverSyncVisibleActorBuffer> synVisibleActorList)
            {
                for (var i = 0; i < ins.Length; ++i)
                {
                    var _in = ins[i];

                    //
                    synVisibleActorList.Add(new ObserverSyncVisibleActorBuffer
                    {
                        synActorType = _in.syncType,
                        actorEntity = _in.entity
                    });
                }
            }
        }

        public float forceSyncTime = 5f;
        float curForceSyncTime = 0f;
        NativeArray<byte> syncTypeCounts;
        NativeArray<bool> syncTypeEnables;
        NativeArray<bool> noneTypeEnables;

        protected override void OnDestroy()
        {
            syncTypeCounts.Dispose();
            syncTypeEnables.Dispose();
            noneTypeEnables.Dispose();

            outs.Dispose();
            ins.Dispose();
        }

        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            syncTypeCounts = new NativeArray<byte>((int)PerFrame.Max, Allocator.Persistent);
            syncTypeEnables = new NativeArray<bool>((int)PerFrame.Max, Allocator.Persistent);
            noneTypeEnables = new NativeArray<bool>(0, Allocator.Persistent);

            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();


            actorCreateMessageQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<OnCreateMessage>(),
                    ComponentType.ReadOnly<Actor>(),
                    ComponentType.ReadOnly<ActorOwner>(),
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<ActorVisibleDistanceOnCD>(),
                },
            });

            actorSyncQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<ActorVisibleDistanceOnSync>(),
                    ComponentType.ReadOnly<Translation>(),
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<OnDestroyMessage>(),
                },
            });
        }

        EntityQuery actorCreateMessageQuery;
        EntityQuery actorSyncQuery;

        NativeQueue<Out> outs = new NativeQueue<Out>(Allocator.Persistent);
        NativeList<Out> ins = new NativeList<Out>(Allocator.Persistent);
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //
            {
                var actorEntitys = actorCreateMessageQuery.ToEntityArray(Allocator.TempJob, out var arrayJobA);
                var actors = actorCreateMessageQuery.ToComponentDataArray<Actor>(Allocator.TempJob, out var arrayJobB);
                var actorOwnerPlayers = actorCreateMessageQuery.ToComponentDataArray<ActorOwner>(Allocator.TempJob, out var arrayJobC);
                var translations = actorCreateMessageQuery.ToComponentDataArray<Translation>(Allocator.TempJob, out var arrayJobD);
                var actorVisibleDistanceOnCDs = actorCreateMessageQuery.ToComponentDataArray<ActorVisibleDistanceOnCD>(Allocator.TempJob, out var arrayJobE);

                var arrayJobs = new NativeArray<JobHandle>(6, Allocator.Temp);
                arrayJobs[0] = inputDeps;
                arrayJobs[1] = arrayJobA;
                arrayJobs[2] = arrayJobB;
                arrayJobs[3] = arrayJobC;
                arrayJobs[4] = arrayJobD;
                arrayJobs[5] = arrayJobE;

                inputDeps = new CreateVisibleDistanceJob
                {
                    actorEntitys = actorEntitys,
                    actors = actors,
                    actorOwnerPlayers = actorOwnerPlayers,
                    translations = translations,
                    actorVisibleDistanceOnCDs = actorVisibleDistanceOnCDs,
                }
                .Schedule(this, JobHandle.CombineDependencies(arrayJobs));
            }


            //
            var translationFromEntity = GetComponentDataFromEntity<Translation>(true);
            var rotationFromEntity = GetComponentDataFromEntity<Rotation>(true);
            var rigidbodyVelocityFromEntity = GetComponentDataFromEntity<RigidbodyVelocity>(true);

            inputDeps = new CreateChangedJob
            {
                translationFromEntity = translationFromEntity,
                rotationFromEntity = rotationFromEntity,
                rigidbodyVelocityFromEntity = rigidbodyVelocityFromEntity,
            }
            .Schedule(this, inputDeps);


            //
            {
                curForceSyncTime += Time.fixedDeltaTime;

                if (curForceSyncTime > forceSyncTime)
                {
                    curForceSyncTime -= forceSyncTime;

                    //
                    {
                        inputDeps = new ChangedJob
                        {
                            syncTypeEnables = noneTypeEnables,

                            translationFromEntity = translationFromEntity,
                            rotationFromEntity = rotationFromEntity,
                            rigidbodyVelocityFromEntity = rigidbodyVelocityFromEntity,
                        }
                        .Schedule(this, inputDeps);

                        outs.Clear();
                        ins.Clear();

                        inputDeps = new ForceGetSyncVisibleDistanceJob
                        {
                            outs = outs.AsParallelWriter(),
                        }
                        .Schedule(this, inputDeps);

                        inputDeps = outs.ToListJob(ref ins, inputDeps);

                        inputDeps = new ForceSyncVisibleDistanceJob
                        {
                            ins = ins.AsDeferredJobArray(),
                        }
                        .Schedule(this, inputDeps);

                        inputDeps = new ResetChangedJob
                        {
                        }
                        .Schedule(this, inputDeps);

                        return inputDeps;
                    }
                }
            }


            //
            {
                for (byte i = (byte)PerFrame._1; i < (byte)PerFrame.Max; ++i)
                {
                    syncTypeCounts[i] += 1;

                    if (syncTypeCounts[i] > i)
                    {
                        syncTypeCounts[i] = 0;
                        syncTypeEnables[i] = true;
                    }
                    else
                    {
                        syncTypeEnables[i] = false;
                    }
                }


                //
                inputDeps = new ChangedJob
                {
                    syncTypeEnables = syncTypeEnables,

                    translationFromEntity = translationFromEntity,
                    rotationFromEntity = rotationFromEntity,
                    rigidbodyVelocityFromEntity = rigidbodyVelocityFromEntity,
                }
                .Schedule(this, inputDeps);

                inputDeps.Complete();
                var actorEntitys = actorSyncQuery.ToEntityArray(Allocator.TempJob, out var arrayJobA);
                var actorVisibleDistanceOnSyncs = actorSyncQuery.ToComponentDataArray<ActorVisibleDistanceOnSync>(Allocator.TempJob, out var arrayJobB);
                var translations = actorSyncQuery.ToComponentDataArray<Translation>(Allocator.TempJob, out var arrayJobC);
                var arrayJobs = new NativeArray<JobHandle>(4, Allocator.Temp);
                arrayJobs[0] = inputDeps;
                arrayJobs[1] = arrayJobA;
                arrayJobs[2] = arrayJobB;
                arrayJobs[3] = arrayJobC;


                inputDeps = new SyncVisibleDistanceJob
                {
                    actorEntitys = actorEntitys,
                    actorVisibleDistanceOnSyncs = actorVisibleDistanceOnSyncs,
                    translations = translations,

                    syncTypeEnables = syncTypeEnables,
                }
                .Schedule(this, JobHandle.CombineDependencies(arrayJobs));


                inputDeps = new ResetChangedJob
                {
                }
                .Schedule(this, inputDeps);
            }


            return inputDeps;
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerObserveDestroyServerSystem : JobComponentSystem
    {
        protected override void OnDestroy()
        {
            destroyActorOutsA.Dispose();
            destroyActorOutsB.Dispose();
            destroyActors.Dispose();
        }

        public struct DestroyActorOut
        {
            public Entity actorEntity;
            public ActorVisibleDistanceOnCD visibleDistanceOnCD;
            public Translation translation;
        }

        /*
        在服务器 只存在一帧(或者周期是固定)的actor 删除信息是不会被发送到客户端的
        */

        //Lifetime还没结束就被删除的 需要同步到客户端
        //Lifetime正常结束的删除是不会被发送到客户端的 因为客户端的entity也有自己的Lifetime
        [BurstCompile]
        [RequireComponentTag(typeof(OnDestroyMessage))]
        //[ExcludeComponent(typeof())]
        struct GetDestroyActorJobA : IJobForEachWithEntity<ActorVisibleDistanceOnCD, Translation, ActorLifetime>
        {
            public NativeQueue<DestroyActorOut>.ParallelWriter destroyActorOuts;
            public void Execute(Entity entity, int index, [ReadOnly]ref ActorVisibleDistanceOnCD actorVisibleDistanceOnCD, [ReadOnly]ref Translation translation, [ReadOnly] ref ActorLifetime actorLifetime)
            {
                if (actorLifetime.value <= 0f/* && actorLifetime.needSyncOnDestroy == false*/)
                    return;

                destroyActorOuts.Enqueue(new DestroyActorOut { actorEntity = entity, visibleDistanceOnCD = actorVisibleDistanceOnCD, translation = translation });
            }
        }

        //没有Lifetime并且可视距离是无限的 在被删除时需要同步到客户端
        [BurstCompile]
        [RequireComponentTag(typeof(OnDestroyMessage))]
        [ExcludeComponent(typeof(ActorLifetime))]
        struct GetDestroyActorJobB : IJobForEachWithEntity<ActorVisibleDistanceOnCD, Translation>
        {
            public NativeQueue<DestroyActorOut>.ParallelWriter destroyActorOuts;
            public void Execute(Entity entity, int index, [ReadOnly]ref ActorVisibleDistanceOnCD actorVisibleDistanceOnCD, [ReadOnly]ref Translation translation)
            {
                if (actorVisibleDistanceOnCD.isUnlimited == false)
                    return;

                destroyActorOuts.Enqueue(new DestroyActorOut { actorEntity = entity, visibleDistanceOnCD = actorVisibleDistanceOnCD, translation = translation });
            }
        }

        [BurstCompile]
        //[RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct DestroyVisibleDistanceJob : IJobForEach_BCC<ObserverDestroyVisibleActorBuffer, ObserverPosition, ObserverVisibleDistance>
        {
            [ReadOnly] public NativeArray<DestroyActorOut> destroyActors;
            public void Execute(DynamicBuffer<ObserverDestroyVisibleActorBuffer> destroyVisibleActorBuffer, [ReadOnly] ref ObserverPosition observerPosition, [ReadOnly] ref ObserverVisibleDistance observerVisibleDistance)
            {
                for (var i = 0; i < destroyActors.Length; ++i)
                {
                    var destroyActor = destroyActors[i];

                    //
                    var visibleDistanceOnCD = destroyActor.visibleDistanceOnCD;
                    var translation = destroyActor.translation;
                    if (visibleDistanceOnCD.isUnlimited || observerVisibleDistance.all || math.distancesq(observerPosition.value, translation.Value) < observerVisibleDistance.valueSq)
                    {
                        destroyVisibleActorBuffer.Add(new ObserverDestroyVisibleActorBuffer
                        {
                            actorEntity = destroyActor.actorEntity,
                        });
                    }
                }
            }
        }


        NativeQueue<DestroyActorOut> destroyActorOutsA = new NativeQueue<DestroyActorOut>(Allocator.Persistent);
        NativeQueue<DestroyActorOut> destroyActorOutsB = new NativeQueue<DestroyActorOut>(Allocator.Persistent);
        NativeList<DestroyActorOut> destroyActors = new NativeList<DestroyActorOut>(Allocator.Persistent);
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //
            destroyActorOutsA.Clear();
            destroyActorOutsB.Clear();
            destroyActors.Clear();


            inputDeps = new GetDestroyActorJobA
            {
                destroyActorOuts = destroyActorOutsA.AsParallelWriter(),
            }
            .Schedule(this, inputDeps);

            inputDeps = new GetDestroyActorJobB
            {
                destroyActorOuts = destroyActorOutsB.AsParallelWriter(),
            }
            .Schedule(this, inputDeps);

            inputDeps = NativeQueueEx.ToListJob(destroyActorOutsA, destroyActorOutsB, ref destroyActors, inputDeps);

            inputDeps = new DestroyVisibleDistanceJob
            {
                destroyActors = destroyActors.AsDeferredJobArray(),
            }
            .Schedule(this, inputDeps);


            return inputDeps;
        }
    }
}