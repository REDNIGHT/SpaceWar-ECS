using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class WeaponFirePrepareFx : MonoBehaviour, IWeaponFirePrepareFx
    {
        float volume = 1f;
        private void Awake()
        {
            var @as = GetComponent<AudioSource>();
            if (@as != null)
            {
                volume = @as.volume;
            }
        }

        public void OnPlayFx(Entity weaponEntity, in WeaponCreator weaponCreator, IActorSpawnerMap actorSpawnerMap, EntityManager entityManager)
        {
            var weaponActor = entityManager.GetComponentData<Actor>(weaponCreator.entity);
            var weaponSpawner = actorSpawnerMap.GetActorSpawner(weaponActor.actorType) as WeaponSpawner;
            Debug.Assert(weaponSpawner != null, $"weaponSpawner != null  {weaponActor.actorType}");

            OnPlayFx(weaponSpawner);

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

        public virtual void OnPlayFx(WeaponSpawner weaponSpawner)
        {
            var ps = GetComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = weaponSpawner.firePrepare;

            gameObject.SetActive(true);


            var @as = GetComponent<AudioSource>();
            if (@as != null && @as.isPlaying)
            {
                @as.volume = volume;
                StartCoroutine(@as.fadeOut(0.5f, weaponSpawner.firePrepare));
            }
        }
    }
}