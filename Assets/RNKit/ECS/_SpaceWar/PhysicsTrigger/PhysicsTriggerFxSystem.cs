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
    public class PhysicsTriggerFxServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;

        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        //[BurstCompile]
        //[RequireComponentTag(typeof())]
        struct PhysicsTriggerFxJob : IJobForEachWithEntity_EBCC<PhysicsTriggerResults, PhysicsTriggerFx, Actor>
        {
            [ReadOnly] public ComponentDataFromEntity<Bullet> bulletFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;

            public EntityCommandBuffer.Concurrent endCommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly, ChangedFilter]DynamicBuffer<PhysicsTriggerResults> physicsTriggerResults, [ReadOnly] ref PhysicsTriggerFx physicsTriggerFx, [ReadOnly] ref Actor actor)
            {
                for (var i = 0; i < physicsTriggerResults.Length; ++i)
                {
                    if (physicsTriggerResults[i].state != physicsTriggerFx.includeResultState)
                        continue;

                    var targetEntity = physicsTriggerResults[i].entity;

                    if (bulletFromEntity.Exists(targetEntity))
                        continue;

                    var translation = translationFromEntity[targetEntity];

                    AttributeModifyFxSpawner.createInServer(endCommandBuffer, index, actor.actorType, translation, half.zero, half.zero, targetEntity);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new PhysicsTriggerFxJob
            {
                bulletFromEntity = GetComponentDataFromEntity<Bullet>(true),
                translationFromEntity = GetComponentDataFromEntity<Translation>(true),

                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}
