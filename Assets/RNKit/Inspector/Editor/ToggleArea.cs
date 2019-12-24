using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RN._Editor
{
    //
    partial class Inspector
    {
        public ToggleArea curToggleArea = null;
        public bool toggleAreaOpen { get { return curToggleArea.open; } }
        public ToggleArea toggleArea(string areaName) { return new ToggleArea(areaName, this); }
    }

    //
    public class ToggleArea : System.IDisposable
    {
        static GUIStyle _headerStyle;
        public static GUIStyle headerStyle
        {
            get
            {
                if (_headerStyle == null)
                {
                    _headerStyle = new GUIStyle(EditorStyles.label);
                    //_headerStyle = new GUIStyle(EditorStyles.foldout);
                    //_headerStyle.fontStyle = FontStyle.Bold;
                    //_headerStyle.alignment = TextAnchor.MiddleLeft;
                    _headerStyle.fontSize = 11;
                    //_headerStyle.normal.textColor = new Color32(255, 0, 0, 255);
                }
                return _headerStyle;
            }
        }


        public bool open = true;
        protected Inspector _editor;
        protected string _areaName;
        public ToggleArea(string areaName, Inspector editor)
        {
            //
            _editor = editor;
            _areaName = areaName;
            //if (_editor.curToggleArea != null)
            //    Debug.LogError("_editor.curArea != null", editor.target);
            _editor.curToggleArea = this;

            //
            var key = editor.target.GetType() + areaName;
            open = EditorPrefs.GetBool(key, true);

            begin(areaName);

            EditorPrefs.SetBool(key, open);
        }

        public void Dispose()
        {
            _editor.curToggleArea = null;

            end(_areaName);
        }

        protected void begin(string areaName)
        {
            using (Inspector.horizontal())
            {
                var pos = GUILayoutUtility.GetRect(0.0f, 16.0f);

                open = GUI.Toggle(pos, open, new GUIContent(areaName), headerStyle);
                //open = EditorGUILayout.Toggle(areaName, open, headerStyle);
                //open = EditorGUILayout.Foldout(open, new GUIContent(areaName), headerStyle);


                EditorGUILayout.Separator();
                _editor.onButtons(areaName, 0, GUILayout.ExpandWidth(false));
            }

            Area.separatorLine();
            if (open)
            {
                //画窗口需要下面一行代码
                EditorGUILayout.BeginVertical(Area.boxStyle);
                //++EditorGUI.indentLevel;
            }
        }

        protected void end(string areaName)
        {
            if (open)
            {
                for (var i = 0; i < ButtonAttribute.maxBottonButtonCount; ++i)
                    using (Inspector.horizontal())
                        _editor.onButtons(areaName, i + 1);

                //--EditorGUI.indentLevel;
                EditorGUILayout.EndVertical();
            }

            Area.separator();
            //GUILayout.Space(4f);
        }
    }

}