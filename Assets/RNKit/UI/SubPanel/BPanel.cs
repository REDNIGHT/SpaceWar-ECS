using UnityEngine;
using System.Collections;

namespace RN.UI
{
    public sealed class BPanel : SubPanel
    {
        private void Start() { }

#if OBJECT_PATH_LINK
        public string backPanelPath;
        public Transform backPanel
        {
            get
            {
                var t = transform.root.Find(backPanelPath.Replace(UIBinding.ignore, ""));
                if (t == null)
                {
                    Debug.LogError(this + ".backPanelPath=" + backPanelPath, this);
                    return null;
                }
                return t;
            }
        }
#else
        public UIPanel back;
#endif


        //public int outChannel = -1;
        public IEnumerator on_back()
        {
            //用UIBinding淡出绑定的界面
            /*if (outChannel >= 0)
            {
                var outPanel = UIManager.singleton.getCurPanel(outChannel);
                if (outPanel == null)
                    Debug.LogError("outPanel == null  outChannel=" + outChannel, this);
                else
                    outPanel.startOut();
            }*/

            if (enabled == false)
                yield break;

            yield return Out();

            if (back == null)
            {
                //Debug.LogError("back == null", this);
                yield break;
            }
            yield return back.In();
        }

        public static System.Func<bool> onBack = () =>
        {
            return Input.GetKeyUp(KeyCode.Escape);
        };

        public bool autoBack = true;
        private IEnumerator onPanelVisible(bool v)
        {
            if (onBack == null || autoBack == false)
                yield break;

            //yield return new WaitForFixedUpdate();
            yield return this;

            while (visible)
            {
                //yield return new WaitForFixedUpdate();
                yield return this;

                if (isTopPanel && onBack())
                {
                    //需要隔开一帧执行
                    //如果有两个panel是打开着的 在没有延迟执行的情况 两panel的isTopPanel都是返回true
                    //inBackPanel()会把当前的panel关闭 另一个panel的isTopPanel就变成true
                    yield return this;

                    /*if (UIManager.singleton.getFading(channel))
                        continue;*/

                    inBackPanel();
                    yield break;
                }
            }
        }
    }
}