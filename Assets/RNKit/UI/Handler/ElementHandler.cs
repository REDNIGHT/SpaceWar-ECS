using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace RN.UI
{
    public class ElementHandler : MonoBehaviour, IUIElement
    {
        public string onUpdateUIFunction { get { return "on_" + name + "_updateUI"; } }

        public IEnumerator onEnableUI()
        {
            var handler = GetComponentInParent<UIPanel>();

            handler.SendMessage(onUpdateUIFunction, this, SendMessageOptions.DontRequireReceiver);

            return null;
        }

        public void onDisableUI() { }



#if UNITY_EDITOR
        [RN._Editor.ButtonInEndArea]
        public void functionCode2CopyBuffer()
        {
            UnityEditor.EditorGUIUtility.systemCopyBuffer = @"
    protected void " + onUpdateUIFunction + @"(ElementHandler e)
    {
    }";
        }
#endif
    }

}