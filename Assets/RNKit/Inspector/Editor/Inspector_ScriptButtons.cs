using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RN._Editor
{
    //
    partial class Inspector
    {
        //
        protected void scriptButton()
        {
            if (GUILayout.Button("script", buttonStyle, GUILayout.ExpandWidth(false)))
                openScript();
        }

        protected bool openScript(string scriptName, System.Type type)
        {
            bool find = false;
            var assets = AssetDatabase.FindAssets(scriptName);
            foreach (var asset in assets)
            {
                var p = AssetDatabase.GUIDToAssetPath(asset);
                var script = (MonoScript)AssetDatabase.LoadAssetAtPath<MonoScript>(p);
                if (script != null
                && script.name == scriptName
                && (type == null || type == script.GetClass()))
                {
                    if (Time.realtimeSinceStartup - selectTime < 0.32f)
                        AssetDatabase.OpenAsset(script);
                    else
                        EditorGUIUtility.PingObject(script);

                    selectTime = Time.realtimeSinceStartup;
                    break;
                }
            }

            return find;
        }
        protected void openScript()
        {
            SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
            MonoScript targetScript = scriptProperty.objectReferenceValue as MonoScript;

            if (Time.realtimeSinceStartup - selectTime < 0.32f)
                AssetDatabase.OpenAsset(targetScript);
            else
                EditorGUIUtility.PingObject(targetScript);

            selectTime = Time.realtimeSinceStartup;
        }


        //
        static bool _moreScriptToggle = false;
        protected void moreScriptToggle()
        {
            _moreScriptToggle = GUILayout.Toggle(_moreScriptToggle, "m", buttonStyle/*dropDownStyle*/, GUILayout.ExpandWidth(false));
        }
        static bool _hasCopy = false;
        protected void moreScriptBottons()
        {
            if (_moreScriptToggle)
            {
                using (horizontal())
                {
                    EditorGUILayout.Separator();

                    if (GUILayout.Button("editor script", buttonStyle, GUILayout.ExpandWidth(false)))
                    {
                        if (openScript(target.GetType().Name + "Editor", null) == false)
                            if (openScript(target.GetType().Name + "Inspector", null) == false)
                                if (openScript(target.GetType().BaseType.Name + "Editor", null) == false)
                                    openScript(target.GetType().BaseType.Name + "Inspector", null);
                    }
                    if (GUILayout.Button("up", buttonStyle, GUILayout.ExpandWidth(false)))
                    {
                        UnityEditorInternal.ComponentUtility.MoveComponentUp(target as Component);
                    }
                    if (GUILayout.Button("down", buttonStyle, GUILayout.ExpandWidth(false)))
                    {
                        UnityEditorInternal.ComponentUtility.MoveComponentDown(target as Component);
                    }
                    if (GUILayout.Button("copy", buttonStyle, GUILayout.ExpandWidth(false)))
                    {
                        _hasCopy = UnityEditorInternal.ComponentUtility.CopyComponent(target as Component);
                    }

                    if (_hasCopy)
                    {
                        if (GUILayout.Button("paste new", buttonStyle, GUILayout.ExpandWidth(false)))
                        {
                            UnityEditorInternal.ComponentUtility.PasteComponentAsNew((target as Component).gameObject);
                            _hasCopy = false;
                        }
                        if (GUILayout.Button("paste value", buttonStyle, GUILayout.ExpandWidth(false)))
                        {
                            UnityEditorInternal.ComponentUtility.PasteComponentValues(target as Component);
                            _hasCopy = false;
                        }
                    }
                }
            }
        }
    }
}