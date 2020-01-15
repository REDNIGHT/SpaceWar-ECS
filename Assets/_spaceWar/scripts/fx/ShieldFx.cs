using System.Collections;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class ShieldFx : MonoBehaviour, IShieldFx
    {
        const float showTime = 0.5f;
        Material material;
        private void Awake()
        {
            material = GetComponentInChildren<Renderer>(true).material;
        }

        public void OnEnable()
        {
            StartCoroutine(fade(true));
        }
        public void OnDestroyFx()
        {
            StartCoroutine(fade(false));
        }


        IEnumerator fade(bool visible)
        {
            var color = material.color;
            var b = 0f;
            var e = 1f;
            if (visible == false)
            {
                b = 1f;
                e = 0f;
            }

            foreach (var t in new TimeEquation().linear.play(showTime))
            {
                color.a = Mathf.Lerp(b, e, t);
                material.color = color;
                yield return this;
            };

            if (visible == false)
            {
                gameObject.SetActive(false);
            }
        }
    }
}