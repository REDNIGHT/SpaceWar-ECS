using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RN._Editor
{
    //
    partial class Inspector
    {
        public System.IDisposable area() { return new Area(this); }
    }

    //
    public class Area : System.IDisposable
    {
        static GUIStyle _boxStyle;
        public static GUIStyle boxStyle
        {
            get
            {
                if (_boxStyle == null)
                {
                    _boxStyle = GUI.skin.FindStyle("box");
                }
                return _boxStyle;
            }
        }

        public Area(Inspector editor)
        {
            EditorGUILayout.BeginHorizontal();
            //editor.onButtons("", 0, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            separatorLine();

            
            EditorGUILayout.BeginVertical(boxStyle);
            //++EditorGUI.indentLevel;
        }

        public void Dispose()
        {
            //editor.onButtons("", 1);
            //--EditorGUI.indentLevel;
            EditorGUILayout.EndVertical();

            separator();
            //EditorGUILayout.Separator();
        }


        //
        static Color SeparatorLineColor = Color.black * 0.32f;
        public static void separatorLine()
        {
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), SeparatorLineColor);
        }
        public static void separator()
        {
            GUILayout.Space(6f);
        }
        /*public static void horizontalSpace()
        {
            GUILayout.Space(15f);
        }*/
    }

}
