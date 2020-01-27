
using Unity.Mathematics;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class MouseFx : MonoBehaviour, IMouseFx
    {
        ParticleSystem ps;
        AudioSource @as;
        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            @as = GetComponent<AudioSource>();
        }
        public void OnPlayFx(in float3 position)
        {
            ps.transform.position = position;
            ps.Emit(1);
            ps.gameObject.SetActive(true);

            @as.Play();
        }
    }
}
