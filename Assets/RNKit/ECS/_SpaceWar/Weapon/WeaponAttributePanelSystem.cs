using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    public class WeaponAttributePanelClientSystem : ComponentSystem
    {
        ActorSyncCreateClientSystem actorClientSystem;

        Rewired.Player input;
        protected override void OnCreate()
        {
            actorClientSystem = World.GetExistingSystem<ActorSyncCreateClientSystem>();
            input = Rewired.ReInput.players.GetPlayer(InputPlayer.Player0);
        }

        protected override void OnUpdate()
        {
            //
            using (var actorDatas1List = Entities
                .WithAllReadOnly<ActorDatas1Buffer>()
                .WithNone<NetworkDisconnectedMessage>()
                .AllBufferElementToList<ActorDatas1Buffer>(Allocator.Temp))
            {
                if (actorDatas1List.Length > 0)
                {
                    var cameraData = GetSingleton<CameraDataSingleton>();

                    for (int i = 0; i < actorDatas1List.Length; ++i)
                    {
                        var actorDatas1 = actorDatas1List[i];
                        if (actorDatas1.synDataType == (sbyte)ActorSynDataTypes.WeaponAttribute)
                        {
                            if (actorClientSystem.actorEntityFromActorId.TryGetValue(actorDatas1.actorId, out Entity actorEntity))
                            {
                                var transform = EntityManager.GetComponentObject<Transform>(actorEntity);

                                var uiT = transform.GetChild(WeaponSpawner.UI_TransformIndex);

                                if (uiT.childCount > 0)
                                {
                                    var weaponAttributePanel = uiT.GetChild(0).GetComponent<WeaponAttributePanel>();
                                    weaponAttributePanel.setAttributes(actorDatas1.shortValueA, actorDatas1.shortValueB);
                                    weaponAttributePanel.autoPlay(cameraData);
                                }
                            }
                        }
                    }
                }
            }



            //
            /*if (input.GetButtonDown(PlayerInputActions.Shift))
            {
                foreach (var weaponAttributePanel in getWeaponAttributePanels())
                    weaponAttributePanel.begin();
            }
            else if (input.GetButton(PlayerInputActions.Shift))
            {
                var cameraData = GetSingleton<CameraDataSingleton>();
                foreach (var weaponAttributePanel in getWeaponAttributePanels())
                    weaponAttributePanel.update(cameraData);
            }
            else if (input.GetButtonUp(PlayerInputActions.Shift))
            {
                foreach (var weaponAttributePanel in getWeaponAttributePanels())
                    weaponAttributePanel.end();
            }



            //
            Entities
                .WithAllReadOnly<OnWeaponUninstallMessage, WeaponInstalledState, Weapon, Transform>()
                .ForEach((Transform weaponT) =>
                {
                    var uiT = weaponT.GetChild(WeaponSpawner.UI_TransformIndex);

                    if (uiT.childCount > 0)
                    {
                        var weaponAttributePanel = uiT.GetChild(0).GetComponent<WeaponAttributePanel>();
                        if(weaponAttributePanel.visible)
                            weaponAttributePanel.end();
                    }
                });*/
        }


        IEnumerable<WeaponAttributePanel> getWeaponAttributePanels()
        {
            //
            var myPlayerSingleton = GetSingleton<MyPlayerSingleton>();
            var myPlayerEntity = myPlayerSingleton.playerEntity;
            if (myPlayerEntity == Entity.Null)
                yield break;
            if (EntityManager.HasComponent<PlayerActorArray>(myPlayerEntity) == false)
                yield break;
            var actors = EntityManager.GetComponentData<PlayerActorArray>(myPlayerEntity);
            if (actors.shipEntity == Entity.Null)
                yield break;


            var weapons = EntityManager.GetComponentData<ShipWeaponArray>(actors.shipEntity);

            //
            for (var i = 0; i < ShipWeaponArray.WeaponMaxCount; ++i)
            {
                var weaponEntitiy = weapons.GetWeaponEntity(i);
                if (weaponEntitiy != Entity.Null)
                {
                    var weaponT = EntityManager.GetComponentObject<Transform>(weaponEntitiy);

                    var uiT = weaponT.GetChild(WeaponSpawner.UI_TransformIndex);

                    if (uiT.childCount > 0)
                    {
                        yield return uiT.GetChild(0).GetComponent<WeaponAttributePanel>();
                    }
                }
            }
        }
    }
}
