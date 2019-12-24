using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RN.UI
{
    public class ButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
    {
        protected virtual Transform target
        {
            get
            {
                var button = transform.GetComponent<Button>();
                if (button != null)
                {
                    var icon = transform.Find("icon");
                    if (icon != null)
                        return icon;
                    icon = transform.Find("text");
                    if (icon != null)
                        return icon;
                    icon = transform.Find("Text");
                    if (icon != null)
                        return icon;
                    return transform;
                }

                var toggle = transform.GetComponent<Toggle>();
                if (toggle == null)
                    return null;

                if (toggle.isOn)
                    return toggle.graphic.transform;
                else
                    return toggle.targetGraphic.transform;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //todo... animaiton

            UIManager.singleton.playHoverSound();
        }

        //
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            //
            var _target = target;
            if (_target != null)
                target.localScale = Vector3.one * 1.25f;

            //
            if (GetComponent<AudioSource>() != null)
                GetComponent<AudioSource>().Play();
            else
                UIManager.singleton.playClickSound();

            //
            StopAllCoroutines();
        }

        //
        public void OnPointerUp(PointerEventData eventData)
        {
            var _target = target;
            if (_target != null)
            {
                StartCoroutine(onPointerUpE(eventData, _target));
            }
        }
        static IEnumerator onPointerUpE(PointerEventData eventData, Transform target)
        {
            var b = target.localScale;
            var e = Vector3.one;
            foreach (var t in new TimeEquation().outBack.playRealtime(0.5f))
            {
                target.localScale = Vector3.LerpUnclamped(b, e, t);
                yield return null;
            }


            /*{
                new Tween(target, 0.25f, 0.0f)
                .equation.outBack
                .transform.localScale.to(Vector3.one)
                .playAndReplaceInUI();
            }*/
        }

        //protected void OnDisable() { }

#if UNITY_EDITOR
        static Color buttonBGColor = new Color(0.1137255f, 0.6235294f, 0.7607844f, 1f);
        [RN._Editor.ButtonInEndArea]
        void setIconButton()
        {
            GetComponent<Image>().color = Color.clear;
            GetComponent<Selectable>().targetGraphic = GetComponent<Image>();

            var b = GetComponent<Selectable>();
            var cs = b.colors;
            cs.normalColor = Color.white;
            cs.highlightedColor = Color.white;
            cs.pressedColor = Color.white;
            cs.disabledColor = Color.white;
            b.colors = cs;

            UnityEditor.EditorUtility.SetDirty(b);
        }
        [RN._Editor.ButtonInEndArea1]
        void setTextButton()
        {
            GetComponent<Image>().color = Color.white;
            GetComponent<Selectable>().targetGraphic = GetComponent<Image>();
            transform.find<Text>("text").color = Color.white;

            var b = GetComponent<Selectable>();
            var cs = b.colors;
            cs.normalColor = buttonBGColor;

            var c = buttonBGColor;
            c.a = 0.75f;
            cs.highlightedColor = c;

            c.a = 0.5f;
            cs.pressedColor = c;

            cs.disabledColor = Color.black;
            b.colors = cs;


            UnityEditor.EditorUtility.SetDirty(b);
        }
        [RN._Editor.ButtonInEndArea1]
        void setTextButtonNoBG()
        {
            GetComponent<Image>().color = Color.white;
            GetComponent<Selectable>().targetGraphic = GetComponent<Image>();
            transform.find<Text>("text").color = Color.white;

            var b = GetComponent<Selectable>();
            var cs = b.colors;
            cs.normalColor = Color.clear;
            var highlightedColor = buttonBGColor;
            highlightedColor.a = 0.25f;
            cs.highlightedColor = highlightedColor;
            cs.pressedColor = buttonBGColor;
            cs.disabledColor = Color.gray;
            b.colors = cs;


            UnityEditor.EditorUtility.SetDirty(b);
        }
        /*[RN._Editor.ButtonInEndArea2]
        void setToggle()
        {
            GetComponent<Image>().color = Color.clear;
            GetComponent<Selectable>().targetGraphic = transform.find<Graphic>("off");
            GetComponent<Toggle>().graphic = transform.find<Graphic>("on");

            var b = GetComponent<Selectable>();
            var cs = b.colors;
            cs.normalColor = buttonBGColor;
            cs.highlightedColor = buttonBGColor;
            cs.pressedColor = buttonBGColor;
            cs.disabledColor = Color.gray;
            b.colors = cs;

            GetComponent<Selectable>().targetGraphic.color = new Color(1f, 1f, 1f, 0.25f);
            UnityEditor.EditorUtility.SetDirty(b);
        }*/
#endif
    }
}
