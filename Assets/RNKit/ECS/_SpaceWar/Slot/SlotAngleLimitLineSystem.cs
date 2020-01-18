using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    public class SlotAngleLimitLineSystem : ComponentSystem
    {
        Rewired.Player input;
        protected override void OnCreate()
        {
            input = Rewired.ReInput.players.GetPlayer(InputPlayer.Player0);
        }


        SlotAngleLimitLines getShipSlotList()
        {
            //
            var myPlayerSingleton = GetSingleton<MyPlayerSingleton>();
            var myPlayerEntity = myPlayerSingleton.playerEntity;
            if (myPlayerEntity == Entity.Null)
                return null;
            if (EntityManager.HasComponent<PlayerActorArray>(myPlayerEntity) == false)
                return null;
            var actors = EntityManager.GetComponentData<PlayerActorArray>(myPlayerEntity);
            if (actors.shipEntity == Entity.Null)
                return null;


            SlotAngleLimitLines shipSlotList;
            if (EntityManager.HasComponent<SlotAngleLimitLines>(actors.shipEntity) == false)
            {
                var shipT = EntityManager.GetComponentObject<Transform>(actors.shipEntity);
                shipSlotList = shipT.gameObject.AddComponent<SlotAngleLimitLines>();
                EntityManager.AddComponentObject(actors.shipEntity, shipSlotList);
            }
            else
            {
                shipSlotList = EntityManager.GetComponentObject<SlotAngleLimitLines>(actors.shipEntity);
            }

            return shipSlotList;
        }

        void begin()
        {
            getShipSlotList()?.begin();
        }

        void end()
        {
            getShipSlotList()?.end();
        }

        void update()
        {
            getShipSlotList()?.update(GetSingleton<MouseDataSingleton>().point);
        }

        protected override void OnUpdate()
        {
            if (input.GetButtonDown(PlayerInputActions.Shift))
            {
                begin();
            }
            else if (input.GetButtonUp(PlayerInputActions.Shift))
            {
                end();
            }

            if (input.GetButton(PlayerInputActions.Shift))
            {
                update();
            }
        }
    }
}
