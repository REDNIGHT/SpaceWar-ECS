using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace RN.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleHandler : MonoBehaviour, IUIElement
    {
        public Component handler;
        public string onValueChangedFunction = "";

        public string onUpdateUIFunction { get { return onValueChangedFunction + "_updateUI"; } }


        //
        protected void Awake()
        {
            if (string.IsNullOrEmpty(onValueChangedFunction))
                onValueChangedFunction = "on" + name.Replace("(Clone)", "");
        }

        /*protected void Start()
        { 
            if (handler == null)
                handler = GetComponentInParent<UIPanel>();
        }*/

        public IEnumerator onEnableUI()
        {
            if (handler == null)
                handler = GetComponentInParent<UIPanel>();

            //
            var toggle = GetComponent<Toggle>();

            handler.SendMessage(onUpdateUIFunction, toggle, SendMessageOptions.DontRequireReceiver);

            //
            toggle.onValueChanged.AddListener(onValueChangedInvoke);

            return null;
        }

        public void onDisableUI()
        {
            GetComponent<Toggle>().onValueChanged.RemoveListener(onValueChangedInvoke);
        }

        //
        protected void onValueChangedInvoke(bool value)
        {
            var toggle = GetComponent<Toggle>();
            if (enabled && toggle.enabled)
                handler.SendMessage(onValueChangedFunction, toggle);
        }


        //
        public static void setValueOnly(Toggle t, bool on)
        {
            if (on == t.isOn)
                return;

            var hs = t.GetComponents<ToggleHandler>();
            if (hs.Length == 0)
            {
                if (t.enabled == false)
                    Debug.LogError("tt.enabled == false  t=" + t, t);
                t.enabled = false;
            }
            else
            {
                foreach (var h in hs)
                {
                    if (h.enabled == false)
                        Debug.LogWarning("h.enabled == false  h=" + h, h);
                    h.enabled = false;
                }
            }


            //----
            t.isOn = on;
            //----


            if (hs.Length == 0)
            {
                t.enabled = true;
            }
            else
            {
                foreach (var h in hs)
                    h.enabled = true;
            }
        }

#if UNITY_EDITOR
        [RN._Editor.ButtonInEndArea]
        public void functionCode2CopyBuffer()
        {
            var n = "on" + name;
            if (string.IsNullOrEmpty(onValueChangedFunction) == false)
                n = onValueChangedFunction;

            UnityEditor.EditorGUIUtility.systemCopyBuffer = @"
    protected void " + n + @"_updateUI(Toggle t)
    {
    }
    protected void " + n + @"(Toggle t)
    {
    }
";
        }
#endif
    }

}