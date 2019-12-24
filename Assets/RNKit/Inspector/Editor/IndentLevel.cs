using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RN._Editor
{
    //
    partial class Inspector
    {
        public static System.IDisposable indentLevel() { return new IndentLevel(); }
        public static System.IDisposable indentLevel_0() { return new IndentLevel_0(); }
    }

    //
    public class IndentLevel_0 : System.IDisposable
    {
        int lastIndentLevel;
        public IndentLevel_0()
        {
            lastIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel = lastIndentLevel;
        }
    }

    public class IndentLevel : System.IDisposable
    {
        public IndentLevel()
        {
            ++EditorGUI.indentLevel;
        }

        public void Dispose()
        {
            --EditorGUI.indentLevel;
        }
    }
}