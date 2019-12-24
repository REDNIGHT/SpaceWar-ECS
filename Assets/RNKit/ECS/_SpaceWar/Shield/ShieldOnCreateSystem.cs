using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShieldOnCreateServerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            //on
            Entities
                .WithAll<ActorLifetime, ActorAttribute3<_HP>, Translation, Shield>().WithAllReadOnly<ActorCreator, OnCreateMessage>()
                .ForEach((Entity shieldEntity, ref ActorCreator shipCreator, ref ActorLifetime actorLifetime, ref ActorAttribute3<_HP> _hp, ref Translation translation, ref Shield shield) =>
                {
                    //
                    var shipT = EntityManager.GetComponentObject<Transform>(shipCreator.entity);
                    var _ShieldLevel = EntityManager.GetComponentData<ActorAttribute1<_ShieldLevel>>(shipCreator.entity);
                    var shieldLevel = (short)(_ShieldLevel.value - 1);
                    if (shieldLevel < 0)
                    {
                        Debug.LogError($"shieldLevel < 0  shipT={shipT}", shipT);
                        return;
                    }

                    var shieldRootT = shipT.GetChild(ShipSpawner.ShieldRoot_TransformIndex);
                    EntityBehaviour._initComponent(shieldRootT, shieldEntity, EntityManager);
                    EntityManager.AddComponentObject(shieldEntity, shieldRootT);
                    EntityManager.AddComponentObject(shieldEntity, shieldRootT.GetComponent<Rigidbody>());


                    var shieldT = shieldRootT.GetChild(shieldLevel);
                    shieldT.gameObject.SetActive(true);



                    //
                    var shipShieldInfos = EntityManager.GetComponentData<ShipShields>(shipCreator.entity);
                    var shieldInfo = shipShieldInfos.get(shieldLevel);
                    actorLifetime = new ActorLifetime { lifetime = shieldInfo.lifetime, value = shieldInfo.lifetime };
                    _hp = new ActorAttribute3<_HP> { max = shieldInfo.hp, regain = 0f, value = shieldInfo.hp };

                    translation = new Translation { Value = shieldRootT.position };
                    //rotation = new Rotation { Value = shieldRootT.rotation };

                    shield = new Shield { curLevel = shieldLevel };



                    //
                    var shipWeaponInstalledArray = EntityManager.GetComponentData<ShipWeaponArray>(shipCreator.entity);
                    shipWeaponInstalledArray.shieldEntity = shieldEntity;
                    EntityManager.SetComponentData(shipCreator.entity, shipWeaponInstalledArray);
                });
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShieldOnCreateClientSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            //on
            Entities
                .WithAllReadOnly<Shield, ActorCreator, OnCreateMessage>()
                .ForEach((Entity shieldEntity, ref ActorCreator shipCreator, ref Shield shield) =>
                {
                    var shipT = EntityManager.GetComponentObject<Transform>(shipCreator.entity);

                    //
                    var shieldRootT = shipT.GetChild(ShipSpawner.ShieldRoot_TransformIndex);
                    foreach (Transform l in shieldRootT)
                        l.gameObject.SetActive(false);
                    shieldRootT.GetChild(shield.curLevel).gameObject.SetActive(true);

                    //
                    EntityManager.AddComponentObject(shieldEntity, shieldRootT);

                    //
                    var shieldRoot = shieldRootT.GetComponent<ShieldRoot>();
                    shieldRoot.enabled = true;
                    EntityManager.AddComponentObject(shieldEntity, shieldRoot);
                });
        }
    }
}
