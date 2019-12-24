using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RN.UI
{
    [RequireComponent(typeof(Slider))]
    public class SliderHandler : MonoBehaviour
        , IUIElement
        , IPointerDownHandler, IPointerUpHandler
    //, IBeginDragHandler, IEndDragHandler
    {
        public Component handler;
        public string onValueChangedFunction = "";
        public string onSliderBeginFunction = "";
        public string onSliderEndFunction = "";

        public string onUpdateUIFunction { get { return onValueChangedFunction + "_updateUI"; } }


        public Text value;
        public string format = "{0:f2}";//{0:P0}
        public bool showByDrag = false;


        //
        protected void Awake()
        {
            var n = name.Replace("(Clone)", "");
            if (onValueChangedFunction.Length == 0)
                onValueChangedFunction = "on" + n;
            if (onSliderBeginFunction.Length == 0)
                onSliderBeginFunction = "on" + n + "_begin";
            if (onSliderEndFunction.Length == 0)
                onSliderEndFunction = "on" + n + "_end";

            //
            if (value == null)
                value = transform.parent.find<Text>("value");
            if (showByDrag && value != null)
                value.gameObject.SetActive(false);

            //
            var slider = GetComponent<Slider>();
            if (slider.handleRect == null)
                slider.handleRect = transform.Find("rect") as RectTransform;
            if (slider.handleRect == null)
                Debug.LogError("slider.handleRect == null");
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
            var slider = GetComponent<Slider>();

            handler.SendMessage(onUpdateUIFunction, slider, SendMessageOptions.DontRequireReceiver);

            if (value != null)
                value.text = string.Format(format, slider.value);

            //
            slider.onValueChanged.AddListener(onValueChangedInvoke);

            return null;
        }

        public void onDisableUI()
        {
            //
            GetComponent<Slider>().onValueChanged.RemoveListener(onValueChangedInvoke);
        }

        protected void onValueChangedInvoke(float v)
        {
            var slider = GetComponent<Slider>();
            if (enabled && slider.enabled)
                handler.SendMessage(onValueChangedFunction, slider);

            if (value != null)
                value.text = string.Format(format, v);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var slider = GetComponent<Slider>();
            if (enabled && slider.enabled)
                handler.SendMessage(onSliderBeginFunction, slider, SendMessageOptions.DontRequireReceiver);

            if (showByDrag && value != null)
                value.gameObject.SetActive(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var slider = GetComponent<Slider>();
            if (enabled && slider.enabled)
                handler.SendMessage(onSliderEndFunction, slider, SendMessageOptions.DontRequireReceiver);

            if (showByDrag && value != null)
                value.gameObject.SetActive(false);
        }



        //
        public static void setValueOnly(Slider s, float v)
        {
            //if (v == s.value) //可能需要修改Text 所有把这段注释了
            //    return;


            var hs = s.GetComponents<SliderHandler>();
            foreach (var h in hs)
            {
                if (h.enabled == false)
                    Debug.LogError("h.enabled == false  h=" + h, h);
                h.enabled = false;

                if (h.value != null)
                    h.value.autoSetText(string.Format(h.format, v));
            }


            //----
            s.value = v;
            //----


            foreach (var h in hs)
                h.enabled = true;
        }
        public static void setMinMaxValueOnly(Slider s, float min, float max)
        {
            //if (v == s.value)
            //    return;

            var hs = s.GetComponents<SliderHandler>();
            foreach (var h in hs)
            {
                if (h.enabled == false)
                    Debug.LogError("h.enabled == false  h=" + h, h);
                h.enabled = false;

                //if (h.value != null)
                //    h.value.autoSetText(v.ToString(h.format));
            }


            //----
            s.minValue = min;
            s.maxValue = max;
            //----


            foreach (var h in hs)
                h.enabled = true;
        }




#if UNITY_EDITOR
        [RN._Editor.ButtonInEndArea]
        public void functionCode2CopyBuffer()
        {
            var n = "on" + name;
            if (string.IsNullOrEmpty(onValueChangedFunction) == false)
                n = onValueChangedFunction;

            UnityEditor.EditorGUIUtility.systemCopyBuffer = @"
    protected void " + n + @"_updateUI(Slider s)
    {
    }
    protected void " + n + @"(Slider s)
    {
    }
    protected void " + n + @"_begin(Slider s)
    {
    }
    protected void " + n + @"_end(Slider s)
    {
    }
";
        }
#endif
    }
}