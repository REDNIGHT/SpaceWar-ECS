using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace RN.UI
{
    public sealed partial class UIPanel : MonoBehaviour//, IVisible
    {
        /// <summary>
        /// 可以多开界面的参数
        /// 0是主channel 界面会全屏显示
        /// 1~16 是左 中 右 上 下 左上 左下 右上 右下 ...
        /// 16以上是newChannel生成的
        /// 也可以不按上面的要求来做 自己定义好channel的作用就可以了
        /// </summary>
        public bool newChannel = false;
        public const int ChannelBeginNum = 16;
        public int channel = 0;


        //[SerializeField]
        bool _visible = false;


        //
        public enum VisibleStateInStart
        {
            Show,
            Hide,
            FadeIn,
        }
        public VisibleStateInStart visibleStateInAwake = VisibleStateInStart.Hide;

        /// <summary>
        /// 切换场景时自动删除
        /// </summary>
        public bool autoDestroy = false;
        //public int siblingIndex = 0;

        public enum Position2Zero
        {
            PositionXY_2Zero,
            PositionX_2Zero,
            PositionY_2Zero,
            None,
        }
        public Position2Zero position2Zero;

        //
        public RectTransform view { get { return transform.Find("view") as RectTransform; } }
        public RectTransform getView(string viewName) { return transform.Find(viewName) as RectTransform; }

        //
        void Awake()
        {
            //
            if (autoDestroy)
                SceneManager.sceneUnloaded += onSceneUnloaded;


            //
            var rectT = transform as RectTransform;

            //
            /*if (rectT.parent != UIManager.singletonT)
            {
                if (UIManager.singletonT.Find(name) != null)
                    return;

                rectT.SetParent(UIManager.singletonT, false);
                rectT.SetSiblingIndex(siblingIndex);
            }*/


            //
            var anchoredPosition = rectT.anchoredPosition;
            if (position2Zero == Position2Zero.PositionXY_2Zero)
            {
                anchoredPosition = default;
            }
            if (position2Zero == Position2Zero.PositionX_2Zero)
            {
                anchoredPosition.x = 0f;
            }
            else if (position2Zero == Position2Zero.PositionY_2Zero)
            {
                anchoredPosition.y = 0f;
            }
            rectT.anchoredPosition = anchoredPosition;


            //
            if (GetComponent<UIViewResourceLoader>() == null)
                SendMessage("onViewLoadComplete", SendMessageOptions.DontRequireReceiver);
        }

        IEnumerator Start()
        {
            //
            if (visibleStateInAwake == VisibleStateInStart.Show)
            {
                visible = true;
            }
            else if (visibleStateInAwake == VisibleStateInStart.Hide)
            {
                panelVisible(_visible, true);
            }
            else if (visibleStateInAwake == VisibleStateInStart.FadeIn)
            {
                yield return this;//加载资源完成后 会有点卡 这里延迟2帧
                yield return this;
                while (fading)//切换场景时 会出现界面正在fading
                    yield return this;
                yield return this;


                if (UIManager.singleton.hasCurPanel(channel))
                    yield return UIManager.singleton.fadeOut(channel);

                yield return In();
            }
        }

        void onSceneUnloaded(Scene _)
        {
            SceneManager.sceneUnloaded -= onSceneUnloaded;

            this.destroyGO();

            if (UIManager.singleton.getCurPanel(channel) == this)
                UIManager.singleton.removeCurPanel(this);
        }

        public bool visible
        {
            get { return _visible; }
            set
            {
                //
                if (_visible == value)
                {
                    Debug.LogError(this + ".vVisible == value  value=" + value, this);
                    return;
                }

                _visible = value;

                //
                panelVisible(_visible);
            }
        }

        void panelVisible(bool v, bool inStart = false)
        {
            if (v)
            {
                viewVisible(v);

                enableUI();

                SendMessage("onPanelVisible", v, SendMessageOptions.DontRequireReceiver);//view底下的节点请用OnDisable和OnEnable或者IUIElement代替

                UIManager.singleton.setCurPanel(this);
            }
            else
            {
                if (inStart == false)
                {
                    UIManager.singleton.removeCurPanel(this);

                    SendMessage("onPanelVisible", v, SendMessageOptions.DontRequireReceiver);//view底下的节点请用OnDisable和OnEnable或者IUIElement代替

                    disableUI();

                    viewVisible(v);
                }
                else
                {
                    viewVisible(v);
                }
            }
        }

        const string ui_view_name = "view";//可以有多个view节点 只有名字里有view这单词就可以了
        public void viewVisible(bool v)
        {
            foreach (Transform c in transform)
            {
                if (c.name.IndexOf(ui_view_name) >= 0)
                {
                    c.gameObject.SetActive(v);
                }
            }
        }


        void enableUI()
        {
            foreach (var uiElement in GetComponentsInChildren<IUIElement>(true))
            {
                var e = uiElement.onEnableUI();
                if (e != null)
                    StartCoroutine(e);
            }
        }
        void disableUI()
        {
            foreach (var uiElement in GetComponentsInChildren<IUIElement>(true))
            {
                uiElement.onDisableUI();
            }
        }
        public void updateUI()
        {
            foreach (var uiElement in GetComponentsInChildren<IUIElement>(true))
            {
                uiElement.onDisableUI();
                var e = uiElement.onEnableUI();
                if (e != null)
                    StartCoroutine(e);
            }
        }



        //
        public enum FadeState
        {
            None,
            Fadeout,
            Fadein,
        }
        public FadeState fadeState { get; set; }
        public bool fading { get { return fadeState > 0; } }
        public bool fadeout { get { return fadeState == FadeState.Fadeout; } }
        public bool fadein { get { return fadeState == FadeState.Fadein; } }


        //
        public IEnumerator Out()
        {
            return UIManager.singleton.fadeOut(this);
        }
        public void startOut()
        {
            StartCoroutine(Out());
        }

        public IEnumerator In()
        {
            return UIManager.singleton.fadeIn(this);
        }
        public void startIn()
        {
            StartCoroutine(In());
        }


        //
        //其他已经打开的界面只是透明了
        public IEnumerator fadeInForce()
        {
            return UIManager.singleton.fadeInForce(this);
        }



        //
        public IEnumerator In(UIPanel back)
        {
            addHistory(back);
            return UIManager.singleton.fadeIn(this);
        }
        public void startIn(UIPanel back)
        {
            addHistory(back);
            StartCoroutine(In());
        }


        //
        void addHistory(UIPanel back)
        {
            var bPanel = GetComponent<BPanel>();
            if (bPanel == null)
            {
                Debug.LogError("bPanel == null", this);
                return;
            }
            bPanel.back = back;
        }
        public void inBackPanel()
        {
            SendMessage("on_back");
        }

        public void inNextPanel()
        {
            SendMessage("on_next");
        }
    }
}