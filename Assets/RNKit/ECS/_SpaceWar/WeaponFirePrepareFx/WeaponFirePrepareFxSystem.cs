using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class WeaponFirePrepareFxServerSystem : JobComponentSystem
    {
        MiddleCommandBufferSystem middleBarrier;

        protected void OnInit(Transform root)
        {
            middleBarrier = World.GetExistingSystem<MiddleCommandBufferSystem>();
        }

        //[BurstCompile]
        [RequireComponentTag(typeof(OnWeaponControlFirePrepareMessage))]
        struct FirePrepareFxJob : IJobForEachWithEntity<Weapon, Translation, WeaponInstalledState>
        {
            [ReadOnly] public ComponentDataFromEntity<WeaponShield> shieldWeaponFromEntity;
            public EntityCommandBuffer.Concurrent middleCommandBuffer;
            public void Execute(Entity weaponEntity, int index, [ReadOnly]ref Weapon weapon, [ReadOnly]ref Translation translation, [ReadOnly]ref WeaponInstalledState weaponInstalledState)
            {
                byte shieldLevel = 0;
                if (weapon.type == WeaponType.Shield)
                {
                    var shieldWeapon = shieldWeaponFromEntity[weaponEntity];
                    shieldLevel = shieldWeapon.level;
                }

                WeaponFirePrepareFxSpawner.createInServer(middleCommandBuffer, index, translation, weaponEntity, weaponInstalledState.shipEntity, shieldLevel);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new FirePrepareFxJob
            {
                shieldWeaponFromEntity = GetComponentDataFromEntity<WeaponShield>(true),
                middleCommandBuffer = middleBarrier.CreateCommandBuffer().ToConcurrent(),

            }.Schedule(this, inputDeps);
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class WeaponFirePrepareFxClientSystem : ComponentSystem
    {
        IActorSpawnerMap actorSpawnerMap;
        ActorSyncCreateClientSystem actorClientSystem;

        protected void OnInit(Transform root)
        {
            actorSpawnerMap = root.GetComponentInChildren<IActorSpawnerMap>();
            Debug.Assert(actorSpawnerMap != null, $"actorSpawnerMap != null  root={root}", root);
        }
        protected override void OnDestroy()
        {
            actorSpawnerMap = null;
        }

        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<WeaponFirePrepareFx, WeaponCreator, OnCreateMessage>()
                .ForEach((Entity weaponFirePrepareEntity, ref WeaponCreator weaponCreator) =>
                {
                    var weaponT = EntityManager.GetComponentObject<Transform>(weaponCreator.entity);

                    var firePrepareFxT = weaponT.GetChild(WeaponSpawner.FirePrepareFx_TransformIndex);

                    //fireFxT.rotation = rotation.Value;

                    var fx = firePrepareFxT.GetComponent<IWeaponFirePrepareFx>();
                    if (fx != null)
                    {
                        fx.OnPlayFx(weaponFirePrepareEntity, weaponCreator, actorSpawnerMap, EntityManager);
                    }
                    else
                    {
                        firePrepareFxT.gameObject.SetActive(true);
                    }
                });
        }
    }
}
