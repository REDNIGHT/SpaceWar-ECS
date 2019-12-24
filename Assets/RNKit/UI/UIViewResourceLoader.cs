using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RN.UI
{
    //这脚本需要放到最上面
    public class UIViewResourceLoader : MonoBehaviour, IOnLoadUI
    {
        public bool destroyByHide = true;
        public string[] resources;

        protected void Awake()
        {
            if (resources.Length == 0)
                Debug.LogError("resources.Length == 0", this);

            if (isLoad)
            {
                foreach (Transform c in transform)
                    c.name = c.name.Replace(name + "_", "");

                SendMessage("onViewLoadComplete");
            }
        }

        public bool isLoad
        {
            get
            {
                foreach (Transform c in transform)
                {
                    if (c.name.IndexOf("view") >= 0)
                        return true;
                }

                return false;
            }
        }

        public IEnumerator onLoadUI()
        {
            if (isLoad)
                yield break;

            foreach (var resource in resources)
            {
                var async = Resources.LoadAsync<Transform>(resource);
                yield return async;

                if (async.asset == null)
                {
                    Debug.LogError("async.asset == null  resource=" + resource, this);
                    continue;
                }


                var prefab = (async.asset as Transform);

                var view = prefab.instantiate(transform, false, false);

                view.name = view.name.Replace("(Clone)", "");
                view.name = view.name.Replace(name + "_", "");
                yield return this;
            }

            SendMessage("onViewLoadComplete");
        }


        protected IEnumerator onPanelVisible(bool v)
        {
            if (v == false && destroyByHide)
                return onUIViewDestroy();

            return null;
        }


        protected IEnumerator onUIViewDestroy()
        {
            SendMessage("onViewUnload", SendMessageOptions.DontRequireReceiver);

            yield return this;
            //
            //foreach (Transform c in transform)//没激活的子节点 有时能有时不能被迭代的 isLoad里就能被迭代 fuck!
            foreach (var resource in resources)
            {
                var n = resource.Replace(name + "_", "");
                var c = transform.Find(n);
                c.destroyGO();
            }
        }

        public static void broadcastMessage_onUIViewDestroy()
        {
            foreach (Transform c in UIManager.singletonT)
            {
                var vrl = c.GetComponent<UIViewResourceLoader>();
                if (vrl != null && vrl.isLoad)
                    vrl.StartCoroutine(vrl.onUIViewDestroy());
            }
        }
    }
}