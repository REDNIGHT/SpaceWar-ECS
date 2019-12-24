using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public static class GameObjectEx
{
    //------------------------------------------------------------------------------------
    public static T instantiate<T>(this T prefab, Transform parent, bool worldStays) where T : Component
    {
        return Object.Instantiate<T>(prefab, parent, worldStays);
    }
    public static T instantiate<T>(this T prefab, Transform parent, bool worldStays, bool active) where T : Component
    {
        var activeSelf = prefab.gameObject.activeSelf;
        prefab.gameObject.SetActive(false);
        var ins = Object.Instantiate<T>(prefab, parent, worldStays);
        prefab.gameObject.SetActive(activeSelf);

        ins.gameObject.SetActive(active);
        return ins;
    }


    //
    /*public static T _getComponent<T>(this object obj) where T : Component
    {
        var t = obj as T;
        if (t != null)
            return t;

        var com = obj as Component;
        if (com != null)
        {
            t = com.GetComponent<T>();

            if (t != null)
                return t;
        }

        var go = obj as GameObject;
        if (go != null)
        {
            t = go.GetComponent<T>();

            if (t != null)
                return t;
        }

        return null;
    }*/

    //------------------------------------------------------------------------------------
    /*
    //这函数调用后，要等到下一帧才会真正把所有子物体删除
    //如果在这函数调用后马上访问对子物体个数或迭代子物体可能会出现不正确的结果
    //例如调用这函数后马上访问transform.childCount 不一定会返回0
    //可以使用MonoBehaviour.Invoke这函数来解决这问题
    //这问题在移动设备上会更明显，PC上一般不会出问题
    public static void destroyChildren(Transform self)
    {
        foreach (Transform c in self)
        {
            Object.Destroy(c.gameObject);
        }
    }
    */

    static List<Transform> removes = new List<Transform>();
    public static void destroyChildrenGO(this Component self)
    {
        foreach (Transform c in self.transform)
            removes.Add(c);

        foreach (Transform c in removes)
        {
            //c.name += "(destroy)";
            c.destroyGO();
        }

        removes.Clear();
    }
    public static void destroyChildrenGO(this Component self, System.Func<Transform, bool> ignore)
    {
        foreach (Transform c in self.transform)
            removes.Add(c);

        foreach (Transform c in removes)
        {
            if (ignore(c))
                continue;
            c.destroyGO();
        }

        removes.Clear();
    }
    public static void destroyChildrenGO(this Component self, string ignoreName)
    {
        foreach (Transform c in self.transform)
            removes.Add(c);

        foreach (Transform c in removes)
        {
            if (c.name == ignoreName)
                continue;
            c.destroyGO();
        }

        removes.Clear();
    }


    public static void destroy(this GameObject go)
    {
        go.name += "(destroy)";
        go.SetActive(false);
        go.transform.SetParent(null);
        Object.Destroy(go);
    }
    public static void destroy(this GameObject go, float delay)
    {
        go.name += "(destroy)";
        go.SetActive(false);
        go.transform.SetParent(null);
        Object.Destroy(go, delay);
    }
    public static void destroy(this Component self)
    {
        if (self == null)
            Debug.LogError("destroy  self == null");
        Object.Destroy(self);
    }

#if IN_MY_RN_PROJECT
    public static void stopAndDestroyFx(this MonoBehaviour self)
    {
        if (self == null)
            Debug.LogError("destroy  self == null");
        self.stopAndDestroyParticleSystem();
    }
#endif

    public static void destroyGO(this Component self)
    {
        self.gameObject.destroy();
    }
    public static void destroyGO(this Component self, float delay)
    {
        destroy(self.gameObject, delay);
    }
    public static void destroyChildrenGOImmediate(this Component self)
    {
        foreach (Transform c in self.transform)
            removes.Add(c);

        foreach (Transform c in removes)
        {
            c.name += "(destroy)";
            Object.DestroyImmediate(c.gameObject);
        }

        removes.Clear();
    }
    public static void destroyImmediate(this Component self)
    {
        Object.DestroyImmediate(self);
    }
    public static void destroyImmediateGO(this Component self)
    {
        Object.DestroyImmediate(self.gameObject);
    }

    //------------------------------------------------------------------------------------
    /*
     * 用这函数时 v是true时 可能会出现在onVisible函数中把子节点隐藏后 但是最后还是会显示的问题
     * 因为子节点也有onVisible函数 在广播信息是也调用了子节点的onVisible函数
     * 解决方法 只要在onVisible函数中返回false 阻止信息继续发送 问题就解决了
     */
    public static void visible(this Component self, bool v)
    {
        if (self == null)//有可能被删除
        {
            Debug.LogError("self == null", self);
            return;
        }

        //
#if IN_MY_RN_PROJECT
        if (self.gameObject.layer == 5)//Builtin Layer 5 is UI
        {
            self.uiVisible(v);
            return;
        }
#endif

        //
        bool hasRenderers = false;
        var renderers = self.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            hasRenderers = true;
            r.enabled = v;
#if UNITY_EDITOR
            _visibleLog(r, v);
#endif
        }

        var graphics = self.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        foreach (var g in graphics)
        {
            hasRenderers = true;
            g.enabled = v;
        }

        var colliders = self.GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders)
        {
            hasRenderers = true;
            c.enabled = v;
        }

        if (hasRenderers)
            self.BroadcastMessage("onVisible", v, SendMessageOptions.DontRequireReceiver);
        else
            self.gameObject.SetActive(v);
    }

    /*public static void visibleWOC(this Component self, bool v)
    {
        if (self == null)//有可能被删除
        {
            Debug.LogError("self == null", self);
            return;
        }

        var renderers = self.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            r.enabled = v;

#if UNITY_EDITOR
            _visibleLog(r, v);
#endif
        }

        //var graphics = self.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        //foreach (var g in graphics)
        //    g.enabled = v;

        self.BroadcastMessage("onVisible", v, SendMessageOptions.DontRequireReceiver);
    }

    public static void visibleWOM(this Component self, bool v)
    {
        if (self == null)//有可能被删除
        {
            Debug.LogError("self == null", self);
            return;
        }

        //
        if (self.gameObject.layer == 5)//Builtin Layer 5 is UI
        {
            self.visibleUI(v);
            return;
        }

        //
        var renderers = self.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            r.enabled = v;

#if UNITY_EDITOR
            _visibleLog(r, v);
#endif
        }

        //var graphics = self.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        //foreach (var g in graphics)
        //    g.enabled = v;

        var colliders = self.GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders)
        {
            c.enabled = v;
        }
    }
    public static void visibleWOCM(this Component self, bool v)
    {
        if (self == null)//有可能被删除
        {
            Debug.LogError("self == null", self);
            return;
        }

        var renderers = self.getComponentsInChildrenWithDisable<Renderer>();
        foreach (var r in renderers)
        {
            r.enabled = v;

#if UNITY_EDITOR
            _visibleLog(r, v);
#endif
        }
    }*/

#if UNITY_EDITOR
    static void _visibleLog(Renderer r, bool v)
    {
#if false
        if (r.name == "goldPrice")//在这写上需要测试的object的名字
            Debug.LogError(r.name + "  v=" + v, r);
#endif
    }
#endif


    //------------------------------------------------------------------------------------
    public static void collidersEnable(this Component self, bool v)
    {
        if (self == null)//有可能被删除
        {
            Debug.LogError("self == null", self);
            return;
        }

        var colliders = self.GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders)
        {
            c.enabled = v;
        }
    }

    //------------------------------------------------------------------------------------
    public static void setLayer(this Component self, int layer)
    {
        self.gameObject.layer = layer;
        foreach (Transform c in self.transform)
        {
            setLayer(c, layer);
        }
    }
    public static void setLayer(this GameObject self, int layer)
    {
        self.layer = layer;
        foreach (Transform c in self.transform)
        {
            setLayer(c.gameObject, layer);
        }
    }


    //------------------------------------------------------------------------------------
    public static void sendMessage<T>(this Component self, System.Action<T> function)
    {
        self.GetComponents<T>().forEach(function);
    }

    public static void broadcastMessage<T>(this Component self, System.Action<T> function)
    {
        self.GetComponentsInChildren<T>(true).forEach(function);
    }

    public static void sendMessageToChildren<T>(this Component self, System.Action<T> function)
    {
        foreach (Transform t in self.transform)
            t.sendMessage<T>(function);
    }

    public static void sendMessageUpwards<T>(this Component self, System.Action<T> function)
    {
        var t = self.GetComponentInParent<T>();
        if (t != null)
            function(t);
    }


    static BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    public static void callFunction(this MonoBehaviour self, string methodName, params object[] values)
    {
        var m = self.GetType().GetMethod(methodName, bindingAttr);

        var e = m.Invoke(self, values) as IEnumerator;
        if (e != null)
        {
            self.StartCoroutine(e);
        }
    }

    public static void sendMessage(this Component self, string methodName, params object[] values)
    {
        self.GetComponents<MonoBehaviour>().forEach(x =>
        {
            var m = x.GetType().GetMethod(methodName, bindingAttr);

            if (m == null) return;

            var e = m.Invoke(x, values) as IEnumerator;
            if (e != null)
            {
                x.StartCoroutine(e);
            }
        });
    }

    public static void sendMessageUpwards(this Component self, string methodName, params object[] values)
    {
        self.GetComponentsInParent<MonoBehaviour>(true).forEach(x =>
        {
            var m = x.GetType().GetMethod(methodName, bindingAttr);

            if (m == null) return;

            var e = m.Invoke(x, values) as IEnumerator;
            if (e != null)
            {
                x.StartCoroutine(e);
            }
        });
    }

    public static void sendMessageToChildren(this Component self, string methodName, params object[] values)
    {
        foreach (Transform t in self.transform)
        {
            t.GetComponents<MonoBehaviour>().forEach(x =>
            {
                var m = x.GetType().GetMethod(methodName, bindingAttr);

                if (m == null) return;

                var e = m.Invoke(x, values) as IEnumerator;
                if (e != null)
                {
                    x.StartCoroutine(e);
                }
            });
        }
    }

    public static void broadcastMessageToType(this Component self, System.Type type, string methodName, params object[] values)
    {
        self.GetComponentsInChildren(type).forEach(x =>
        {
            var m = x.GetType().GetMethod(methodName, bindingAttr);

            if (m == null) return;

            var e = m.Invoke(x, values) as IEnumerator;
            if (e != null)
            {
                (x as MonoBehaviour).StartCoroutine(e);
            }
        });
    }

    public static void broadcastMessage(this Component self, string methodName, params object[] values)
    {
        self.GetComponentsInChildren<MonoBehaviour>(true).forEach(x =>
        {
            var m = x.GetType().GetMethod(methodName, bindingAttr);

            if (m == null) return;

            var e = m.Invoke(x, values) as IEnumerator;
            if (e != null)
            {
                x.StartCoroutine(e);
            }
        });
    }

    public static void sendPrefixMessage(this Component self, string methodPrefix, params object[] values)
    {
        self.GetComponents<MonoBehaviour>().forEach(x =>
        {
            x.GetType().GetMethods(bindingAttr)
                .Where(y => y.Name.StartsWith(methodPrefix))
                .forEach(z =>
                {
                    var e = z.Invoke(x, values) as IEnumerator;
                    if (e != null)
                    {
                        x.StartCoroutine(e);
                    }
                });
        });
    }

    public static void callPrefixFunctions(this MonoBehaviour self, string methodPrefix, params object[] values)
    {
        self.GetType().GetMethods(bindingAttr)
            .Where(y => y.Name.StartsWith(methodPrefix))
            .forEach(z =>
            {
                var e = z.Invoke(self, values) as IEnumerator;
                if (e != null)
                {
                    self.StartCoroutine(e);
                }
            });
    }


    //
    /*
    public static void callFunction1v(this MonoBehaviour self, string methodName, object value)
    {
        self.callFunction(methodName, value);
    }
    public static void sendMessage1vEx(this Component self, string methodName, object value)
    {
        self.sendMessageEx(methodName, value);
    }
    public static void sendMessage1vUpwardsEx(this Component self, string methodName, object value)
    {
        self.sendMessageUpwardsEx(methodName, value);
    }
    public static void sendMessage1vToChildrenEx(this Component self, string methodName, object value)
    {
        self.sendMessageToChildrenEx(methodName, value);
    }
    public static void broadcastMessage1vToTypeEx(this Component self, System.Type type, string methodName, object value)
    {
        self.broadcastMessageToTypeEx(type, methodName, value);
    }

    public static void broadcastMessage1vEx(this Component self, string methodName, object value)
    {
        self.broadcastMessageEx(methodName, value);
    }
    public static void sendMessageEx1v_Prefix(this Component self, string methodPrefix, object value)
    {
        self.sendMessageEx_Prefix(methodPrefix, value);
    }
    public static void callFunctions1v_Prefix(this MonoBehaviour self, string methodPrefix, object value)
    {
        self.callFunctions_Prefix(methodPrefix, value);
    }

    public static void callFunction2v(this MonoBehaviour self, string methodName, object valueA, object valueB)
    {
        self.callFunction(methodName, valueA, valueB);
    }
    public static void sendMessage2vEx(this Component self, string methodName, object valueA, object valueB)
    {
        self.sendMessageEx(methodName, valueA, valueB);
    }
    public static void sendMessage2vUpwardsEx(this Component self, string methodName, object valueA, object valueB)
    {
        self.sendMessageUpwardsEx(methodName, valueA, valueB);
    }
    public static void sendMessage2vToChildrenEx(this Component self, string methodName, object valueA, object valueB)
    {
        self.sendMessageToChildrenEx(methodName, valueA, valueB);
    }
    public static void broadcastMessage2vToTypeEx(this Component self, System.Type type, string methodName, object valueA, object valueB)
    {
        self.broadcastMessageToTypeEx(type, methodName, valueA, valueB);
    }
    public static void broadcastMessage2vEx(this Component self, string methodName, object valueA, object valueB)
    {
        self.broadcastMessageEx(methodName, valueA, valueB);
    }
    public static void sendMessageEx2v_Prefix(this Component self, string methodPrefix, object valueA, object valueB)
    {
        self.sendMessageEx_Prefix(methodPrefix, valueA, valueB);
    }
    public static void callFunctions2v_Prefix(this MonoBehaviour self, string methodPrefix, object valueA, object valueB)
    {
        self.callFunctions_Prefix(methodPrefix, valueA, valueB);
    }
    */


    //------------------------------------------------------------------------------------
    public static Transform findObjectByTag(this Transform self, string tag)
    {
        if (self.CompareTag(tag))
            return self;

        foreach (Transform c in self)
        {
            var t = findObjectByTag(c, tag);
            if (t != null)
                return t;
        }

        return null;
    }
    public static List<Transform> findObjectsByTag(this Transform self, string tag)
    {
        var list = new List<Transform>();
        findObjectsByTag(self, tag, list);
        return list;
    }

    public static void findObjectsByTag(this Transform self, string tag, List<Transform> list)
    {
        if (self.CompareTag(tag))
            list.Add(self);

        foreach (Transform c in self)
        {
            findObjectsByTag(c, tag, list);
        }
    }

    //
    public static Transform findObjectByLayer(this Transform self, int layer)
    {
        if ((1 << self.gameObject.layer & layer) > 0)
            return self;

        foreach (Transform c in self)
        {
            var t = findObjectByLayer(c, layer);

            if (t != null)
                return t;
        }
        return null;
    }
    public static List<Transform> findObjectsByLayer(this Transform self, int layer)
    {
        var list = new List<Transform>();
        findObjectsByLayer(self, layer, list);
        return list;
    }
    public static void findObjectsByLayer(this Transform self, int layer, List<Transform> list)
    {
        if ((1 << self.gameObject.layer & layer) > 0)
            list.Add(self);

        foreach (Transform c in self)
        {
            findObjectsByLayer(c, layer, list);
        }
    }

    public static T find<T>(this Transform self, string name)
    {
        var t = self.Find(name);
        if (t != null)
            return t.GetComponent<T>();
        return default(T);
    }

    //no find myself
    public static Transform _find(this Transform self, string name, int level = int.MaxValue, int curLevel = 0)
    {
        if (curLevel > level)
            return null;

        //if (self.name == name)
        //    return self;

        var r = self.Find(name);
        if (r != null)
            return r;

        ++curLevel;
        foreach (Transform c in self)
        {
            var t = _find(c, name, level, curLevel);
            if (t != null)
                return t;
        }
        return null;
    }

    public static Transform _find2(this Transform self, string namePart)
    {
        foreach (Transform c in self)
        {
            if (c.name.IndexOf(namePart) >= 0)
                return c;
        }
        return null;
    }

    /*public static Transform _find(this Component self, string name, int level = int.MaxValue, int curLevel = 0)
    {
        return _find(self.transform, name, level, curLevel);
    }*/

    public static List<Transform> findObjectsByName(this Transform self, string name)
    {
        var list = new List<Transform>();
        findObjectsByName(self, name, list);
        return list;
    }
    /*public static List<Transform> findObjectsByName(this Component self, string name)
    {
        return findObjectsByName(self.transform, name);
    }*/
    public static void findObjectsByName(this Transform self, string name, List<Transform> list)
    {
        if (self.name == name)
            list.Add(self);

        foreach (Transform c in self)
        {
            findObjectsByName(c, name, list);
        }
    }

    public static IEnumerable<T> getComponentsInOneLevelChildren<T>(this Transform self) where T : Component
    {
        foreach (Transform c in self)
        {
            var t = c.GetComponent<T>();
            if (t != null)
                yield return t;
        }
    }

    /*public static List<T> getComponentsInChildrenWithDisable<T>(this Component self) where T : Component
    {
        var list = new List<T>();
        getComponentsInChildrenWithDisable<T>(self, list);
        return list;
    }

    public static void getComponentsInChildrenWithDisable<T>(this Component self, List<T> list) where T : Component
    {
        foreach (var c in self.GetComponents<T>())
            list.Add(c);

        foreach(Transform c in self.transform)
            getComponentsInChildrenWithDisable<T>(c, list);
    }*/

    //------------------------------------------------------------------------------------
    public static string getHierarchy(this Transform transform, Transform root = null)
    {
        var h = transform.name;
        var parent = transform.parent;
        while (parent != root && parent != null)
        {
            h = parent.name + "/" + h;
            parent = parent.parent;
        }
        return h;
    }

    public static void get_name_hierarchy(this string hierarchy, out string n, out string h)
    {
        var index = hierarchy.LastIndexOf("/");
        n = hierarchy.Substring(index, hierarchy.Length - index);
        h = hierarchy.Substring(0, index);
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("RNTools/print hierarchy")]
    public static void printHierarchy()
    {
        var ts = UnityEditor.Selection.transforms;
        var str = "";
        foreach (var t in ts)
            str += t.getHierarchy() + "\n";

        Debug.Log(str, UnityEditor.Selection.activeTransform);

        UnityEditor.EditorGUIUtility.systemCopyBuffer = str;
    }
#endif

    //------------------------------------------------------------------------------------

    public class CoroutineManager : RN.Singleton<CoroutineManager>
    {
        public static new CoroutineManager singleton
        {
            get
            {
                var s = RN.Singleton<CoroutineManager>.singleton;
                if (s == null)
                {
                    var cm = new GameObject().AddComponent<CoroutineManager>();
                    GameObject.DontDestroyOnLoad(cm);
                    return RN.Singleton<CoroutineManager>.singleton;
                }

                return s;
            }
        }

        [RN._Editor.ButtonInEndArea]
        void printCoroutine()
        {
            var str = "";
            coroutineMap.forEach(x =>
            {
                str += x.Key + "; ";
            });
            Debug.Log(str);
        }
    }

    static Dictionary<object, Coroutine> coroutineMap = new Dictionary<object, Coroutine>();
    public static void startCoroutine(this object key, IEnumerator routine)
    {
        Coroutine coroutine;
        if (coroutineMap.TryGetValue(key, out coroutine))
        {
            CoroutineManager.singleton.StopCoroutine(coroutine);
        }

        CoroutineManager.singleton.StartCoroutine(routine);
    }

    public static void stopCoroutine(this object key)
    {
        Coroutine coroutine;
        if (coroutineMap.TryGetValue(key, out coroutine))
        {
            CoroutineManager.singleton.StopCoroutine(coroutine);
            coroutineMap.Remove(key);
        }
    }



    //------------------------------------------------------------------------------------
    //http://wiki.unity3d.com/index.php/IsVisibleFrom
    /*
        void Update()
        {
            if (renderer.IsVisibleFrom(Camera.main)) Debug.Log("Visible");
            else Debug.Log("Not visible");
        }
    */

    public static bool isVisibleFrom(this Renderer renderer, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    public static bool isVisibleFrom(this Component c, Camera camera)
    {
        //todo... 找renderer
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, c.GetComponent<Renderer>().bounds);
    }

    public static bool testPointInPlanes(this Plane[] planes, Vector3 point)
    {
        foreach (var p in planes)
        {
            if (p.GetSide(point))
                continue;
            else
                return false;
        }
        return true;
    }
    public static bool testPointInFrustum(this Camera camera, Vector3 point)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return testPointInPlanes(planes, point);
    }


    //
#if UNITY_EDITOR
    static Transform getDebugGO(string name)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name);
            //go.AddComponent<MeshRenderer>();
            //go.AddComponent<MeshFilter>();
        }
        return go.transform;
    }
    public static void debugGO(Vector3 pos, string name = "__debugGO")
    {
        var dpt = getDebugGO(name);
        if (dpt == null)
            return;

        dpt.position = pos;
    }
    public static void debugGO(Vector3 pos, Vector3 normal, string name = "__debugGO")
    {
        var dpt = getDebugGO(name);
        if (dpt == null)
            return;

        dpt.position = pos;
        dpt.forward = -normal;
    }
    public static void debugLineGO(Vector3[] points, string name = "__debugGO")
    {
        var dpt = getDebugGO(name);
        if (dpt == null)
            return;
        var lineRenderer = dpt.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("lineRenderer == null");
            return;
        }
        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPositions(points);
    }
    public static void debugLineGO(string name, params Vector3[] points)
    {
        debugLineGO(points, name);
    }
#endif


    //
    public static string str(this Component component, params string[] values)
    {
        var s = component.ToString();
        foreach (var v in values)
            s += "  " + v + '\n';
        return s;
    }
}








//------------------------------------------------------------------------------------
/*public static MethodInfo getMethod(this object self, string methodName)
{
    //如果self是null 可能是掉包丢失了
    return self.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
}

public static object invoke(this object self, string methodName, params object[] values)
{
    //Debug.Log("self.name=" + self + "  methodName=" + methodName + "  values.Length=" + values.Length, self as MonoBehaviour);
    var f = getMethod(self, methodName);

    if (f != null)
        return _invoke(self, f, values);
    else
        Debug.LogError("cannot find the methodName:" + methodName + "   self=" + self, self as Object);

    return null;
}

public static bool invoke2(this object self, string methodName, params object[] values)
{
    var f = getMethod(self, methodName);

    if (f != null)
    {
        _invoke(self, f, values);
        return true;
    }

    return false;
}

public static object _invoke(object obj, MethodInfo f, params object[] values)
{
#if UNITY_EDITOR
    try
    {
        return f.Invoke(obj, values);
    }
    catch (System.ArgumentException exc)
    {
        Debug.LogError(exc.Message + "  fun=" + obj + "." + f.Name, obj as MonoBehaviour);
        return null;
    }
    catch (System.Reflection.TargetParameterCountException exc)
    {
        Debug.LogError(exc.Message + "  fun=" + obj + "." + f.Name, obj as MonoBehaviour);
        return null;
    }
    catch (System.MethodAccessException exc)
    {
        Debug.LogError(exc.Message + "  fun=" + obj + "." + f.Name, obj as MonoBehaviour);
        return null;
    }
    catch (System.InvalidOperationException exc)
    {
        Debug.LogError(exc.Message + "  fun=" + obj + "." + f.Name, obj as MonoBehaviour);
        return null;
    }
#else
    //Debug.Log("f.GetParameters().Length=" + f.GetParameters().Length, self);
    return f.Invoke(obj, values);
#endif
}

public static bool invoke3(this object self, string methodName, object value)
{
    var f = getMethod(self, methodName);

    if (f != null)
    {
        f.Invoke(self, new object[] { value });
        return true;
    }

    return false;
}

public static object invoke4(this object self, string methodName, object value)
{
    var f = getMethod(self, methodName);

    if (f != null)
        return f.Invoke(self, new object[] { value });
    else
        Debug.LogError("cannot find the methodName:" + methodName + "   self=" + self, self as Object);

    return null;
}*/



//------------------------------------------------------------------------------------
//如果返回true 就是有对象处理了这个事件
//可以把信息发送给本节点的所有脚本
/*public static bool sendMessage(this Component self, string methodName, params object[] values)
{
    //Debug.Log("self.name=" + self.name + "  methodName=" + methodName + "  values.Length=" + values.Length, self);

    var mbs = self.GetComponents<MonoBehaviour>();

    bool isInvoke = false;
    foreach (var mb in mbs)
    {
        var f = getMethod(mb, methodName);

        if (f != null)
        {
            var r = _invoke(mb, f, values);
            isInvoke = true;

            if (r == null)
                continue;
            var e = r as IEnumerator;
            if (e != null)
            {
                if (mb.gameObject.activeInHierarchy == false)
                    Debug.LogError("mb.gameObject.activeInHierarchy == false", mb);
                mb.StartCoroutine(e);
            }
        }
    }

    return isInvoke;
}


//如果调用的函数返回true 表示继续发送  返回false 停止信息传递
//返回值是void 表示继续发送
static bool dispatchMessage(this Component self, string methodName, params object[] values)
{
    //
    var mbs = self.GetComponents<MonoBehaviour>();

    foreach (var mb in mbs)
    {
#if UNITY_EDITOR
        if (mb == null) Debug.LogError("mb == null", self);
#endif
        var f = getMethod(mb, methodName);

        if (f != null)
        {
            var r = _invoke(mb, f, values);

            if (r == null)
                continue;

            var e = r as IEnumerator;
            if (e != null)
                mb.StartCoroutine(e);
            else if ((bool)r == false)
                return false;
        }
    }

    return true;
}

//with self
public static bool broadcastMessage(this Component self, string methodName, params object[] values)
{
    if (self.gameObject.activeInHierarchy)
    {
        var b = dispatchMessage(self, methodName, values);
        if (b == false)
            return b;

        foreach (Transform c in self.transform)
        {
            b = broadcastMessage(c, methodName, values);
            if (b == false)
                return b;
        }
    }

    return true;
}

//with self
public static void sendMessageUpwards(this Component self, string methodName, params object[] values)
{
    var b = dispatchMessage(self, methodName, values);
    if (b == false)
        return;

    var parent = self.transform.parent;
    if (parent != null)
        sendMessageUpwards(parent, methodName, values);
}*/














