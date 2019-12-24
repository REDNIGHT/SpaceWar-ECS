using System;

namespace RN._Editor
{
    [AttributeUsageAttribute(AttributeTargets.Field, AllowMultiple = false)]
    public class ToggleAreaAttribute : Attribute
    {
        public readonly string header;

        public ToggleAreaAttribute(string h)
        {
            header = h;
        }
    }
}