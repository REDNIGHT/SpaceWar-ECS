using UnityEngine;
using System.Collections;

namespace RN.UI
{
    public sealed class NPanel : SubPanel
    {
#if OBJECT_PATH_LINK
        public string nextPanelPath;
        public Transform nextPanel
        {
            get
            {
                var t = transform.root.Find(nextPanelPath.Replace(UIBinding.ignore, ""));
                if (t == null)
                {
                    Debug.LogError(this + ".nextPanelPath=" + nextPanelPath, this);
                    return null;
                }
                return t;
            }
        }
#else
        public UIPanel next;
        /*protected void Awake()
        {
            if (nextPanel == null)
                Debug.LogError("nextPanel == null", this);
        }*/
#endif
        public IEnumerator on_next()
        {
            if (enabled == false)
                yield break;

            var n = next;
            if (n == null)
                Debug.LogError("nextPanel == null", this);

            //
            //n.addHistoryInBPanel(thisPanel);

            //
            yield return Out();
            yield return n.In(thisPanel);
        }
    }
}