using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    public enum TriggerResultState
    {
        None = 0,
        Enter = 1,
        Stay = 2,
        Exit = 3,
    }

    public struct Trigger : IComponentData
    {
        /// <summary>
        /// 根据这TriggerResultState进行计数
        /// 计数最好用Stay或Exit
        /// 如果用Enter计数  最后一个角色进入后触发器就被删除了爆炸了  效果很懵逼
        /// </summary>
        public TriggerResultState includeResultState;

        /// <summary>
        /// 计数为0时删除触发器
        /// </summary>
        public int count;
    }


    public struct PhysicsTriggerResults : IBufferElementData
    {
        public TriggerResultState state;
        public Entity entity;
    }

    public partial class OnTrigger : EntityBehaviour<OnTrigger>
    {
        public bool enterEnable;
        public bool stayEnable;
        public bool exitEnable;

        /// <summary>
        /// 对刚体的操作是在OnTriggerXXX之前的
        /// PhysicsTriggerResults的结果却在OnTriggerXXX之后
        /// 如果需要对刚体进行操作 就把这变量设为true 这样就可以把PhysicsTriggerResults的数据在下一帧开始时存到PhysicsRigidbodyResults里
        /// </summary>
        public bool toPhysicsRigidbodyResults;


        public static void _initComponent(Entity entity, EntityManager entityManager, bool enterEnable, bool stayEnable, bool exitEnable, bool toPhysicsRigidbodyResults)
        {
            var onTrigger = entityManager.GetComponentObject<GameObject>(entity).GetComponent<OnTrigger>();
            onTrigger.entity = entity;
            onTrigger.world = entityManager.World;

            onTrigger.enterEnable = enterEnable;
            onTrigger.stayEnable = stayEnable;
            onTrigger.exitEnable = exitEnable;
            onTrigger.toPhysicsRigidbodyResults = toPhysicsRigidbodyResults;

            Debug.Assert(onTrigger.gameObject == entityManager.GetComponentObject<GameObject>(entity),
                $"{onTrigger.gameObject} == {entityManager.GetComponentObject<GameObject>(entity)}",
                onTrigger.gameObject);
        }

        public static void _initComponent(Component component, Entity entity, EntityManager entityManager, bool enterEnable, bool stayEnable, bool exitEnable, bool toPhysicsRigidbodyResults)
        {
#if UNITY_EDITOR
            Debug.Assert(component != null, $"component != null  entity={entityManager.GetName(entity)}");
#endif

            var onTrigger = component.GetComponent<OnTrigger>();
            onTrigger.entity = entity;
            onTrigger.world = entityManager.World;

            onTrigger.enterEnable = enterEnable;
            onTrigger.stayEnable = stayEnable;
            onTrigger.exitEnable = exitEnable;
            onTrigger.toPhysicsRigidbodyResults = toPhysicsRigidbodyResults;
        }

        public static new void _removeComponent(Entity entity, EntityManager entityManager)
        {
            var go = entityManager.GetComponentObject<GameObject>(entity);
            var onTrigger = go.GetComponent<OnTrigger>();
            Debug.Assert(onTrigger != null, $"onTrigger != null  go={go}", go);
            onTrigger.destroy();

            go.GetComponent<Collider>().destroy();
        }

        public static new void _removeComponent(Component component, Entity entity, EntityManager entityManager)
        {
            var onTrigger = component.GetComponent<OnTrigger>();
            Debug.Assert(onTrigger != null, $"onTrigger != null  component={component}", component);
            onTrigger.destroy();

            component.GetComponent<Collider>().destroy();
        }


        void FixedUpdate()
        {
            var triggerResults = entityManager.GetBuffer<PhysicsTriggerResults>(entity);
            if (triggerResults.Length == 0)
                return;


            if (toPhysicsRigidbodyResults)
            {
                var physicsRigidbodyResults = entityManager.GetBuffer<PhysicsResults>(entity);
                foreach (var r in triggerResults)
                {
                    if (r.state == TriggerResultState.Stay)
                    {
                        physicsRigidbodyResults.Add(new PhysicsResults { entity = r.entity });
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (enterEnable == false) return;
            if (entityManager == null) return;
            if (entityManager.Exists(entity) == false) return;

            if (EntityBehaviour.getEntity(other.attachedRigidbody, out Entity transformEntity, entityManager.World))
            {
                entityManager.GetBuffer<PhysicsTriggerResults>(entity).Add(new PhysicsTriggerResults { state = TriggerResultState.Enter, entity = transformEntity });
            }
            else
            {
                var entityBehaviour = other.GetComponent<EntityBehaviour>();
                if (entityBehaviour == null) return;

                //别的world里的Collider 进入到这Trigger
                if (world != entityBehaviour.world) return;

#if UNITY_EDITOR
                var entityName = entityManager.GetName(entityBehaviour.entity);
#else
                var entityName = "";
#endif
                Debug.LogError($"EntityBehaviour.getEntity(other={other}, ...) == false" +
                    $"\nentityBehaviour.entity={entityBehaviour.entity}  {entityName}  Exists={entityManager.Exists(entityBehaviour.entity)}" +
                    $"\nentityBehaviour.world={entityBehaviour.world}  entityManager.World={entityManager.World}" +
                    $"\nthis={this}"
                    , other);
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (stayEnable == false) return;
            if (entityManager == null) return;
            if (entityManager.Exists(entity) == false) return;

            if (EntityBehaviour.getEntity(other.attachedRigidbody, out Entity transformEntity, entityManager.World))
            {
                entityManager.GetBuffer<PhysicsTriggerResults>(entity).Add(new PhysicsTriggerResults { state = TriggerResultState.Stay, entity = transformEntity });
            }
            else
            {
                var entityBehaviour = other.GetComponent<EntityBehaviour>();
                if (entityBehaviour == null) return;

                //别的world里的Collider 进入到这Trigger
                if (world != entityBehaviour.world) return;

                //if (entityManager.Exists(entityBehaviour.entity) == false) return;

#if UNITY_EDITOR
                var entityName = entityManager.GetName(entityBehaviour.entity);
#else
                var entityName = "";
#endif
                Debug.LogError($"EntityBehaviour.getEntity(other={other}, ...) == false" +
                    $"\nentityBehaviour.entity={entityBehaviour.entity}  {entityName}  Exists={entityManager.Exists(entityBehaviour.entity)}" +
                    $"\nentityBehaviour.world={entityBehaviour.world}  entityManager.World={entityManager.World}" +
                    $"\nthis={this}"
                    , other);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (exitEnable == false) return;
            if (entityManager == null) return;
            if (entityManager.Exists(entity) == false) return;

            if (EntityBehaviour.getEntity(other.attachedRigidbody, out Entity transformEntity, entityManager.World))
            {
                entityManager.GetBuffer<PhysicsTriggerResults>(entity).Add(new PhysicsTriggerResults { state = TriggerResultState.Exit, entity = transformEntity });
            }
            else
            {
                var entityBehaviour = other.GetComponent<EntityBehaviour>();
                if (entityBehaviour == null) return;

                //别的world里的Collider 进入到这Trigger
                if (world != entityBehaviour.world) return;


#if UNITY_EDITOR
                var entityName = entityManager.GetName(entityBehaviour.entity);
#else
                var entityName = "";
#endif
                Debug.LogError($"EntityBehaviour.getEntity(other={other}, ...) == false" +
                    $"\nentityBehaviour.entity={entityBehaviour.entity}  {entityName}  Exists={entityManager.Exists(entityBehaviour.entity)}" +
                    $"\nentityBehaviour.world={entityBehaviour.world}  entityManager.World={entityManager.World}" +
                    $"\nthis={this}"
                    , other);
            }
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    class TriggerClearSystem : JobComponentSystem
    {
        [BurstCompile]
        //[RequireComponentTag(typeof())]
        struct ClearJob : IJobForEach_B<PhysicsTriggerResults>
        {
            public void Execute([ChangedFilter]DynamicBuffer<PhysicsTriggerResults> physicsTriggerResults)
            {
                physicsTriggerResults.Clear();
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ClearJob { }.Schedule(this, inputDeps);
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    class TriggerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        //public TransformMapSystem transformMapSystem;
        protected override void OnDestroy()
        {
        }
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
            //transformMapSystem = World.GetExistingSystem<TransformMapSystem>();
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OnPhysicsCallMessage))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct TriggerJob : IJobForEachWithEntity_EBC<PhysicsTriggerResults, Trigger>
        {
            public ComponentType OnDestroyMessage;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity entity, int index, [ChangedFilter]DynamicBuffer<PhysicsTriggerResults> physicsTriggerResults, ref Trigger trigger)
            {
                if (trigger.includeResultState == TriggerResultState.None)
                {
                    for (var i = 0; i < physicsTriggerResults.Length; ++i)
                    {
                        if (trigger.count > 0)
                        {
                            --trigger.count;
                            if (trigger.count == 0)
                            {
                                endCommandBuffer.AddComponent(index, entity, OnDestroyMessage);

                                //
                                var removeIndex = i + 1;
                                if (physicsTriggerResults.Length > removeIndex)
                                {
                                    physicsTriggerResults.RemoveRange(removeIndex, physicsTriggerResults.Length - removeIndex);
                                }
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (trigger.includeResultState != TriggerResultState.None)
                        throw new System.Exception("//test...");

                    for (var i = 0; i < physicsTriggerResults.Length; ++i)
                    {
                        if (physicsTriggerResults[i].state != trigger.includeResultState)
                            continue;

                        if (trigger.count > 0)
                        {
                            --trigger.count;
                            if (trigger.count == 0)
                            {
                                endCommandBuffer.AddComponent(index, entity, OnDestroyMessage);

                                //
                                for (var j = physicsTriggerResults.Length - 1; j > i; --j)
                                {
                                    if (physicsTriggerResults[j].state == trigger.includeResultState)
                                    {
                                        physicsTriggerResults.RemoveAt(j);
                                    }
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new TriggerJob
            {
                OnDestroyMessage = typeof(OnDestroyMessage),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}
