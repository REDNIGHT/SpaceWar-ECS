using UnityEngine;
using System.Reflection;

namespace RN._Editor
{
    public interface RNDebug
    {
        void log(object message, Object context = null);
    }

    public class RNDebug_Log : RNDebug
    {
        public static RNDebug debug = new RNDebug_Log();
        public void log(object message, Object context = null)
        {
            Debug.Log(context + " -> " + message, context);
        }
    }
    public class RNDebug_LogWarning : RNDebug
    {
        public static RNDebug debug = new RNDebug_LogWarning();
        public void log(object message, Object context = null)
        {
            Debug.LogWarning(context + " -> " + message, context);
        }
    }
    public class RNDebug_LogError : RNDebug
    {
        public static RNDebug debug = new RNDebug_LogError();
        public void log(object message, Object context = null)
        {
            Debug.LogError(context + " -> " + message, context);
        }
    }

    public static class RNDebugEx
    {
        public static void logs(this MonoBehaviour self)
        {
            invoke(self, "onLogs", RNDebug_Log.debug);
        }
        public static void warnings(this MonoBehaviour self)
        {
            invoke(self, "onWarnings", RNDebug_LogWarning.debug);
        }
        public static void errors(this MonoBehaviour self)
        {
            invoke(self, "onErrors", RNDebug_LogError.debug);
        }


        //--------------------------------------------------------------------------------------------
        public static void invoke(Object self, string methodName, object o)
        {
            var f = getMethod(self, methodName);

            if (f != null)
                _invoke(self, f, o);
        }
        public static void invoke(Object self, string methodName)
        {
            var f = getMethod(self, methodName);

            if (f != null)
                _invoke(self, f);
        }
        static MethodInfo getMethod(this Object self, string methodName)
        {
            return self.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance /*| BindingFlags.FlattenHierarchy*/);
        }
        static object _invoke(Object self, MethodInfo f, params object[] values)
        {
#if UNITY_EDITOR
            try
            {
                return f.Invoke(self, values);
            }
            catch (System.ArgumentException exc)
            {
                Debug.LogError(exc.Message + "  fun=" + self + "." + f.Name, self as MonoBehaviour);
                return null;
            }
            /*catch (System.Reflection.TargetInvocationException exc)
            {
                Debug.LogError(exc.Message + "  fun=" + obj + "." + f.Name, obj as MonoBehaviour);
                return null;
            }*/
            catch (System.Reflection.TargetParameterCountException exc)
            {
                Debug.LogError(exc.Message + "  fun=" + self + "." + f.Name, self as MonoBehaviour);
                return null;
            }
            catch (System.MethodAccessException exc)
            {
                Debug.LogError(exc.Message + "  fun=" + self + "." + f.Name, self as MonoBehaviour);
                return null;
            }
            catch (System.InvalidOperationException exc)
            {
                Debug.LogError(exc.Message + "  fun=" + self + "." + f.Name, self as MonoBehaviour);
                return null;
            }
#else
            return f.Invoke(self, values);
#endif
        }

    }
}