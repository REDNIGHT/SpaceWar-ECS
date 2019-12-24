using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class WeaponFireFxClientSystem : ComponentSystem
    {
        IActorSpawnerMap actorSpawnerMap;

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
                .WithAllReadOnly<WeaponFireFx, WeaponCreator, OnCreateMessage>()
                .ForEach((Entity bulletEntity, ref WeaponCreator weaponCreator) =>
                {
                    var weaponT = EntityManager.GetComponentObject<Transform>(weaponCreator.entity);

                    var fireFxT = weaponT.GetChild(WeaponSpawner.FireFx_TransformIndex);

                    //fireFxT.rotation = rotation.Value;

                    var fx = fireFxT.GetComponent<IWeaponFireFx>();
                    if (fx != null)
                    {
                        fx.OnPlayFx(bulletEntity, weaponCreator, actorSpawnerMap, EntityManager);
                    }
                    else
                    {
                        fireFxT.gameObject.SetActive(true);
                    }
                });
        }
    }
}
