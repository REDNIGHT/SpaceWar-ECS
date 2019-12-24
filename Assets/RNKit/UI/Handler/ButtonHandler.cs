using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace RN.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonHandler : MonoBehaviour, IUIElement
    {
        public Component handler;
        public string onClickFunction = "";

        public string onUpdateUIFunction { get { return onClickFunction + "_updateUI"; } }

        protected void Awake()
        {
            if (onClickFunction.Length == 0)
                onClickFunction = "on" + name.Replace("(Clone)", "");
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
            var button = GetComponent<Button>();

            handler.SendMessage(onUpdateUIFunction, button, SendMessageOptions.DontRequireReceiver);

            //
            button.onClick.AddListener(onClickInvoke);

            return null;
        }

        public void onDisableUI()
        {
            GetComponent<Button>().onClick.RemoveListener(onClickInvoke);
        }

        protected void onClickInvoke()
        {
            handler.SendMessage(onClickFunction, GetComponent<Button>());
        }





#if UNITY_EDITOR
        [RN._Editor.ButtonInEndArea]
        public void functionCode2CopyBuffer()
        {
            var n = "on" + name;
            if (string.IsNullOrEmpty(onClickFunction) == false)
                n = onClickFunction;

            UnityEditor.EditorGUIUtility.systemCopyBuffer = @"
    protected void " + n + @"_updateUI(Button b)
    {
    }
    protected void " + n + @"(Button b)
    {
    }
";
        }
#endif
    }

}