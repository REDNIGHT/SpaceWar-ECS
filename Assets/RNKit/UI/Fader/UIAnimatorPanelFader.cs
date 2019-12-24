using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RN.UI
{
    public class UIAnimatorPanelFader : MonoBehaviour, IUIPanelFader
    {
        public float fadeOutTime = 0.5f;
        public float fadeInTime = 0.5f;
        public IEnumerator Out(UIPanel panel)
        {
            var a = panel.GetComponent<Animator>();
            a.Play("fadeout");

            yield return new WaitForSecondsRealtime(fadeOutTime);
        }

        public IEnumerator In(UIPanel panel)
        {
            var a = panel.GetComponent<Animator>();
            a.Play("fadein");

            yield return new WaitForSecondsRealtime(fadeInTime);
        }
    }
}
