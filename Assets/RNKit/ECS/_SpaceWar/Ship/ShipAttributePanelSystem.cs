using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipAttributePanelClientSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entity myActorEntity = default;
            var myPlayerSingleton = GetSingleton<MyPlayerSingleton>();
            if (myPlayerSingleton.playerEntity != default)
            {
                myActorEntity = EntityManager.GetComponentData<PlayerActorArray>(myPlayerSingleton.playerEntity).mainActorEntity;
            }



            Entities
                .WithAllReadOnly<Ship, OnCreateMessage, Transform>()
                .WithNone<Battery>()
                .ForEach((Entity actorEntity, Transform actorTransform) =>
                {
                    var ui_AttributePanelT = actorTransform
                        .GetChild(ShipSpawner.UI_TransformIndex)
                        .GetChild(ShipSpawner.__UI_AttributePanel_TransformIndex);

                    ui_AttributePanelT.gameObject.SetActive(myActorEntity == actorEntity);

                    if (myActorEntity == actorEntity)
                    {
                        EntityManager.AddComponentObject(actorEntity, ui_AttributePanelT.GetComponent<ShipAttributePanel>());
                    }
                });

            Entities
                .WithAllReadOnly<ShipAttributePanel, Ship, ActorAttribute3<_HP>, ActorAttribute3<_Power>>()
                .WithNone<OnDestroyMessage>()
                .ForEach((ShipAttributePanel actorAttributeUI, ref ActorAttribute3<_HP> hp, ref ActorAttribute3<_Power> power) =>
                {
                    actorAttributeUI.setAttributes(hp, power);
                });
        }
    }
}
