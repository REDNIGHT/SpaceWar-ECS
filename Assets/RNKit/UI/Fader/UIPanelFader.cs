using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RN.UI
{
    public class UIPanelFader : MonoBehaviour, IUIPanelFader
    {
        public float fadeOutTime = 0.5f;
        public float fadeInTime = 0.5f;

        public Equations.Enum fadeOutEquation = Equations.Enum.inCubic;
        public Equations.Enum fadeInEquation = Equations.Enum.outCubic;

        public IEnumerator Out(UIPanel panel)
        {
            var canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();

            foreach (var t in new TimeEquation(fadeOutEquation).playInverseRealtime(fadeOutTime))
            {
                canvasGroup.alpha = t;
                yield return this;
            }
        }

        public IEnumerator In(UIPanel panel)
        {
            var canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();

            foreach (var t in new TimeEquation(fadeInEquation).playRealtime(fadeInTime))
            {
                canvasGroup.alpha = t;
                yield return this;
            }
        }
    }
}