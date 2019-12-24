using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public abstract class WeaponFireFx<T> : MonoBehaviour, IWeaponFireFx where T : ActorSpawner
    {
        public void OnPlayFx(Entity bulletEntity, in WeaponCreator weaponCreator, IActorSpawnerMap actorSpawnerMap, EntityManager entityManager)
        {
        }

        public abstract void OnPlayFx(WeaponSpawner weaponSpawner, T fireActorSpawner, bool mainSlot);
    }
}