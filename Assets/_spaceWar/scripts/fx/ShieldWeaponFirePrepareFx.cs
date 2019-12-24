using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class ShieldWeaponFirePrepareFx : MonoBehaviour, IWeaponFirePrepareFx
    {
        public void OnPlayFx(Entity weaponFirePrepareEntity, in WeaponCreator weaponCreator, IActorSpawnerMap actorSpawnerMap, EntityManager entityManager)
        {
            var actorCreator = entityManager.GetComponentData<ActorCreator>(weaponFirePrepareEntity);
            var weaponFirePrepareFx = entityManager.GetComponentData<SpaceWar.WeaponFirePrepareFx>(weaponFirePrepareEntity);

            var shipActor = entityManager.GetComponentData<Actor>(actorCreator.entity);
            var shipSpawner = actorSpawnerMap.GetActorSpawner(shipActor.actorType) as ShipSpawner;
            Debug.Assert(shipSpawner != null, $"shipSpawner != null  {shipActor.actorType}");

            OnPlayFx(shipSpawner, weaponFirePrepareFx.level);

            /*//todo... 在WeaponSyncInstalledStateServerSystem里同步weaponInstalledState到客户端
            //var weaponInstalledState = EntityManager.GetComponentData<WeaponInstalledState>(weaponCreator.entity);
            //var fireActorType = weaponInstalledState.mainSlot ? weaponSpawner.fireActorTypeByMainSlot : weaponSpawner.fireActorType;
            //var mainSlot = weaponInstalledState.mainSlot;
            var fireActorType = weaponSpawner.fireActorType;
            var mainSlot = false;


            var fireActorSpawner = actorSpawnerMap.GetActorSpawner((short)fireActorType) as ActorSpawner;
            Debug.Assert(fireActorSpawner != null, $"fireActorSpawner != null  {fireActorSpawner.actorType}");

            OnPlayFx(weaponSpawner, fireActorSpawner, mainSlot);*/
        }

        public virtual void OnPlayFx(ShipSpawner shipSpawner, int level)
        {
            var ps = GetComponent<ParticleSystem>();
            var main = ps.main;
            if (level == 0)
                main.duration = shipSpawner.shield0.prepare;
            else if (level == 1)
                main.duration = shipSpawner.shield1.prepare;
            else if (level == 2)
                main.duration = shipSpawner.shield2.prepare;
            else
            {
                Debug.LogError($"level == {level}", this);
            }

            gameObject.SetActive(true);
        }
    }
}