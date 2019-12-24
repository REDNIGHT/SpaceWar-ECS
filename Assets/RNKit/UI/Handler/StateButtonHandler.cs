using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace RN.UI
{
    [RequireComponent(typeof(Button))]
    public class StateButtonHandler : MonoBehaviour, IUIElement
    {
        public Component handler;
        public string onClickFunction = "";

        public string onUpdateUIFunction { get { return onClickFunction + "_updateUI"; } }

        [SerializeField]
        protected int _state = 0;

        public List<Graphic> states;
        public int stateCount { get { return states.Count; } }

        protected void Awake()
        {
            if (onClickFunction.Length == 0)
                onClickFunction = "on" + name.Replace("(Clone)", "");

            //
            if (states.Count == 0)
            {
                foreach (Transform c in transform)
                {
                    var g = c.GetComponent<Graphic>();
                    if (g != null && g.gameObject.activeSelf)
                        states.Add(g);
                }
            }

            if (states.Count <= 0)
                Debug.LogError("states.Count <= 0", this);

            //
            setStateIgnoreMessage(_state);
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

            handler.SendMessage(onUpdateUIFunction, this, SendMessageOptions.DontRequireReceiver);

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
            if (enabled)
                ++state;
        }

        public int state
        {
            get { return _state; }
            set
            {
                //
                if (value == _state)
                    return;

                setStateIgnoreMessage(value);

                var button = GetComponent<Button>();
                if (enabled && button.enabled)
                    handler.SendMessage(onClickFunction, this);
            }
        }

        public void setStateIgnoreMessage(int value)
        {
            _state = value;
            if (_state >= stateCount)
                _state = 0;

            //GetComponent<Button>().targetGraphic = states[_state];

            //
            states[_state].gameObject.SetActive(true);
            for (var i = 0; i < states.Count; ++i)
                if (i != _state)
                    states[i].gameObject.SetActive(false);
        }




#if UNITY_EDITOR
        [RN._Editor.ButtonInEndArea]
        public void functionCode2CopyBuffer()
        {
            var n = "on" + name;
            if (string.IsNullOrEmpty(onClickFunction) == false)
                n = onClickFunction;

            UnityEditor.EditorGUIUtility.systemCopyBuffer = @"
    protected void " + n + @"_updateUI(StateButtonHandler sb)
    {
    }
    protected void " + n + @"(StateButtonHandler sb)
    {
    }
";
        }
#endif
    }
}