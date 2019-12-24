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
    public class CatchWeaponServerSystem : JobComponentSystem
    {
        public struct FindOut
        {
            public Entity weaponEntity;
            /// <summary>
            /// 右键自动开火
            /// </summary>
            public bool autoFire;

            public Entity shipEntity => slot.shipEntity;

            public Entity slotEntity;
            public Slot slot;
        }

        [BurstCompile]
        [ExcludeComponent(typeof(SlotUsingState), typeof(AssistSlot), typeof(OnCreateMessage), typeof(OnDestroyMessage))]
        struct CatchWeaponJob : IJobForEachWithEntity<Slot, Translation>
        {
            public float catchVelocityOffsetSq;
            public float catchRadiusSq;

            [ReadOnly] public ComponentDataFromEntity<RigidbodyVelocity> rigidbodyVelocityEntity;

            [ReadOnly] public NativeArray<Entity> weaponEntitys;
            [ReadOnly] public NativeArray<Weapon> weapons;
            [ReadOnly] public NativeArray<Translation> weaponTranslations;

            public NativeQueue<FindOut>.ParallelWriter _outs;

            public void Execute(Entity slotEntity, int index, [ReadOnly]ref Slot slot, [ReadOnly]ref Translation slotTranslation)
            {
                for (var i = 0; i < weaponEntitys.Length; ++i)
                {
                    if (weapons[i].type != WeaponType.Attack)
                        continue;

                    var b = _catch(slotEntity, slot, slotTranslation, rigidbodyVelocityEntity[slot.shipEntity].linear,
                        rigidbodyVelocityEntity[weaponEntitys[i]].linear, weaponTranslations[i],
                        catchVelocityOffsetSq, catchRadiusSq);

                    if (b)
                    {
                        _outs.Enqueue(new FindOut
                        {
                            weaponEntity = weaponEntitys[i],
                            autoFire = weapons[i].autoFire,
                            slotEntity = slotEntity,
                            slot = slot,
                        });

                        return;
                    }
                }
            }

            public static bool _catch(Entity slotEntity, in Slot slot, in Translation slotTranslation, in float3 slotVeloctiy,
                in float3 weaponVeloctiy, in Translation weaponTranslation,
                float catchVelocityOffsetSq, float catchRadiusSq)
            {
                if (math.lengthsq(weaponVeloctiy) > 1f)//武器在移动
                {
                    var velocityOffset = weaponVeloctiy - slotVeloctiy;//都在移动的情况下 只要移动方向和速度差不多 就能捕捉成功
                    var _catchVelocityOffsetSq = 1f;
                    if (math.lengthsq(slotVeloctiy) > 1f)//ship在移动
                    {
                        _catchVelocityOffsetSq = catchVelocityOffsetSq;
                    }

                    if (math.lengthsq(velocityOffset) > _catchVelocityOffsetSq)
                        return false;
                }

                if (math.distancesq(slotTranslation.Value, weaponTranslation.Value) > catchRadiusSq)
                    return false;

                return true;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(AssistSlot))]
        [ExcludeComponent(typeof(SlotUsingState), typeof(OnDestroyMessage))]
        struct CatchAssistWeaponJob : IJobForEachWithEntity<Slot, Translation>
        {
            public float catchRadiusSq;
            public float catchVelocityOffsetSq;

            [ReadOnly] public ComponentDataFromEntity<RigidbodyVelocity> rigidbodyVelocityEntity;

            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> weaponEntitys;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Weapon> weapons;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> weaponTranslations;

            public NativeQueue<FindOut>.ParallelWriter _outs;

            public void Execute(Entity slotEntity, int index, [ReadOnly]ref Slot slot, [ReadOnly]ref Translation slotTranslation)
            {
                for (var i = 0; i < weaponEntitys.Length; ++i)
                {
                    if (weapons[i].type == WeaponType.Attack)
                        continue;

                    var b = CatchWeaponJob._catch(slotEntity, slot, slotTranslation, rigidbodyVelocityEntity[slot.shipEntity].linear,
                        rigidbodyVelocityEntity[weaponEntitys[i]].linear, weaponTranslations[i],
                        catchVelocityOffsetSq, catchRadiusSq);

                    if (b)
                    {
                        _outs.Enqueue(new FindOut
                        {
                            weaponEntity = weaponEntitys[i],
                            slotEntity = slotEntity,
                            slot = slot,
                        });

                        return;
                    }
                }
            }
        }

        public float catchRadius { set => catchRadiusSq = value * value; }
        public float catchVelocityOffset { set => catchVelocityOffsetSq = value * value; }
        float catchRadiusSq = 1f;
        float catchVelocityOffsetSq = 1f;

        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();

            weaponQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<Weapon>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<RigidbodyVelocity>() },
                None = new ComponentType[] { ComponentType.ReadOnly<WeaponInstalledState>(), ComponentType.ReadOnly<OnDestroyMessage>()/*, ComponentType.ReadOnly<WeaponExplosionSelf>()*/ },
            });
        }
        EntityQuery weaponQuery;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var weaponEntitys = weaponQuery.ToEntityArray(Allocator.TempJob, out var arrayJobA);
            var weapons = weaponQuery.ToComponentDataArray<Weapon>(Allocator.TempJob, out var arrayJobB);
            var weaponTranslations = weaponQuery.ToComponentDataArray<Translation>(Allocator.TempJob, out var arrayJobC);
            var arrayJobs = new NativeArray<JobHandle>(4, Allocator.Temp);
            arrayJobs[0] = inputDeps;
            arrayJobs[1] = arrayJobA;
            arrayJobs[2] = arrayJobB;
            arrayJobs[3] = arrayJobC;

            var _outsA = new NativeQueue<FindOut>(Allocator.TempJob);
            var _outsB = new NativeQueue<FindOut>(Allocator.TempJob);
            {
                var rigidbodyVelocityEntity = GetComponentDataFromEntity<RigidbodyVelocity>(true);
                inputDeps = new CatchWeaponJob
                {
                    catchRadiusSq = catchRadiusSq,
                    catchVelocityOffsetSq = catchVelocityOffsetSq,

                    rigidbodyVelocityEntity = rigidbodyVelocityEntity,

                    weaponEntitys = weaponEntitys,
                    weapons = weapons,
                    weaponTranslations = weaponTranslations,

                    _outs = _outsA.AsParallelWriter(),
                }
                .Schedule(this, JobHandle.CombineDependencies(arrayJobs));

                inputDeps = new CatchAssistWeaponJob
                {
                    catchRadiusSq = catchRadiusSq,
                    catchVelocityOffsetSq = catchVelocityOffsetSq,

                    rigidbodyVelocityEntity = rigidbodyVelocityEntity,

                    weaponEntitys = weaponEntitys,
                    weapons = weapons,
                    weaponTranslations = weaponTranslations,

                    _outs = _outsB.AsParallelWriter(),
                }
                .Schedule(this, inputDeps);


                //
                var endCommandBuffer = endBarrier.CreateCommandBuffer();
                inputDeps.Complete();
                while (_outsA.Count > 0)
                {
                    var _out = _outsA.Dequeue();

                    if (EntityManager.HasComponent<WeaponInstalledState>(_out.weaponEntity))//两个插槽位置一样时会出现一个weaponEntity添加两个WeaponInstalledState的情况
                        continue;
                    if (EntityManager.HasComponent<SlotUsingState>(_out.slotEntity))
                        continue;

                    SetWeaponInstalled(_out, endCommandBuffer);
                }
                _outsA.Dispose();


                //
                while (_outsB.Count > 0)
                {
                    var _out = _outsB.Dequeue();

                    if (EntityManager.HasComponent<WeaponInstalledState>(_out.weaponEntity))
                        continue;
                    if (EntityManager.HasComponent<SlotUsingState>(_out.slotEntity))
                        continue;

                    SetWeaponInstalled(_out, endCommandBuffer);
                }
                _outsB.Dispose();


                //commandBuffer.Playback(EntityManager);
                //endBarrier.AddJobHandleForProducer(inputDeps);
                return inputDeps;
            }

            void SetWeaponInstalled(in FindOut _out, EntityCommandBuffer endCommandBuffer)
            {
                var shipActorOwner = EntityManager.GetComponentData<ActorOwner>(_out.shipEntity);
                EntityManager.AddComponentData(_out.weaponEntity, new WeaponInstalledState { shipActorOwner = shipActorOwner, slotEntity = _out.slotEntity, slot = _out.slot, autoFire = _out.autoFire });
                EntityManager.AddComponentData(_out.slotEntity, new SlotUsingState { });

                EntityManager.AddComponentData(_out.weaponEntity, new OnWeaponInstallMessage { });
                endCommandBuffer.RemoveComponent<OnWeaponInstallMessage>(_out.weaponEntity);
            }
        }

    }

}
