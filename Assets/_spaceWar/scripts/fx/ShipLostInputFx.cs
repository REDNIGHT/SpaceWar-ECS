using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class ShipLostInputFx : MonoBehaviour, IShipLostInputFx
    {
        public void OnPlayFx(float time)
        {
            var ps = GetComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = time;

            gameObject.SetActive(true);
        }
    }
}