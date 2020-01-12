using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class ShipLostInputFx : MonoBehaviour, IShipLostInputFx
    {
        private void Awake()
        {
            foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
            {
                ps.gameObject.SetActive(false);
            }
        }
        public void OnPlayFx(float time)
        {
            foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                main.duration = time;

                ps.gameObject.SetActive(true);
            }
        }
    }
}