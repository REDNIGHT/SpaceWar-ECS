using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace RN.UI
{
    //todo...
    //MiniLoading 改为继承UIPanel
    //网络下载不稳定 不要用MiniLoading 避免锁屏
    public class MiniLoading : Singleton<MiniLoading>//, IVisible
    {
        //
        public class Using : System.IDisposable
        {
            public bool lockScreen { get; protected set; }
            public string text;
            public string _progress;
            public float progress { set { _progress = value.ToString("P0"); } }
            public Using(string _text, bool _lockScreen = false)
            {
                lockScreen = _lockScreen;
                text = _text;

                MiniLoading.singleton.visible(this, true);
            }
            public void Dispose()
            {
                MiniLoading.singleton.visible(this, false);
            }
        }


        //
        //在节点没激活的情况下 onPrepareAwake 依然会被调用
        protected void onPrepareAwake()
        {
            gameObject.SetActive(false);
            transform.localPosition = Vector3.zero;
            GetComponent<CanvasGroup>().alpha = 0f;
        }


        List<Using> usings = new List<Using>();
        public void visible(Using u, bool v)
        {
            //
            if (v)
                usings.Add(u);
            else if (usings.Remove(u) == false)
                Debug.LogError("loadingObjects.Remove(o) == false  u=" + u, this);

            StopAllCoroutines();
            gameObject.SetActive(true);
            StartCoroutine(visibleE());
        }

        const float fadeTime = 0.25f;
        IEnumerator visibleE()
        {
            //
            enabled = usings.Count > 0;
            if (enabled)
            {
                lockScreen = allLockScreen;
                text = allText;
                GetComponentInChildren<ContentSizeFitter>(true).SetLayoutVertical();
            }



            //
            {
                var cg = GetComponent<CanvasGroup>();
                var b = cg.alpha;
                var e = usings.Count > 0 ? 1f : 0f;
                foreach (var t in new TimeEquation().outCubic.playRealtime(fadeTime))
                {
                    //rectT.anchoredPosition = Vector2.Lerp(b, e, t);
                    cg.alpha = Mathf.Lerp(b, e, t);
                    yield return this;
                }
            }


            //
            if (usings.Count <= 0)
                gameObject.SetActive(false);
        }

        public void clear(string text)
        {
            foreach (var u in usings)
            {
                if (u.text == text)
                {
                    visible(u, false);
                    return;
                }
            }
        }

        public bool lockScreen
        {
            set
            {
                transform.Find("lockScreen").gameObject.SetActive(value);
            }
        }
        public string text
        {
            set
            {
                transform.Find("textBG/text").autoSetText(value);
                transform.Find("textBG").gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }

        protected void FixedUpdate()
        {
            text = allText;
        }

        public bool allLockScreen
        {
            get
            {
                foreach (var u in usings)
                {
                    if (u.lockScreen)
                    {
                        return u.lockScreen;
                    }
                }

                return false;
            }
        }

        public string allText
        {
            get
            {
                var at = "";
                foreach (var u in usings)
                {
                    at += u.text + (string.IsNullOrEmpty(u._progress) ? "" : "  " + u._progress) + "\n";
                }

                if (at.Length > 0)
                    at = at.Remove(at.Length - 1);

                return at;
            }
        }




        [RN._Editor.ButtonInEndArea]
        void test()
        {
            UIManager.singleton.StartCoroutine(testE());
        }
        IEnumerator testE()
        {
            using (var l = new MiniLoading.Using("test...\n..............", true))
            {
                for (var i = 0; i <= 100; ++i)
                {
                    yield return this;
                    yield return this;
                    yield return this;
                    yield return this;
                    yield return this;
                    yield return this;
                    yield return this;
                    yield return this;
                    l.progress = (float)i / 100f;
                }

                yield return this;
            }
        }
    }
}