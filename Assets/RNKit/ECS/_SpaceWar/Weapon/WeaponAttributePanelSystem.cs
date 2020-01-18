using Unity.Collections;
using Unity.Entities;
using UnityEngine;

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
                                    //weaponAttributePanel.autoPlay(cameraData);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
