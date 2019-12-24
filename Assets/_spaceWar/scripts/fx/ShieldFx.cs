using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class ShieldFx : MonoBehaviour, IShieldFx
    {
        public void OnDestroyFx()
        {
            gameObject.SetActive(false);
        }
    }
}