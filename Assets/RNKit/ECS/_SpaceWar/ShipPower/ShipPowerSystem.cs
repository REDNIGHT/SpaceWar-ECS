using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipPowerServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;

        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        [BurstCompile]
        struct PowerLevelJob : IJobForEach<ShipPowers, ActorAttribute3<_Power>, ActorAttribute1<_PowerLevel>>
        {
            public void Execute([ReadOnly] ref ShipPowers shipPowers, ref ActorAttribute3<_Power> power, [ReadOnly, ChangedFilter] ref ActorAttribute1<_PowerLevel> powerLevel)
            {
                var level = (int)powerLevel.value;
                var powerInfo = shipPowers.get(level);

                power.max = powerInfo.max;
                power.regain = powerInfo.regain;
            }
        }

        [BurstCompile]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct DisableInputTimeJob : IJobForEachWithEntity<ShipLostInputState>
        {
            public float fixedDeltaTime;
            public ComponentType ShipDisableInputState;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity shipEntity, int index, ref ShipLostInputState shipDisableInputState)
            {
                shipDisableInputState.time -= fixedDeltaTime;
                if (shipDisableInputState.time <= 0f)
                {
                    endCommandBuffer.RemoveComponent(index, shipEntity, ShipDisableInputState);
                }
            }
        }

        //[BurstCompile]
        [RequireComponentTag(typeof(Ship))]
        [ExcludeComponent(typeof(ShipLostInputState), typeof(OnDestroyMessage))]
        struct DisableInputJob : IJobForEachWithEntity<ActorAttribute3<_Power>, ShipPowers, Translation>
        {
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity shipEntity, int index,
                [ReadOnly]ref ActorAttribute3<_Power> power,
                [ReadOnly]ref ShipPowers shipPowers, [ReadOnly]ref Translation translation)
            {
                if (power.value <= 0f)
                {
                    var lostInputTime = shipPowers.lostInputTime + math.abs(power.value) * shipPowers.power2Time;

                    endCommandBuffer.AddComponent(index, shipEntity, new ShipLostInputState { time = shipPowers.lostInputTime = lostInputTime });
                    endCommandBuffer.SetComponent(index, shipEntity, new ShipMoveInput { lost = true });

                    ShipLostInputFxSpawner.createInServer(endCommandBuffer, index, shipEntity, translation, lostInputTime);
                }
            }
        }

        [BurstCompile]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct PowerRegainJob : IJobForEach<ActorAttribute3<_Power>, ControlForceDirection, ControlTorqueAngular>
        {
            public float fixedDeltaTime;

            public void Execute(ref ActorAttribute3<_Power> attribute3, ref ControlForceDirection controlForceDirection, ref ControlTorqueAngular controlTorqueAngular)
            {
                var scale = 1f;
                if (controlForceDirection.force > 0f)
                {
                    scale -= 0.5f;
                }
                if (controlTorqueAngular.torque > 0f)
                {
                    scale -= 0.25f;
                }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (attribute3.regain < 0f)
                    throw new System.Exception("attribute3.regain < 0f");
#endif
                //value
                attribute3.value += attribute3.regain * scale * fixedDeltaTime;
                if (attribute3.value >= attribute3.max)
                    attribute3.value = attribute3.max;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //
            var inputDepsA = new PowerLevelJob
            {
            }
            .Schedule(this, inputDeps);


            //
            var inputDepsB = new DisableInputTimeJob
            {
                fixedDeltaTime = Time.fixedDeltaTime,
                ShipDisableInputState = typeof(ShipLostInputState),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);


            //
            inputDepsA = new ActorAttribute3ServerSystem<_Power>.SampleAttribute3ModifysJob
            {
            }
            .Schedule(this, inputDepsA);

            //
            inputDepsA = new DisableInputJob
            {
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDepsA);

            //
            inputDepsA = new PowerRegainJob
            {
                fixedDeltaTime = Time.fixedDeltaTime,
            }
            .Schedule(this, inputDepsA);

            //
            inputDeps = JobHandle.CombineDependencies(inputDepsA, inputDepsB);
            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipPowerClientSystem : ComponentSystem
    {
        IActorSpawnerMap actorSpawnerMap;
        ActorSyncCreateClientSystem actorClientSystem;

        protected void OnInit(Transform root)
        {
            actorSpawnerMap = root.GetComponentInChildren<IActorSpawnerMap>();
            actorClientSystem = World.GetExistingSystem<ActorSyncCreateClientSystem>();

            powerLevelQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<ShipPowers>(), ComponentType.ReadOnly<ActorAttribute1<_PowerLevel>>(), ComponentType.ReadWrite<ActorAttribute3<_Power>>() },
            });
            powerLevelQuery.SetFilterChanged(typeof(ActorAttribute1<_PowerLevel>));
        }
        EntityQuery powerLevelQuery;

        protected override void OnUpdate()
        {
            //
            Entities
                .With(powerLevelQuery)
                .ForEach((ref ShipPowers shipPowers, ref ActorAttribute3<_Power> power, ref ActorAttribute1<_PowerLevel> powerLevel) =>
                {
                    var level = (int)powerLevel.value;
                    var powerInfo = shipPowers.get(level);

                    power.max = powerInfo.max;
                    power.regain = powerInfo.regain;
                });


            //
            Entities
                .WithAll<ShipLostInputState>()
                .ForEach((Entity shipEntity, ref ShipLostInputState disableInputState) =>
                {
                    disableInputState.time -= Time.fixedDeltaTime;
                    if (disableInputState.time <= 0f)
                    {
                        PostUpdateCommands.RemoveComponent<ShipLostInputState>(shipEntity);
                    }
                });

            var myShipEntity = Entity.Null;
            var myPlayerSingleton = GetSingleton<MyPlayerSingleton>();
            if (myPlayerSingleton.playerEntity != default)
            {
                myShipEntity = EntityManager.GetComponentData<PlayerActorArray>(myPlayerSingleton.playerEntity).shipEntity;
            }

            Entities
                .WithAllReadOnly<ShipLostInputFx, ActorCreator, OnCreateMessage>()
                .ForEach((Entity disableInputFxEntity, ref ActorCreator actorCreator, ref ShipLostInputFx shipLostInputFx) =>
                {
                    //
                    if (actorCreator.entity == myShipEntity)
                    {
                        Debug.Assert(myShipEntity != default, "myShipEntity != default");
                        EntityManager.AddComponentData(actorCreator.entity, new ShipLostInputState { time = shipLostInputFx.time });
                        EntityManager.SetComponentData(actorCreator.entity, new ShipMoveInput { lost = true });
                    }


                    //
                    var shipT = EntityManager.GetComponentObject<Transform>(actorCreator.entity);
                    var disableInputFxT = shipT.GetChild(ShipSpawner.DisableInputFx_TransformIndex);
                    var fx = disableInputFxT.GetComponent<IShipLostInputFx>();
                    if (fx != null)
                    {
                        fx.OnPlayFx(shipLostInputFx.time);
                    }
                    else
                    {
                        disableInputFxT.gameObject.SetActive(true);
                    }
                });
        }
    }
}
