using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorAttributePanelSystem : ComponentSystem
    {
        public ActorAttributePanel shipAttributePanelPrefab;

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
                .ForEach((Entity actorEntity, Transform actorTransform) =>
                {
                    if (myActorEntity == actorEntity)
                    {
                        var ui = GameObject.Instantiate(shipAttributePanelPrefab);
                        ui.transform.parent = actorTransform;
                        ui.transform.localPosition = Vector3.zero;

                        EntityManager.AddComponentObject(actorEntity, ui);
                    }
                });

            var cameraControllerSingleton = GetSingleton<CameraControllerSingleton>();
            Entities
                .WithAllReadOnly<ActorAttributePanel, Ship, ActorAttribute3<_HP>, ActorAttribute3<_Power>>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Entity actorEntity, ActorAttributePanel actorAttributeUI, ref ActorAttribute3<_HP> hp, ref ActorAttribute3<_Power> power) =>
                {
                    actorAttributeUI.setAttributes(hp, power);
                    actorAttributeUI.transform.rotation = cameraControllerSingleton.targetRotation;
                });
        }
    }
}
