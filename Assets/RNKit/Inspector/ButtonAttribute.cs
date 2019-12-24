using System;

namespace RN._Editor
{
    [AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonAttribute : Attribute
    {
        public readonly string _areaName;
        public readonly int _position;
        public const int maxBottonButtonCount = 5;
        public const string BeginArea = "__BeginArea__";
        public const string EndArea = "__EndArea__";

        public ButtonAttribute(string areaName)
        {
            _areaName = areaName;
            _position = 0;
        }
        public ButtonAttribute(string areaName, int position)
        {
            _areaName = areaName;
            _position = position;
        }
    }

    [AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonInBeginLeftAreaAttribute : ButtonAttribute
    {
        public ButtonInBeginLeftAreaAttribute()
            : base(ButtonAttribute.BeginArea, 0)
        {
        }
    }
    [AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonInBeginRightAreaAttribute : ButtonAttribute
    {
        public ButtonInBeginRightAreaAttribute()
            : base(ButtonAttribute.BeginArea, 1)
        {
        }
    }

    [AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonInEndAreaAttribute : ButtonAttribute
    {
        public ButtonInEndAreaAttribute()
            : base(ButtonAttribute.EndArea, 0)
        {
        }
    }

    [AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonInEndArea1Attribute : ButtonAttribute
    {
        public ButtonInEndArea1Attribute()
            : base(ButtonAttribute.EndArea, 1)
        {
        }
    }

    [AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonInEndArea2Attribute : ButtonAttribute
    {
        public ButtonInEndArea2Attribute()
            : base(ButtonAttribute.EndArea, 2)
        {
        }
    }
    [AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonInEndArea3Attribute : ButtonAttribute
    {
        public ButtonInEndArea3Attribute()
            : base(ButtonAttribute.EndArea, 3)
        {
        }
    }
    [AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonInEndArea4Attribute : ButtonAttribute
    {
        public ButtonInEndArea4Attribute()
            : base(ButtonAttribute.EndArea, 4)
        {
        }
    }
}
