using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RN.UI
{
    [RequireComponent(typeof(InputField))]
    public class InputFieldHandler : MonoBehaviour, IUIElement
    //, IPointerDownHandler
    {
        public Component handler;
        //public string onBeginEditFunction = "";
        public string onEndEditFunction = "";
        public string onValueChangeFunction = "";

        public string onUpdateUIFunction { get { return onEndEditFunction.Replace("End", "") + "_updateUI"; } }

        protected void Awake()
        {
            if (onEndEditFunction.Length == 0)
                onEndEditFunction = "on" + name.Replace("(Clone)", "") + "End";
            if (onValueChangeFunction.Length == 0)
                onValueChangeFunction = "on" + name.Replace("(Clone)", "") + "Change";
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


            var inputField = GetComponent<InputField>();

            handler.SendMessage(onUpdateUIFunction, inputField, SendMessageOptions.DontRequireReceiver);

            //
            inputField.onEndEdit.AddListener(onEndEditInvoke);

            if (onValueChangeFunction.Length > 0)
                inputField.onValueChanged.AddListener(onValueChangeInvoke);

            return null;
        }

        public void onDisableUI()
        {
            var inputField = GetComponent<InputField>();

            inputField.onEndEdit.RemoveListener(onEndEditInvoke);

            if (onValueChangeFunction.Length > 0)
                inputField.onValueChanged.RemoveListener(onValueChangeInvoke);
        }


        //
        protected void onValueChangeInvoke(string value)
        {
            if (enabled && GetComponent<InputField>().enabled)
                handler.SendMessage(onValueChangeFunction, GetComponent<InputField>(), SendMessageOptions.DontRequireReceiver);
        }
        /*public void OnPointerDown(PointerEventData eventData)
        {
            if (enabled && GetComponent<InputField>().enabled && string.IsNullOrEmpty(onBeginEditFunction) == false)
                handler.SendMessage(onBeginEditFunction, GetComponent<InputField>());
        }*/
        protected void onEndEditInvoke(string value)
        {
            if (enabled && GetComponent<InputField>().enabled)
                handler.SendMessage(onEndEditFunction, GetComponent<InputField>());
        }



#if UNITY_EDITOR
        [RN._Editor.ButtonInEndArea]
        public void functionCode2CopyBuffer()
        {
            var n = "on" + name + "End";
            if (string.IsNullOrEmpty(onEndEditFunction) == false)
                n = onEndEditFunction;

            var code = @"
    protected void on" + name + @"_updateUI(InputField inputField)
    {
    }
    protected void " + n + @"(InputField inputField)
    {
    }";

            n = "on" + name + "Change";
            if (string.IsNullOrEmpty(onValueChangeFunction) == false)
                n = onValueChangeFunction;
            code += @"
    protected void " + n + @"(InputField inputField)
    {
    }
";

            UnityEditor.EditorGUIUtility.systemCopyBuffer = code;
        }
#endif
    }
}