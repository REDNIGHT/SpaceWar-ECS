using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class LaserUpdateServerSystem : JobComponentSystem//
    {
        [BurstCompile]
        [RequireComponentTag(typeof(Laser), typeof(CallTrigger))]
        [ExcludeComponent(typeof(OnDestroyMessage))]
        struct LaserControlJobA : IJobForEachWithEntity<Laser_TR_Temp, WeaponCreator>
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> weaponTranslationFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Rotation> weaponRotationFromEntity;
            public ComponentType OnDestroyMessage;
            public EntityCommandBuffer.Concurrent endCommandBuffer;
            public void Execute(Entity laserEntity, int index, ref Laser_TR_Temp laser_TR_Temp, [ReadOnly] ref WeaponCreator weaponCreator)
            {
                if (weaponTranslationFromEntity.Exists(weaponCreator.entity))
                {
                    laser_TR_Temp.position = weaponTranslationFromEntity[weaponCreator.entity].Value;
                    laser_TR_Temp.rotation = weaponRotationFromEntity[weaponCreator.entity].Value;
                }
                else
                {
                    //武器开启自爆模式后 武器被删除 激光应该一起删除
                    endCommandBuffer.AddComponent(index, laserEntity, OnDestroyMessage);
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Laser), typeof(CallTrigger))]
        struct LaserControlJobB : IJobForEach<Translation, Rotation, Laser_TR_Temp>
        {
            public void Execute(ref Translation Translation, ref Rotation Rotation, [ReadOnly] ref Laser_TR_Temp TR_Temp)
            {
                Translation.Value = TR_Temp.position;
                Rotation.Value = TR_Temp.rotation;
            }
        }

        EndCommandBufferSystem endBarrier;
        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
            Debug.Assert(endBarrier != null, "endBarrier != null");
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new LaserControlJobA
            {
                weaponTranslationFromEntity = GetComponentDataFromEntity<Translation>(true),
                weaponRotationFromEntity = GetComponentDataFromEntity<Rotation>(true),

                OnDestroyMessage = typeof(OnDestroyMessage),
                endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);

            inputDeps = new LaserControlJobB
            {
            }
            .Schedule(this, inputDeps);

            return inputDeps;
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class LaserUpdateClientSystem : ComponentSystem//
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<Laser, ActorLifetime, WeaponCreator>()
                .WithAll<Translation, Rotation>()
                .ForEach((ref Translation translation, ref Rotation rotation, ref WeaponCreator weaponCreator) =>
                {
                    var weaponTranslation = EntityManager.GetComponentData<Translation>(weaponCreator.entity);
                    var weaponRotation = EntityManager.GetComponentData<Rotation>(weaponCreator.entity);

                    translation.Value = weaponTranslation.Value;
                    rotation.Value = weaponRotation.Value;
                });
        }
    }
}
