
using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShieldOnDestroyServerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            //off
            Entities
                .WithAllReadOnly<Shield, ActorCreator, OnDestroyMessage>()
                .ForEach((ref ActorCreator shipCreator) =>
                {
                    var shipT = EntityManager.GetComponentObject<Transform>(shipCreator.entity);

                    var shieldRootT = shipT.GetChild(ShipSpawner.ShieldRoot_TransformIndex);

                    var entityBehaviour = shieldRootT.GetComponent<EntityBehaviour>();
                    entityBehaviour.Reset();

                    foreach (Transform shieldT in shieldRootT)
                    {
                        shieldT.gameObject.SetActive(false);
                    }


                    var shipWeaponInstalledArray = EntityManager.GetComponentData<ShipWeaponArray>(shipCreator.entity);
                    shipWeaponInstalledArray.shieldEntity = Entity.Null;
                    EntityManager.SetComponentData(shipCreator.entity, shipWeaponInstalledArray);
                });
        }
    }




    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShieldOnDestroyClientSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            //off
            Entities
                .WithAllReadOnly<Shield, Transform, ShieldRoot, OnDestroyMessage>()
                .ForEach((Entity shieldEntity, Transform shieldRootT, ShieldRoot shieldRoot) =>
                {
                    shieldRoot.enabled = false;

                    foreach (Transform shieldT in shieldRootT)
                    {
                        if (shieldT.gameObject.activeSelf)
                        {
                            var fx = shieldT.GetComponent<IShieldFx>();
                            if (fx != null)
                            {
                                fx.OnDestroyFx();
                                return;
                            }

                            shieldT.gameObject.SetActive(false);

                            return;
                        }
                    }
                });
        }
    }
}
