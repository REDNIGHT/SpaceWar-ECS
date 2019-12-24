using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class AttributeTriggerAMServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;

        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        //[BurstCompile]
        [RequireComponentTag(typeof(OnPhysicsCallMessage))]
        struct AMJob : IJobForEachWithEntity_EBCCC<PhysicsTriggerResults, ActorAttribute3Offset<_HP>, ActorOwner, Actor>
        {
            public Unity.Mathematics.Random random;
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;

            public BufferFromEntity<ActorAttribute3Modifys<_HP>> hpModifyFromEntity;

            public EntityCommandBuffer.Concurrent endCommandBuffer;

            public void Execute(Entity entity, int index,
                [ReadOnly, ChangedFilter]DynamicBuffer<PhysicsTriggerResults> physicsTriggerResults,
                [ReadOnly] ref ActorAttribute3Offset<_HP> hpOffset,
                [ReadOnly]ref ActorOwner actorOwner, [ReadOnly]ref Actor actor)
            {
                for (var i = 0; i < physicsTriggerResults.Length; ++i)
                {
                    var targetEntity = physicsTriggerResults[i].entity;

                    var hp = hpOffset.GetValue(random.NextFloat(0f, 1f));

                    if (hpModifyFromEntity.Exists(targetEntity))
                    {
                        hpModifyFromEntity[targetEntity].Add(new ActorAttribute3Modifys<_HP>
                        {
                            player = actorOwner.playerEntity,
                            type = actor.actorType,
                            value = hp,
                            attribute3ModifyType = Attribute3SubModifyType.ValueOffset
                        });

                    }

                    //
                    var translation = translationFromEntity[targetEntity];
                    AttributeModifyFxSpawner.createInServer(endCommandBuffer, index, actor.actorType, translation, hp, 0f, default);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new AMJob
            {
                random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue)),
                translationFromEntity = GetComponentDataFromEntity<Translation>(true),
                hpModifyFromEntity = GetBufferFromEntity<ActorAttribute3Modifys<_HP>>(),

                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .ScheduleSingle(this, inputDeps);

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}
