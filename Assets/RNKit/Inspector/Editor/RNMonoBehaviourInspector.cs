#if true
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RN._Editor
{
#if false
    [CustomEditor(typeof(RNMonoBehaviour), true), CanEditMultipleObjects]
    public class RNMonoBehaviourInspector : Inspector { }
#else
    [CustomEditor(typeof(/*RN*/MonoBehaviour), true), CanEditMultipleObjects]
    public class RNMonoBehaviourInspector : Inspector { }
#endif
}
#endif