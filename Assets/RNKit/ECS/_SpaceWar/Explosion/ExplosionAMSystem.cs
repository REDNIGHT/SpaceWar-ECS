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
    public class ExplosionAMServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;

        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        //[BurstCompile]
        [RequireComponentTag(typeof(OnPhysicsCallMessage))]
        struct AMJob : IJobForEachWithEntity_EBBCCCC<PhysicsResults, PhysicsOverlapHitPoints, Explosion, ActorAttribute3Offset<_HP>, ActorOwner, Actor>
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;

            public BufferFromEntity<ActorAttribute3Modifys<_HP>> hpModifyFromEntity;

            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity explosionEntity, int index,
                [ReadOnly, ChangedFilter]DynamicBuffer<PhysicsResults> rigidbodyResults,
                [ReadOnly]DynamicBuffer<PhysicsOverlapHitPoints> overlapHitPoints,
                [ReadOnly]ref Explosion explosion, [ReadOnly]ref ActorAttribute3Offset<_HP> hpOffset,
                [ReadOnly]ref ActorOwner actorOwner, [ReadOnly]ref Actor actor)
            {
                for (var i = 0; i < rigidbodyResults.Length; ++i)
                {
                    var targetEntity = rigidbodyResults[i].entity;

                    var targetTranslation = translationFromEntity[targetEntity];
                    var explosionTranslation = translationFromEntity[explosionEntity];


                    var distance = math.distance(explosionTranslation.Value, targetTranslation.Value);
                    var percent = 0f;
                    if (distance < explosion.radius)
                        percent = 1f - distance / explosion.radius;

                    var hp = hpOffset.GetValue(percent);

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
                    var translation = new Translation { Value = overlapHitPoints[i].value };
                    AttributeModifyFxSpawner.createInServer(endCommandBuffer, index, actor.actorType, translation, hp, 0f, default);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new AMJob
            {
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
