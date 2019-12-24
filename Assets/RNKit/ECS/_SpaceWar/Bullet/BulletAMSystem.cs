using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class BulletAMServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;

        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }


        //[BurstCompile]
        [RequireComponentTag(typeof(OnPhysicsCallMessage), typeof(Bullet))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct AMJob : IJobForEachWithEntity_EBCCCC<PhysicsRaycastResults, ActorAttribute3Offset<_HP>, ActorLifetime, ActorOwner, Actor>
        {
            public BufferFromEntity<ActorAttribute3Modifys<_HP>> hpModifyFromEntity;

            public EntityCommandBuffer.Concurrent endCommandBuffer;

            public void Execute(Entity bulletEntity, int index,
                [ReadOnly, ChangedFilter]DynamicBuffer<PhysicsRaycastResults> raycastResults,
                [ReadOnly]ref ActorAttribute3Offset<_HP> hpOffset, [ReadOnly]ref ActorLifetime actorLifetime,
                [ReadOnly]ref ActorOwner actorOwner, [ReadOnly]ref Actor actor)
            {
                if (raycastResults.Length <= 0)
                    return;

                var hp = hpOffset.GetValue(actorLifetime.percent);

                for (var i = 0; i < raycastResults.Length; ++i)
                {
                    var targetEntity = raycastResults[i].entity;

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
                    var translation = new Translation { Value = raycastResults[i].point };
                    AttributeModifyFxSpawner.createInServer(endCommandBuffer, index, actor.actorType, translation, hp, 0f, default);
                }


                endCommandBuffer.AddComponent(index, bulletEntity, new OnDestroyMessage { });
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new AMJob
            {
                hpModifyFromEntity = GetBufferFromEntity<ActorAttribute3Modifys<_HP>>(),

                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .ScheduleSingle(this, inputDeps);

            endBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}
