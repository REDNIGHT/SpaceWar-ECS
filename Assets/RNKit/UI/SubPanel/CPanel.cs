using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace RN.UI
{
    public sealed partial class CPanel : SubPanel
    {
#if OBJECT_PATH_LINK
        public string inPanelPath;
        public Transform inPanel
        {
            get
            {
                var t = transform.root.Find(inPanelPath.Replace(UIBinding.ignore, ""));
                if (t == null)
                {
                    Debug.LogError(this + ".inPanelPath=" + inPanelPath, this);
                    return null;
                }
                return t;
            }
        }
#else
        //public UIPanel _in;
#endif

        public IEnumerator on_close()
        {
            if (enabled == false)
                yield break;

            //var n = _in;

            yield return Out();
            //if (n != null)
            //    yield return n.In();
        }
    }
}