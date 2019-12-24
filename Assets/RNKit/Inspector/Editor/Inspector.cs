using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RN._Editor
{
    //
    public partial class Inspector : Editor
    {
        protected struct RNButtonAttributeInfo
        {
            public ButtonAttribute buttonAttribute;
            public MethodInfo methodInfo;
        }
        protected List<RNButtonAttributeInfo> buttonAttributeInfos = new List<RNButtonAttributeInfo>();

        protected void OnEnable()
        {
            buttonAttributeInfos.Clear();

            var ms = getAllMethods(target.GetType());
            foreach (var m in ms)
            {
                var bas = m.GetCustomAttributes(typeof(ButtonAttribute), true);
                foreach (ButtonAttribute ba in bas)
                {
                    buttonAttributeInfos.Add(new RNButtonAttributeInfo { buttonAttribute = ba, methodInfo = m });
                }
            }

            onDebugButtonState(true);
        }
        protected void OnDisable()
        {
            onDebugButtonState(false);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();
            using (horizontal())
            {
                onButtons(ButtonAttribute.BeginArea, 0, GUILayout.ExpandWidth(false));

                EditorGUILayout.Separator();

                onButtons(ButtonAttribute.BeginArea, 1, GUILayout.ExpandWidth(false));

                GUILayout.Space(4);

                moreScriptToggle();
                debugButton();
                scriptButton();
            }

            //
            moreScriptBottons();

            //
            onDebugInfo();
            onDebug();

            //
            using (area())
            //Area.separatorLine();
            {
                onInspectorGUIBegin();
                onInspectorGUI();
                onInspectorGUIEnd();
            }

            //
            for (var i = 0; i < ButtonAttribute.maxBottonButtonCount; ++i)
                using (Inspector.horizontal())
                    onButtons(ButtonAttribute.EndArea, i);

            Area.separator();
        }

        public virtual void onButtons(string areaName, int position, params GUILayoutOption[] options)
        {
            foreach (var bai in buttonAttributeInfos)
            {
                var ba = bai.buttonAttribute;
                var methodInfo = bai.methodInfo;
                if (ba._areaName == areaName && ba._position == position)
                {
                    if (GUILayout.Button(methodInfo.Name, buttonStyle, options /*GUILayout.ExpandWidth(false)*/ ))
                    {
                        foreach (var t in targets)
                        {
                            var e = methodInfo.Invoke(t, null);
                            var ie = e as IEnumerator;
                            if (ie != null)
                            {
                                var m = t as MonoBehaviour;
                                m.StartCoroutine(ie);
                            }
                        }
                    }
                }
            }
        }

        protected static List<string> propertyNames = new List<string>();
        protected bool hasToggleArea = false;
        protected void onInspectorGUIBegin()
        {
            serializedObject.Update();

            //
            propertyNames.Clear();

#if true
            foreach (var field in getAllFields(target.GetType(), true, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (field.FieldType.IsInterface)
                    continue;

                if (field.FieldType.IsSubclassOf(typeof(System.Delegate))) //delegate
                    continue;

                if (field.FieldType.IsSubclassOf(typeof(YieldInstruction)))
                    continue;

                if (field.FieldType.IsPublic == false && field.FieldType.IsSerializable == false) //class
                    continue;

                if (field.IsPublic && field.GetCustomAttributes(typeof(System.NonSerializedAttribute), true).Length > 0)
                    continue;

                if (field.IsPublic == false && field.GetCustomAttributes(typeof(SerializeField), true).Length == 0)
                    continue;

                /*{
                    var t = field.FieldType;
                    Debug.Log(t.Name
                        + "\n IsAbstract=" + t.IsAbstract
                        + "\n IsAnsiClass=" + t.IsAnsiClass
                        + "\n IsAutoClass=" + t.IsAutoClass
                        + "\n IsAutoLayout=" + t.IsAutoLayout
                        + "\n IsByRef=" + t.IsByRef
                        + "\n IsClass=" + t.IsClass
                        + "\n IsCOMObject=" + t.IsCOMObject
                        + "\n IsContextful=" + t.IsContextful
                        + "\n IsExplicitLayout=" + t.IsExplicitLayout
                        + "\n IsGenericParameter=" + t.IsGenericParameter
                        + "\n IsGenericType=" + t.IsGenericType
                        + "\n IsGenericTypeDefinition=" + t.IsGenericTypeDefinition
                        + "\n IsImport=" + t.IsImport
                        + "\n IsInterface=" + t.IsInterface
                        + "\n IsLayoutSequential=" + t.IsLayoutSequential
                        + "\n IsMarshalByRef=" + t.IsMarshalByRef
                        + "\n IsNestedAssembly=" + t.IsNestedAssembly
                        + "\n IsNestedFamANDAssem=" + t.IsNestedFamANDAssem
                        + "\n IsNestedFamily=" + t.IsNestedFamily
                        + "\n IsNestedFamORAssem=" + t.IsNestedFamORAssem
                        + "\n IsNestedPrivate=" + t.IsNestedPrivate
                        + "\n IsNestedPublic=" + t.IsNestedPublic
                        + "\n IsNotPublic=" + t.IsNotPublic
                        + "\n IsPointer=" + t.IsPointer
                        + "\n IsPrimitive=" + t.IsPrimitive
                        + "\n IsPublic=" + t.IsPublic
                        + "\n IsSealed=" + t.IsSealed
                        + "\n IsSerializable=" + t.IsSerializable
                        + "\n IsSpecialName=" + t.IsSpecialName
                        + "\n IsUnicodeClass=" + t.IsUnicodeClass
                        + "\n IsValueType=" + t.IsValueType
                        + "\n IsVisible=" + t.IsVisible
                        + "\n MemberType=" + t.MemberType
                        + "\n BaseType=" + t.BaseType
                        );
                }*/

                if (ignoreField(field.Name))
                    continue;

                propertyNames.Add(field.Name);
                if (string.IsNullOrEmpty(getToggleAreaName(field.Name)) == false)
                    hasToggleArea = true;
            }
#else
            SerializedProperty it = serializedObject.GetIterator ();
            it.Next (true);
            do {
                if (it.name.IndexOf ("m_") == 0)
                    continue;
                if (ignoreField(it.name))
                    continue;

                propertyNames.Add (it.name);

                if (string.IsNullOrEmpty (getToggleAreaName (it.name)) == false)
                    hasToggleArea = true;

            } while (it.Next (false));

            foreach (var field in getAllFields (target.GetType (), true, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (field.GetCustomAttributes (typeof (SerializeField), true).Length > 0)
                {
                    var fn = serializedObject.FindProperty (field.Name).name;
                    if (ignoreField(fn))
                        continue;

                    propertyNames.Add (fn);
                }
            }
#endif
        }
        protected virtual bool ignoreField(string fName)
        {
            return false;
        }

        protected virtual void onInspectorGUI() { }

        protected void onInspectorGUIEnd()
        {
            System.IDisposable endArea = null;
            if (hasToggleArea == false && propertyNames.Count > 0)
                endArea = area();

            ToggleArea endToggleArea = null;

            foreach (var p in propertyNames)
            {
                var property = serializedObject.FindProperty(p);

                if (property == null)
                {
                    helpBox(p, target.GetType().GetField(p).FieldType);
                    continue;
                }
                var headerName = getToggleAreaName(property.name);
                if (headerName != null)
                {
                    if (endToggleArea != null)
                        endToggleArea.Dispose();

                    endToggleArea = toggleArea(headerName);
                }

                if (endToggleArea != null && endToggleArea.open == false)
                    continue;

                //var indentLevel = property.isArray || property.hasVisibleChildren;
                //if (indentLevel) ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(property, true);
                //if (indentLevel) --EditorGUI.indentLevel;
            }

            if (endToggleArea != null)
            {
                endToggleArea.Dispose();
                endToggleArea = null;
            }

            if (endArea != null)
                endArea.Dispose();

            if (endToggleArea != null && endArea != null)
                Debug.LogError("endToggleArea != null && endArea != null", this);

            //
            serializedObject.ApplyModifiedProperties();
        }
        protected string getToggleAreaName(string propertyName)
        {
            var info = target.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance /*| BindingFlags.FlattenHierarchy*/ );
            if (info != null)
            {
                var hs = info.GetCustomAttributes(typeof(ToggleAreaAttribute), false);
                foreach (ToggleAreaAttribute h in hs)
                    return h.header;
            }
            return null;
        }

        //-----------------------------------------------------------------------------------------------
        GUIStyle _buttonStyle = null;
        protected GUIStyle buttonStyle
        {
            get
            {
                if (_buttonStyle == null)
                {
                    _buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
                    _buttonStyle.fixedHeight = 16;
                }
                return _buttonStyle;
            }
        }
        GUIStyle _dropDownStyle = null;
        protected GUIStyle dropDownStyle
        {
            get
            {
                if (_dropDownStyle == null)
                {
                    _dropDownStyle = new GUIStyle(EditorStyles.toolbarDropDown);
                    _dropDownStyle.fixedHeight = 16;
                }
                return _dropDownStyle;
            }
        }

        //-----------------------------------------------------------------------------------------------
        static IEnumerable<MethodInfo> getAllMethods(System.Type t)
        {
            if (t == null || t == typeof(MonoBehaviour) || t == typeof(ScriptableObject))
                return Enumerable.Empty<MethodInfo>();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            return t.GetMethods(flags).Concat(getAllMethods(t.BaseType));
        }
        static IEnumerable<FieldInfo> getAllFields(System.Type t, bool baseType, BindingFlags flags)
        {
            if (t == null || t == typeof(MonoBehaviour) || t == typeof(ScriptableObject))
                return Enumerable.Empty<FieldInfo>();

            var fs = t.GetFields(flags);
            if (baseType)
                return fs.Concat(getAllFields(t.BaseType, baseType, flags));
            else
                return fs;
        }
        static IEnumerable<PropertyInfo> getAllPropertys(System.Type t, bool baseType, BindingFlags flags)
        {
            if (t == null || t == typeof(MonoBehaviour) || t == typeof(ScriptableObject))
                return Enumerable.Empty<PropertyInfo>();

            var ps = t.GetProperties(flags);
            if (baseType)
                return ps.Concat(getAllPropertys(t.BaseType, baseType, flags));
            else
                return ps;
        }

        //-----------------------------------------------------------------------------------------------
        protected class InvokeInfo
        {
            public float delay;
            public System.Action action;

            public float time;
        }
        protected static InvokeInfo invokeInfo = null;
        protected static void invoke(float delay, System.Action action)
        {
            if (invokeInfo != null)
                Debug.Log("invokeInfo != null");
            invokeInfo = new InvokeInfo { delay = delay, action = action, time = Time.realtimeSinceStartup };
            EditorApplication.update += _invoke;
        }
        protected static void _invoke()
        {
            if (Time.realtimeSinceStartup >= invokeInfo.delay + invokeInfo.time)
            {
                invokeInfo.action();
                invokeInfo = null;
                EditorApplication.update -= _invoke;
            }
        }
    }
}