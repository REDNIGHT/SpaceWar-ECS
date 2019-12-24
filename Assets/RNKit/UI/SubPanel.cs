using UnityEngine;
using System.Collections;


namespace RN.UI
{
    public abstract class SubPanel : MonoBehaviour
    {
        public int channel { get { return GetComponent<UIPanel>().channel; } }
        public bool visible { get { return thisPanel.visible; } set { thisPanel.visible = value; } }

        public UIPanel thisPanel { get { return GetComponent<UIPanel>(); } }
        public RectTransform view { get { return transform.Find("view") as RectTransform; } }
        public RectTransform getView(string viewName) { return transform.Find(viewName) as RectTransform; }

        //
        public void updateUI() { thisPanel.updateUI(); }

        //
        //public UIHistory history { get { return thisPanel.vHistory; } }
        //public UIPanel lastPanel { get { return thisPanel.vHistory.peek(); } }

        public bool hasPanel { get { return UIManager.singleton.hasCurPanel(channel); } }
        public UIPanel curPanel { get { return UIManager.singleton.getCurPanel(channel); } }
        public RectTransform curPanelT { get { return UIManager.singleton.getCurPanelT(channel); } }
        public bool isTopPanel => UIManager.singleton.isTopPanel(thisPanel);

        //
        public UIPanel.FadeState fadeState { get { return thisPanel.fadeState; } }
        public bool fading { get { return thisPanel.fading; } }
        public bool fadeout { get { return thisPanel.fadeout; } }
        public bool fadein { get { return thisPanel.fadein; } }


        //
        public IEnumerator Out() { return thisPanel.Out(); }
        public void startOut() { thisPanel.startOut(); }

        public IEnumerator In() { return thisPanel.In(); }
        public void startIn() { thisPanel.startIn(); }

        public IEnumerator In(UIPanel back) { return thisPanel.In(back); }
        public void startIn(UIPanel back) { thisPanel.startIn(back); }
        public void inBackPanel() { thisPanel.inBackPanel(); }
        public void inNextPanel() { thisPanel.inNextPanel(); }

        public IEnumerator InForce() { return thisPanel.fadeInForce(); }
    }

    public abstract class SubPanel<T> : Singleton<T>
        where T : SubPanel<T>
    {
        public static UIPanel singletonPanel { get { return singleton != null ? singleton.GetComponent<UIPanel>() : null; } }

        public int channel { get { return GetComponent<UIPanel>().channel; } }
        public bool visible { get { return thisPanel.visible; } set { thisPanel.visible = value; } }

        public UIPanel thisPanel { get { return GetComponent<UIPanel>(); } }
        public RectTransform view { get { return transform.Find("view") as RectTransform; } }
        public RectTransform getView(string viewName) { return transform.Find(viewName) as RectTransform; }
        public RectTransform getView(int index) { return transform.GetChild(index) as RectTransform; }

        //
        public void updateUI() { thisPanel.updateUI(); }

        //
        //public UIHistory history { get { return thisPanel.vHistory; } }
        //public UIPanel lastPanel { get { return thisPanel.vHistory.peek(); } }

        public bool hasPanel { get { return UIManager.singleton.hasCurPanel(channel); } }
        public UIPanel curPanel { get { return UIManager.singleton.getCurPanel(channel); } }
        public RectTransform curPanelT { get { return UIManager.singleton.getCurPanelT(channel); } }
        public bool isTopPanel => UIManager.singleton.isTopPanel(thisPanel);

        //
        public UIPanel.FadeState fadeState { get { return thisPanel.fadeState; }  }
        public bool fading { get { return thisPanel.fading; } }
        public bool fadeout { get { return thisPanel.fadeout; } }
        public bool fadein { get { return thisPanel.fadein; } }


        //
        public IEnumerator Out() { return thisPanel.Out(); }
        public void startOut() { thisPanel.startOut(); }

        public IEnumerator In() { return thisPanel.In(); }
        public void startIn() { thisPanel.startIn(); }

        public IEnumerator In(UIPanel back) { return thisPanel.In(back); }
        public void startIn(UIPanel back) { thisPanel.startIn(back); }

        public void inBackPanel() { thisPanel.inBackPanel(); }
        public void inNextPanel() { thisPanel.inNextPanel(); }

        public IEnumerator InForce() { return thisPanel.fadeInForce(); }
    }
}