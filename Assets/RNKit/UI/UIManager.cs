using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace RN.UI
{
    public partial class UIManager : Singleton<UIManager>
    {
        public new static RectTransform singletonT
        {
            get
            {
                return Singleton<UIManager>.singletonT as RectTransform;
            }
        }

        protected new void Awake()
        {
            base.Awake();

            //
            vCurPanels = new List<UIPanel>(UIPanel.ChannelBeginNum);

            //
            /*var _camera = GUICamera;
            foreach (Transform c in transform)
            {
                var canvas = c.GetComponent<Canvas>();
                if (canvas == null)
                    continue;

                canvas.sortingOrder = c.GetSiblingIndex();

                canvas.worldCamera = _camera;
            }*/


            //
            fader = GetComponent<IUIPanelFader>();


            //
            eventSystem = EventSystem.current;
            if (eventSystem == null)
                Debug.LogError("eventSystem == null", this);
        }


        //------------------------------------------------------------------------------------------------
        public Camera GUICamera { get { return transform.find<Camera>("GUICamera"); } }


        //------------------------------------------------------------------------------------------------
        public int layer { get { return gameObject.layer; } }


        //------------------------------------------------------------------------------------------------
        EventSystem eventSystem;
        public bool inputEnabled { get { return eventSystem.enabled; } set { eventSystem.enabled = value; } }

        public class InputDisable : System.IDisposable
        {
            public InputDisable()
            {
                if (UIManager.singleton.inputEnabled == false)
                    Debug.LogError("UIManager.singleton.inputEnabled == false");

                UIManager.singleton.inputEnabled = false;
            }
            public void Dispose()
            {
                UIManager.singleton.inputEnabled = true;
            }
        }


        //------------------------------------------------------------------------------------------------
        public IUIPanelFader fader { get; private set; }


        //------------------------------------------------------------------------------------------------
        public List<UIPanel> vCurPanels;
        public IEnumerable<UIPanel> getCurPanels()
        {
            foreach (var p in vCurPanels)
            {
                if (p != null)
                    yield return p;
            }
        }
        public bool hasCurPanel(int channel)
        {
            if (channel < 0)
            {
                Debug.LogError("channel < 0", this);
                return false;
            }

            if (channel < vCurPanels.Count)
                return vCurPanels[channel] != null;
            else
                return false;
        }
        public UIPanel getCurPanel(int channel)
        {
            if (channel < 0)
            {
                //Debug.LogError("channel < 0", this);
                return null;
            }
            if (channel >= vCurPanels.Count)
            {
                Debug.LogError("channel >= vCurPanels.Count  channel=" + channel + "  vCurPanels.Count=" + vCurPanels.Count, this);
                return null;
            }
            return vCurPanels[channel];
        }
        public RectTransform getCurPanelT(int channel)
        {
            var p = getCurPanel(channel);
            if (p == null)
            {
                if (Application.isPlaying)
                    Debug.LogError("p == null", this);
                return null;
            }
            return p.transform as RectTransform;
        }

        public System.Action<UIPanel /*new*/, UIPanel /*old*/> onCurPanelChanged;
        public void setCurPanel(UIPanel panel)
        {
            if (panel.newChannel)
            {
                //找空位
                bool f = false;
                while (UIPanel.ChannelBeginNum > vCurPanels.Count)
                {
                    vCurPanels.Add(null);
                }

                for (var i = UIPanel.ChannelBeginNum; i < vCurPanels.Count; ++i)
                {
                    if (vCurPanels[i] == null)
                    {
                        panel.channel = i;
                        f = true;
                        break;
                    }
                }

                //新加channel
                if (f == false)
                {
                    panel.channel = vCurPanels.Count;
                    vCurPanels.Add(null);
                }
            }
            else
            {
                //该索引是否有初始化
                while (panel.channel >= vCurPanels.Count)
                {
                    vCurPanels.Add(null);
                }
            }


            if (vCurPanels[panel.channel] != null)
                Debug.LogError("vCurPanels[channel] != null"
                    + "\nvCurPanels[channel]=" + vCurPanels[panel.channel]
                    + "\nnew panel=" + panel
                    + "\nchannel=" + panel.channel
                    , vCurPanels[panel.channel]);

            //
            var old = vCurPanels[panel.channel];
            vCurPanels[panel.channel] = panel;
            if (onCurPanelChanged != null)
                onCurPanelChanged(panel, old);

            //Debug.LogWarning("n=" + panel + "  o=" + old);
        }
        public void removeCurPanel(UIPanel panel)
        {
            if (vCurPanels.Count == 0)
            {
                Debug.LogError("vCurPanels.Count == 0  panel=" + panel, panel);
                return;
            }

            if (panel.channel >= vCurPanels.Count)
            {
                Debug.LogError("panel.channel >= vCurPanels.Count  panel=" + panel, panel);
                return;
            }

            if (vCurPanels[panel.channel] == null)
            {
                Debug.LogError("vCurPanels[panel.channel] == null  panel=" + panel, panel);
                return;
            }

            if (vCurPanels[panel.channel] != panel)
            {
                Debug.LogError("vCurPanels[panel.channel] != panel  panel=" + panel + "  channel=" + panel.channel, panel);
                return;
            }


            var old = vCurPanels[panel.channel];
            vCurPanels[panel.channel] = null;
            if (onCurPanelChanged != null)
                onCurPanelChanged(null, old);

            //Debug.LogWarning("n=" + null + "  o=" + old);
        }

        public void updateCurUIPanel()
        {
            foreach (var p in getCurPanels())
                p.updateUI();
        }

        public bool isTopPanel(UIPanel panel)
        {
            for (var i = vCurPanels.Count - 1; i >= 0; --i)
            {
                if (vCurPanels[i] != null)
                {
                    return vCurPanels[i] == panel;
                }
            }

            return false;
        }

        public void broadcastMessageToAllPanels(string methodName, object value = null)
        {
            foreach (Transform c in transform)
            {
                var panel = c.GetComponent<UIPanel>();
                if (panel != null)
                    panel.SendMessage(methodName, value, SendMessageOptions.DontRequireReceiver);
            }
        }

        public void broadcastMessageToCurPanels(string methodName, object value = null)
        {
            foreach (var panel in getCurPanels())
            {
                if (panel != null)
                    panel.SendMessage(methodName, value, SendMessageOptions.DontRequireReceiver);
            }
        }


        //
        public void setCurPanelViewAlpha(float a)
        {
            foreach (var panel in getCurPanels())
            {
                var cg = panel.transform.find<CanvasGroup>("view");
                if (cg == null)
                    cg = panel.view.gameObject.AddComponent<CanvasGroup>();

                cg.alpha = a;
            }
        }

        //
        public UIPanel.FadeState getFadingState(int channel)
        {
            var p = getCurPanel(channel);
            if (p != null)
                return p.fadeState;
            return UIPanel.FadeState.None;
        }
        public bool getFading(int channel) { return getFadingState(channel) > 0; }
        public bool getFadeout(int channel) { return getFadingState(channel) == UIPanel.FadeState.Fadeout; }
        public bool getFadein(int channel) { return getFadingState(channel) == UIPanel.FadeState.Fadein; }


        //------------------------------------------------------------------------------------------------
        [Header("Audio")]
        public AudioClip clickSound;
        public AudioClip hoverSound;
        //public AudioClip warnSound;
        //public AudioClip errorSound;
        [RN._Editor.ButtonInEndArea]
        public void playClickSound()
        {
            if (clickSound == null)
                return;

            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
        }
        [RN._Editor.ButtonInEndArea]
        public void playHoverSound()
        {
            if (hoverSound == null)
                return;

            AudioSource.PlayClipAtPoint(hoverSound, Camera.main.transform.position);
        }

        /*[RN._Editor.ButtonInEndArea]
        public void playWarnSound()
        {
            if (warnSound == null)
                return;

            AudioSource.PlayClipAtPoint(warnSound, Camera.main.transform.position);
        }
        [RN._Editor.ButtonInEndArea]
        public void playErrorSound()
        {
            if (errorSound == null)
                return;

            AudioSource.PlayClipAtPoint(errorSound, Camera.main.transform.position);
        }*/


        //------------------------------------------------------------------------------------------------

        public IEnumerator fadeOut(UIPanel panel)
        {
            //Debug.Log(panel + ".fadeOut()", panel);

            //
            var curPanel = getCurPanel(panel.channel);
            if (curPanel != panel)
            {
                Debug.LogError("curPanel != panel  panel=" + panel
                    + "  curPanel=" + curPanel
                    //+ "  lastPanel=" + getLastPanel(channel)
                    , this);
                yield break;
            }

            if (curPanel.visible == false)
            {
                //应该是相同的界面淡出了两次
                Debug.LogError("curPanel.visible == false  curPanel=" + curPanel, curPanel);
                yield break;
            }

            //
            while (panel.fading == true)
            {
                //Debug.LogError($"{panel.name}.fading == true  fadeState={panel.fadeState}  panel.channel{panel.channel}", panel);
                yield return panel;
            }


            //
            panel.BroadcastMessage("onFadeOutStart", SendMessageOptions.DontRequireReceiver);

            panel.fadeState = UIPanel.FadeState.Fadeout;
            if (fader != null)
                yield return fader.Out(panel);
            panel.visible = false;
            panel.fadeState = UIPanel.FadeState.None;
        }

        public IEnumerator fadeOut(int channel)
        {
            return fadeOut(getCurPanel(channel));
        }

        public IEnumerator fadeIn(UIPanel panel)
        {
            //Debug.Log(panel + ".fadeIn()", panel);

            //
            //while (Message.singleton.visible)
            //    yield return this;

            //
            List<IOnLoadUI> iOnLoadUIs = new List<IOnLoadUI>();
            {
                iOnLoadUIs.AddRange(panel.GetComponents<IOnLoadUI>());


                if (iOnLoadUIs.Count > 0)
                {
                    foreach (var i in iOnLoadUIs)
                    {
                        var e = i.onLoadUI();
                        if (e != null)
                            yield return e;
                    }
                }

                iOnLoadUIs.Clear();
            }

            //
            panel.fadeState = UIPanel.FadeState.Fadein;
            panel.visible = true;

            if (fader != null)
                yield return fader.In(panel);

            panel.fadeState = UIPanel.FadeState.None;
            panel.BroadcastMessage("onFadeInComplete", SendMessageOptions.DontRequireReceiver);
        }

        const float fadeForceTime = 0.5f;
        public IEnumerator fadeInForce(UIPanel panel)
        {
            //Debug.Log(panel + ".fadeInForce()", panel);

            var curPanel = getCurPanel(panel.channel);
            if (curPanel != null)
            {
                //用其他比较少用的channel
                Debug.LogError("curPanel != null", this);
                yield break;
            }


            //
            var canvasGroups = new List<CanvasGroup>();
            foreach (var p in getCurPanels())
                canvasGroups.Add(p.GetComponent<CanvasGroup>());

            foreach (var t in new TimeEquation().outCubic.playInverseRealtime(fadeForceTime))
            {
                foreach (var canvasGroup in canvasGroups)
                    canvasGroup.alpha = t;
                yield return this;
            }
            foreach (var canvasGroup in canvasGroups)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }


            yield return panel.In();


            while (panel.visible)
                yield return this;


            foreach (var canvasGroup in canvasGroups)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            foreach (var t in new TimeEquation().outCubic.playRealtime(fadeForceTime))
            {
                foreach (var canvasGroup in canvasGroups)
                    canvasGroup.alpha = t;
                yield return this;
            }
        }
    }
}