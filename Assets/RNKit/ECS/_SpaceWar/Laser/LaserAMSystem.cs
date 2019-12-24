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
    public class LaserAMServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;

        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        //[BurstCompile]
        [RequireComponentTag(typeof(OnPhysicsCallMessage))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct AMJob : IJobForEachWithEntity_EBCCCCC<PhysicsRaycastResults, Laser, ActorAttribute3Offset<_HP>, Translation, ActorOwner, Actor>
        {
            public BufferFromEntity<ActorAttribute3Modifys<_HP>> hpModifyFromEntity;

            public EntityCommandBuffer.Concurrent endCommandBuffer;

            public void Execute(Entity laserEntity, int index,
                [ReadOnly, ChangedFilter]DynamicBuffer<PhysicsRaycastResults> raycastResults,
                [ReadOnly]ref Laser laser, [ReadOnly]ref ActorAttribute3Offset<_HP> hpOffset, [ReadOnly]ref Translation laserTranslation,
                [ReadOnly]ref ActorOwner actorOwner, [ReadOnly]ref Actor actor)
            {
                for (var i = 0; i < raycastResults.Length; ++i)
                {
                    var raycastResult = raycastResults[i];

                    var distance = math.distance(laserTranslation.Value, raycastResult.point);
                    var percent = 1f - distance / laser.distance;
                    var hp = hpOffset.GetValue(percent);


                    var targetEntity = raycastResult.entity;

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
                    var translation = new Translation { Value = raycastResult.point };
                    AttributeModifyFxSpawner.createInServer(endCommandBuffer, index, actor.actorType, translation, hp, 0f, default);
                }
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
