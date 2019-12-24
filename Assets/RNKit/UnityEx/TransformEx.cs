using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformEx
{
    public static void setLocalPositionX(this Transform t, float x)
    {
        var pos = t.localPosition;
        pos.x = x;
        t.localPosition = pos;
    }
    public static void setLocalPositionY(this Transform t, float y)
    {
        var pos = t.localPosition;
        pos.y = y;
        t.localPosition = pos;
    }
    public static void setLocalPositionZ(this Transform t, float z)
    {
        var pos = t.localPosition;
        pos.z = z;
        t.localPosition = pos;
    }


    public static void setPositionX(this Transform t, float x)
    {
        var pos = t.position;
        pos.x = x;
        t.position = pos;
    }
    public static void setPositionY(this Transform t, float y)
    {
        var pos = t.position;
        pos.y = y;
        t.position = pos;
    }
    public static void setPositionZ(this Transform t, float z)
    {
        var pos = t.position;
        pos.z = z;
        t.position = pos;
    }


    //
    public static void setLocalEulerAnglesX(this Transform t, float x)
    {
        var a = t.localEulerAngles;
        a.x = x;
        t.localEulerAngles = a;
    }
    public static void setLocalEulerAnglesY(this Transform t, float y)
    {
        var a = t.localEulerAngles;
        a.y = y;
        t.localEulerAngles = a;
    }
    public static void setLocalEulerAnglesZ(this Transform t, float z)
    {
        var a = t.localEulerAngles;
        a.z = z;
        t.localEulerAngles = a;
    }


    public static void setEulerAnglesX(this Transform t, float x)
    {
        var a = t.eulerAngles;
        a.x = x;
        t.eulerAngles = a;
    }
    public static void setEulerAnglesY(this Transform t, float y)
    {
        var a = t.eulerAngles;
        a.y = y;
        t.eulerAngles = a;
    }
    public static void setEulerAnglesZ(this Transform t, float z)
    {
        var a = t.eulerAngles;
        a.z = z;
        t.eulerAngles = a;
    }


    //
    public static void setLocalScaleX(this Transform t, float x)
    {
        var s = t.localScale;
        s.x = x;
        t.localScale = s;
    }
    public static void setLocalScaleY(this Transform t, float y)
    {
        var s = t.localScale;
        s.y = y;
        t.localScale = s;
    }
    public static void setLocalScaleZ(this Transform t, float z)
    {
        var s = t.localScale;
        s.z = z;
        t.localScale = s;
    }



    //
    public static void setAnchoredPositionX(this RectTransform t, float x)
    {
        var pos = t.anchoredPosition3D;
        pos.x = x;
        t.anchoredPosition3D = pos;
    }
    public static void setAnchoredPositionY(this RectTransform t, float y)
    {
        var pos = t.anchoredPosition3D;
        pos.y = y;
        t.anchoredPosition3D = pos;
    }
    public static void setAnchoredPositionZ(this RectTransform t, float z)
    {
        var pos = t.anchoredPosition3D;
        pos.z = z;
        t.anchoredPosition3D = pos;
    }


    //
    static public Rect getWorldRect(this RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        var r = new Rect(rt.position, Vector2.zero);
        foreach (var c in corners)
            r = r.encapsulate(c);
        return r;
    }
}