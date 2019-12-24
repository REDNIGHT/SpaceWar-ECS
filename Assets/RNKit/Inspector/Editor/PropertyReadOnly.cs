using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RN._Editor
{
    //
    partial class Inspector
    {
        public static System.IDisposable propertyReadOnly() { return new PropertyReadOnly(); }
    }

    public class PropertyReadOnly : System.IDisposable
    {
        public PropertyReadOnly()
        {
            GUI.enabled = false;
        }
        public void Dispose()
        {
            GUI.enabled = true;
        }
    }
}
