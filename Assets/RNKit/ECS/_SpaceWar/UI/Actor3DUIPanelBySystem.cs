using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public interface IActor3DUIPanel
    {
        void show(float showTime, in CameraDataSingleton cameraData);
    }

    public class Actor3DUIPanelBySystem : ComponentSystem
    {
        Rewired.Mouse mouseInput;

        protected override void OnCreate()
        {
            var input = Rewired.ReInput.players.GetPlayer(InputPlayer.Player0);
            mouseInput = input.controllers.Mouse;
        }

        public int layerMask;
        public float radius = 1f;
        RaycastHit[] results = new RaycastHit[16];
        public float showTime = 1f;
        protected override void OnUpdate()
        {
            var ray = Camera.main.ScreenPointToRay(mouseInput.screenPosition);

            int numFound = Physics.SphereCastNonAlloc(ray, radius, results, 64f, layerMask);
            if (numFound > 0)
            {
                var cameraData = GetSingleton<CameraDataSingleton>();

                results
                    .Take(numFound)
                    .Select(x => x.rigidbody)
                    .Where(x => x != null)
                    .Distinct()

                    .SelectMany(x => x.GetComponentsInChildren<IActor3DUIPanel>())
                    .Where(x => x != null)
                    .forEach(x => x.show(showTime, cameraData));
            }
        }
    }
}
