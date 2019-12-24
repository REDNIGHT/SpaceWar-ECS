using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RN.UI
{
    //长按 压力感应(用力按ios)      //都是同一个事件onLongPressedFunction
    //鼠标中键 鼠标右键  //onMouseClickFunction
    public class ClickHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IUIElement
    {
        const float time = 0.65f;
        public void OnPointerDown(PointerEventData eventData)
        {
            StartCoroutine(play(eventData));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pressureTarget != null) pressureTarget.localScale = Vector3.one;

            StopAllCoroutines();

            StartCoroutine(stop());

            //Debug.LogWarning("button=" + eventData.button + "  eventData.pointerId=" + eventData.pointerId, this);
            onMouseClickInvoke(eventData.button);
        }

        public Transform pressureTarget;
        IEnumerator play(PointerEventData eventData)
        {
            var curTime = time;
            while (curTime > 0f)
            {
                curTime -= Time.unscaledDeltaTime;

                if (Input.touchCount > 0)
                {
                    var touch = Input.GetTouch(eventData.pointerId);
                    Debug.LogWarning("touch.pressure=" + touch.pressure + "  eventData.pointerId=" + eventData.pointerId, this);

                    if (touch.pressure > 1f)
                    {
                        if (pressureTarget != null) pressureTarget.localScale = Vector3.one;

                        //onLongPressedInvoke();
                        break;
                    }

                    if (pressureTarget != null) pressureTarget.localScale = Vector3.one * (1f + touch.pressure * 0.25f);
                }

                yield return this;
            }


            onLongPressedInvoke();

            var sb = GetComponent<StateButtonHandler>();
            if (sb != null) sb.enabled = false;

            var bh = GetComponent<ButtonHandler>();
            if (bh != null) bh.enabled = false;

            var th = GetComponent<ToggleHandler>();
            if (th != null) th.enabled = false;
        }
        IEnumerator stop()
        {
            yield return this;

            onDisableUI();
        }


        public Component handler;
        public string onLongPressedFunction = "";
        public string onMouseClickFunction = "";

        protected void Awake()
        {
            if (string.IsNullOrEmpty(onLongPressedFunction))
                onLongPressedFunction = "on_LongPressed" + name.Replace("(Clone)", "") + "";
        }

        public IEnumerator onEnableUI()
        {
            //
            if (handler == null)
                handler = GetComponentInParent<UIPanel>();

            if (pressureTarget == null)
                pressureTarget = transform.GetChild(0);
            if (pressureTarget == null)
                pressureTarget = transform;

            return null;
        }

        public void onDisableUI()
        {
            var sb = GetComponent<StateButtonHandler>();
            if (sb != null) sb.enabled = true;

            var bh = GetComponent<ButtonHandler>();
            if (bh != null) bh.enabled = true;

            var th = GetComponent<ToggleHandler>();
            if (th != null) th.enabled = true;
        }


        protected void onLongPressedInvoke()
        {
            if (enabled)
                handler.SendMessage(onLongPressedFunction, this);
        }

        protected void onMouseClickInvoke(PointerEventData.InputButton button)
        {
            if (enabled)
            {
                if (string.IsNullOrEmpty(onMouseClickFunction) == false)
                    handler.sendMessage(onMouseClickFunction
                        , button
                        /*, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                        , Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
                        , Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)*/
                        , this);
                else if (button == PointerEventData.InputButton.Right)
                    onLongPressedInvoke();
            }
        }


#if UNITY_EDITOR
        [RN._Editor.ButtonInEndArea]
        public void functionCode2CopyBuffer()
        {
            var n = "on_LongPressed" + name;
            if (string.IsNullOrEmpty(onLongPressedFunction) == false)
                n = onLongPressedFunction;

            UnityEditor.EditorGUIUtility.systemCopyBuffer = @"
    protected void " + n + @"(LongPressedHandler h)
    {
    }
";

            n = "on" + name + "MouseClick";
            if (string.IsNullOrEmpty(onMouseClickFunction) == false)
                n = onMouseClickFunction;

            UnityEditor.EditorGUIUtility.systemCopyBuffer += @"
    protected void " + onMouseClickFunction + @"(PointerEventData.InputButton button, /*bool shift, bool control, bool alt,*/ LongPressedHandler h)
    {
    }
";
        }

#endif
    }
}