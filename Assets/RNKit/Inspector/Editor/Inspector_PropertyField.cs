using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RN._Editor
{
    //
    partial class Inspector
    {
        //
        protected bool hasProperty(string propertyName)
        {
            return propertyNames.Contains(propertyName);
        }
		
        protected SerializedProperty dequeueProperty(string propertyName)
        {
            if (propertyNames.Remove(propertyName) == false)
            {
                Debug.LogError("propertyNames.Remove(propertyName) == false  propertyName=" + propertyName, this);
                return null;
            }

            if (curToggleArea != null && curToggleArea.open == false)
                return null;

            return serializedObject.FindProperty(propertyName);
        }


        //
        /*protected void enumPopup(string propertyName, System.Enum selected, float width = 12.0f)
        {
            if (propertyEnable(propertyName))
                dequeueProperty(propertyName).enumValueIndex
                = System.Convert.ToInt32(EditorGUILayout.EnumPopup(selected, EditorStyles.toolbarPopup, GUILayout.Width(width)));
        }*/
        protected void propertyField(string propertyName, params GUILayoutOption[] options)
        {
            var p = dequeueProperty(propertyName);
            if (p != null)
                EditorGUILayout.PropertyField(p, true, options);
        }

        protected void propertyPopup(string propertyName, string[] displayedOptions, params GUILayoutOption[] options)
        {
            var p = dequeueProperty(propertyName);
            if (p != null)
            {
                using (propertyDrity(p))
                    p.intValue = EditorGUILayout.Popup(p.displayName, p.intValue, displayedOptions, options);
            }
        }
        protected void propertyMaskField(string propertyName, string[] displayedOptions, params GUILayoutOption[] options)
        {
            var p = dequeueProperty(propertyName);
            if (p != null)
            {
                using (propertyDrity(p))
                    p.intValue = EditorGUILayout.MaskField(p.name, p.intValue, displayedOptions, options);
            }
        }
        protected void propertyMinMaxSlider(string label, string minPropertyName, string maxPropertyName, float minLimit, float maxLimit, params GUILayoutOption[] options)
        {
            var minP = dequeueProperty(minPropertyName);
            var maxP = dequeueProperty(maxPropertyName);
            if (minP != null)
            {
                using (horizontal())
                {
                    var min = minP.floatValue;
                    var max = maxP.floatValue;

                    using (propertyDrity(minP))
                    {
                        using (propertyDrity(maxP))
                        {
                            EditorGUILayout.MinMaxSlider(new GUIContent(label), ref min, ref max, minLimit, maxLimit, options);

                            using (indentLevel_0())
                            {
                                EditorGUILayout.FloatField(min, GUILayout.MaxWidth(38f));
                                EditorGUILayout.FloatField(max, GUILayout.MaxWidth(38f));
                            }
                        }
                    }

                    minP.floatValue = min;
                    maxP.floatValue = max;
                }
            }
        }

        protected void arrayPropertyField(string propertyName, System.Action<SerializedProperty> handler, params GUILayoutOption[] options)
        {
            var arrayP = dequeueProperty(propertyName);
            if (arrayP != null)
            {
                using (propertyDrity(arrayP))
                {
                    //
                    var key = target.GetType() + propertyName;
                    var open = EditorPrefs.GetBool(key, false);

#if false
                    //open = EditorGUILayout.Foldout(open, arrayP.displayName + (open ? "" : "        Size  " + arrayP.arraySize), EditorStyles.foldout);
                    open = GUILayout.Toggle(open, arrayP.displayName + (open ? "" : "        Size  " + arrayP.arraySize),  EditorStyles.foldout);
#else
                    var w = 20f;
                    var h = 16f;
                    var pos = GUILayoutUtility.GetRect(0.0f, 16.0f);
                    pos.x -= 11;
                    //open = EditorGUI.Foldout(pos, open, arrayP.displayName + (open ? "" : "        Size  " + arrayP.arraySize), EditorStyles.foldout);
                    //open = EditorGUI.Toggle(pos, arrayP.displayName + (open ? "" : "        Size  " + arrayP.arraySize), open, EditorStyles.foldout);
                    open = GUI.Toggle(pos, open, arrayP.displayName + (open ? "" : "        Size  " + arrayP.arraySize),  EditorStyles.foldout);
#endif
                    EditorPrefs.SetBool(key, open);

                    if (open)
                    {
                        using (indentLevel())
                        {
#if false
                            arrayP.arraySize = EditorGUILayout.IntField("Size", arrayP.arraySize);
#else
                            pos = GUILayoutUtility.GetRect(0, h);
                            arrayP.arraySize = EditorGUI.IntField(pos, "Size", arrayP.arraySize);
#endif
                            var index = 0;
                            while (index < arrayP.arraySize)
                            {
                                using (horizontal())
                                {
                                    var ae = arrayP.GetArrayElementAtIndex(index);
                                    handler(ae);
#if false
                                    if (GUILayout.Button("+"))
                                        arrayP.InsertArrayElementAtIndex(index);
                                    
                                    if (GUILayout.Button("-"))
                                        arrayP.DeleteArrayElementAtIndex(index);
#else
                                    pos = GUILayoutUtility.GetRect(w, h, GUILayout.Width(w), GUILayout.Height(h));
                                    pos.y += 1f;
                                    if (GUI.Button(pos, "+"))
                                    {
                                        arrayP.InsertArrayElementAtIndex(index);
                                    }

                                    pos = GUILayoutUtility.GetRect(w, h, GUILayout.Width(w), GUILayout.Height(h));
                                    pos.y += 1f;
                                    if (GUI.Button(pos, "-"))
                                    {
                                        arrayP.DeleteArrayElementAtIndex(index);
                                    }
#endif

                                }

                                ++index;
                            }
                        }
                    }
                }
            }
        }



        //---------------------------------------------------------------------------------------------------------------------------------------
        protected static float selectTime = 0f;
        protected void pingOrSelect(Object o)
        {
            if (Time.realtimeSinceStartup - selectTime < 0.32f)
                Selection.activeObject = o;
            else
                EditorGUIUtility.PingObject(o);

            selectTime = Time.realtimeSinceStartup;
        }
    }
}