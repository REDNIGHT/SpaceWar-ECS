using UnityEngine;
using UnityEngine.UI;
#if TMPRO
using TMPro;
#endif

//namespace RN.UI//不加名字空间 避免使用时需要using

public static class TextEx
{
    //
    public static void autoSetText(this Text tm, string text)
    {
        tm.text = text;
    }
    public static void autoSetText(this TextMesh tm, string text)
    {
        tm.text = text;
    }
#if TMPRO
    public static void autoSetText(this TextMeshPro tm, string text)
    {
        tm.text = text;
    }
#endif

    //
    public static void autoSetText(this Component tm, string text)
    {
        //ugui crp
        var uitext = tm.GetComponentInChildren<Text>(true);
        if (uitext != null)
        {
            autoSetText(uitext, text);
            return;
        }

        //
        var textMesh = tm.GetComponentInChildren<TextMesh>(true);
        if (textMesh != null)
        {
            autoSetText(textMesh, text);
            return;
        }

#if TMPRO
        //
        var textMeshPro = tm.GetComponentInChildren<TextMeshPro>(true);
        if (textMeshPro != null)
        {
            setText(textMeshPro, text);
            return;
        }
#endif

        Debug.LogError("can not find text component!", tm);
    }


    public static void autoSetText(this Component tm, int i)
    {
        autoSetText(tm, i.ToString());
    }

    public static void autoSetText(this Component tm, float f)
    {
        autoSetText(tm, f.ToString());
    }

    public static string autoGetTextString(this Component tm)
    {
        var textMesh = tm.GetComponentInChildren<TextMesh>(true);
        if (textMesh != null)
            return textMesh.text;

#if TMPRO
        var textMeshPro = tm.GetComponentInChildren<TextMeshPro>(true);
        if (textMeshPro != null)
            return textMeshPro.text;
#endif

        var uitext = tm.GetComponentInChildren<Text>(true);
        if (uitext != null)
            return uitext.text;

        Debug.LogError("can not find text component!", tm);
        return "";
    }

    public static int autoGetText2Int(this Component tm)
    {
        return int.Parse(autoGetTextString(tm));
    }
    public static float autoGetText2Float(this Component tm)
    {
        return float.Parse(autoGetTextString(tm));
    }


    //
    public static void autoSetText(this Component c, string hierarchy, string text)
    {
        var t = c.transform.Find(hierarchy);
        if (t != null)
            t.autoSetText(text);
        else
            Debug.LogError("can not find hierarchy=" + hierarchy, c);
    }


    public static void autoSetText(this Component c, string hierarchy, int i)
    {
        var t = c.transform.Find(hierarchy);
        if (t != null)
            t.autoSetText(i.ToString());
        else
            Debug.LogError("can not find hierarchy=" + hierarchy, c);
    }

    public static void autoSetText(this Component c, string hierarchy, float f)
    {
        var t = c.transform.Find(hierarchy);
        if (t != null)
            t.autoSetText(f.ToString());
        else
            Debug.LogError("can not find hierarchy=" + hierarchy, c);
    }

    public static string autoGetTextString(this Component c, string hierarchy)
    {
        var t = c.transform.Find(hierarchy);
        if (t != null)
            return t.autoGetTextString();
        else
        {
            Debug.LogError("can not find hierarchy=" + hierarchy, c);
            return "";
        }
    }

    public static int autoGetText2Int(this Component c, string hierarchy)
    {
        var t = c.transform.Find(hierarchy);
        if (t != null)
            return int.Parse(t.autoGetTextString());
        else
        {
            Debug.LogError("can not find hierarchy=" + hierarchy, c);
            return 0;
        }
    }
    public static float autoGetText2Float(this Component c, string hierarchy)
    {
        var t = c.transform.Find(hierarchy);
        if (t != null)
            return float.Parse(t.autoGetTextString());
        else
        {
            Debug.LogError("can not find hierarchy=" + hierarchy, c);
            return 0f;
        }
    }


    //
    public static void autoSetTexts(this Component c, string[] text)
    {
        Debug.LogError("//todo...", c);
    }
    public static string[] autoGetTexts(this Component c)
    {
        Debug.LogError("//todo...", c);
        return null;
    }
}
