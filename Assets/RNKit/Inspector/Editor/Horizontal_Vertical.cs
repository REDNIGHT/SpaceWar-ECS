using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RN._Editor
{
    //
    partial class Inspector
    {
        public static System.IDisposable horizontal() { return new Horizontal(); }
        public static System.IDisposable vertical() { return new Vertical(); }
    }

    //
    public class Horizontal : System.IDisposable
    {
        public Horizontal()
        {
            EditorGUILayout.BeginHorizontal();
        }

        public void Dispose()
        {
            EditorGUILayout.EndHorizontal();
        }
    }
    public class Vertical : System.IDisposable
    {
        public Vertical()
        {
            EditorGUILayout.BeginVertical();
        }

        public void Dispose()
        {
            EditorGUILayout.EndVertical();
        }
    }

}