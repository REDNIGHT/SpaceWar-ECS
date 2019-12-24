using UnityEngine;
using RN.UI;

namespace RN.Network.SpaceWar
{
    public class SpacePanel : SubPanel
    {
        void Update()
        {
            if (visible && fading == false && Input.anyKeyDown)
            {
                inNextPanel();
            }
        }
    }
}