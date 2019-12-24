using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class TextureEx
{
    public static void clear(this RenderTexture renderTexture, Color color)
    {
        RenderTexture last = RenderTexture.active;
        RenderTexture.active = renderTexture;

        GL.Clear(false, true, color);

        RenderTexture.active = last;
    }
    public static void clearDepth(this RenderTexture renderTexture)
    {
        RenderTexture last = RenderTexture.active;
        RenderTexture.active = renderTexture;

        GL.Clear(true, false, Color.clear);

        RenderTexture.active = last;
    }

    public static Texture2D newTempTexture2D(int width, int height)
    {
        var temp = new Texture2D(width, height, TextureFormat.RGBA32, false);
        temp.hideFlags = HideFlags.HideAndDontSave;
        return temp;
    }
    public static void load(this RenderTexture renderTexture, string path)
    {
        if (File.Exists(path))
        {
            Texture2D temp = newTempTexture2D(renderTexture.width, renderTexture.height);

            if (temp.load(path))
                Graphics.Blit(temp, renderTexture);

            Object.Destroy(temp);
            return;
        }

        Debug.LogError("File is not exist!  path=" + path);
        return;
    }


    public static byte[] getData(this Texture2D texture, string path, int jpgQuality)
    {
        byte[] data = null;
        if (path.IndexOf(".jpg") > 0 || path.IndexOf(".jpeg") > 0)
            data = texture.EncodeToJPG(jpgQuality);
        else if (path.IndexOf(".png") > 0)
            data = texture.EncodeToPNG();
        else
            data = texture.GetRawTextureData();

        return data;
    }

    public static bool setData(this Texture2D texture, string path, byte[] data)
    {
        if (path.IndexOf(".jpg") > 0 || path.IndexOf(".jpeg") > 0 || path.IndexOf(".png") > 0)
        {
            if (texture.LoadImage(data))
            {
                return true;
            }
            else
            {
                Debug.LogError("LoadImage fail!  path=" + path);
                return false;
            }
        }
        else
        {
            texture.LoadRawTextureData(data);
            texture.Apply();

            return true;
        }
    }

    public static bool load(this Texture2D texture, string path)
    {
        if (File.Exists(path))
        {
            return texture.setData(path, File.ReadAllBytes(path));
        }

        Debug.LogError("File is not exist!  path=" + path);
        return false;
    }

    public static IEnumerator save(this RenderTexture renderTexture, string path, int jpgQuality)
    {
        //Debug.Log("save  path="+ path, renderTexture);
        yield return new WaitForEndOfFrame();
        RenderTexture last = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D temp = newTempTexture2D(renderTexture.width, renderTexture.height);
        temp.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);

        byte[] data = temp.getData(path, jpgQuality);

        /*var task = new RN.Task(() =>
        {
            File.WriteAllBytes(path, data);
        }).startInShareThread();

        while (task.wait)
            yield return null;

        if (task.isFaulted)
        {
            Debug.LogError("task.isFaulted  exception=" + task.exception);
        }*/
        File.WriteAllBytes(path, data);

        Object.Destroy(temp);
        RenderTexture.active = last;
    }

    public static void save(this Texture2D texture, string path, int jpgQuality)
    {
        File.WriteAllBytes(path, texture.getData(path, jpgQuality));
    }


    //
    public static Texture2D getTexture2D(this RenderTexture renderTexture)
    {
        RenderTexture last = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D temp = newTempTexture2D(renderTexture.width, renderTexture.height);
        temp.name = renderTexture.name;
        temp.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
        temp.Apply();

        RenderTexture.active = last;

        return temp;
    }
    public static Texture2D getTexture2D(this RenderTexture renderTexture, Texture2D texture)
    {
        RenderTexture last = RenderTexture.active;
        RenderTexture.active = renderTexture;

        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
        texture.Apply();

        RenderTexture.active = last;

        return texture;
    }

    public static Color[] getPixels(this RenderTexture renderTexture)
    {
        RenderTexture last = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D temp = newTempTexture2D(renderTexture.width, renderTexture.height);
        temp.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
        temp.Apply();
        var ps = temp.GetPixels();

        Object.Destroy(temp);
        RenderTexture.active = last;

        return ps;
    }

    public static Color32[] getPixels32(this RenderTexture renderTexture)
    {
        RenderTexture last = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D temp = newTempTexture2D(renderTexture.width, renderTexture.height);
        temp.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
        temp.Apply();
        var ps = temp.GetPixels32();

        Object.Destroy(temp);
        RenderTexture.active = last;

        return ps;
    }


    public static byte[] encodeToJPG(this RenderTexture renderTexture, int quality)
    {
        RenderTexture last = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D temp = newTempTexture2D(renderTexture.width, renderTexture.height);
        temp.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
        temp.Apply();
        var ps = temp.EncodeToJPG(quality);

        Object.Destroy(temp);
        RenderTexture.active = last;

        return ps;
    }


    //需要在之前调用    yield return new WaitForEndOfFrame();
    public static void setPixels(this RenderTexture renderTexture, Color[] ps)
    {
        Texture2D temp = newTempTexture2D(renderTexture.width, renderTexture.height);

        temp.SetPixels(ps);
        temp.Apply();

        Graphics.Blit(temp, renderTexture);

        Object.Destroy(temp);
    }

}