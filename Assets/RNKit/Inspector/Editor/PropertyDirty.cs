using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RN._Editor
{
    //
    partial class Inspector
    {
        public static System.IDisposable propertyDrity(SerializedProperty property) { return new PropertyDirty(property); }
    }

    //
    public class PropertyDirty : System.IDisposable
    {
        int lastIndentLevel;
        public PropertyDirty(SerializedProperty property)
        {
            EditorGUI.BeginProperty(new Rect(), GUIContent.none, property);
        }

        public void Dispose()
        {
            EditorGUI.EndProperty();
        }
    }
}
