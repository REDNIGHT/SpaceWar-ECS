using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GraphicsEx
{
    //
    public static Material blitMaterial;


    //
    public static void blit(Texture source, float source_a, RenderTexture dest)
    {
        if (source == dest)
        {
            Debug.LogError("from == to");
            return;//UIManager.singleton.message.show("请选择其他图层和第一个图层合并!");
        }


        blitMaterial.SetFloat("_alpha", source_a);
        Graphics.Blit(source, dest, blitMaterial);
    }


    //尺寸不一样的图片用这功能, 图片不会被混合, 只能覆盖, 并且居中.
    public static void blit(Texture source, /*float source_a,*/ RenderTexture dest)
    {
        Vector2 scale = Vector2.one;
        Vector2 offset = Vector2.zero;
        if (source.height > source.width)
        {
            scale.x = (float)source.height / (float)source.width * (float)dest.width / (float)dest.height;
            offset.x = 0.5f - scale.x * 0.5f;
        }
        else if (source.width > source.height)
        {
            scale.y = (float)source.width / (float)source.height * (float)dest.height / (float)dest.width;
            offset.y = 0.5f - scale.y * 0.5f;
        }

        Graphics.Blit(source, dest, scale, offset);
    }


    /*public static Texture2D blit(Texture source, Color clear, int width, int height, System.Action<RenderTexture> framesBlit)
    {
        RenderTexture dest = RenderTexture.GetTemporary(width, height);

        dest.clear(clear);

        if (framesBlit != null)
            framesBlit(dest);
        else
            blit(source, 1f, dest);

        Texture2D temp = dest.getTexture2D();
        temp.name = source.name + "_blit";
        RenderTexture.ReleaseTemporary(dest);

        return temp;
    }*/


    //
    public static void mirror(RenderTexture source, bool isHorizon)
    {
        var temp = source.getTexture2D();
        mirror(temp, source, isHorizon);
        Object.Destroy(temp);
    }
    public static void mirror(Texture2D source, bool isHorizon)
    {
        using (var rt = new RenderTarget(source.width, source.height))
        {
            mirror(source, rt.renderTexture, isHorizon);

            rt.renderTexture.getTexture2D(source);
        }
    }
    public static void mirror(Texture2D source, RenderTexture dest, bool isHorizon)
    {
        if (isHorizon)
            Graphics.Blit(source, dest, new Vector2(-1, 1), new Vector2(1, 0));
        else
            Graphics.Blit(source, dest, new Vector2(1, -1), new Vector2(0, 1));
    }
}


public class RenderTarget : System.IDisposable
{
    public RenderTexture renderTexture { get; protected set; }
    public RenderTarget(int width, int height, Color clear = new Color(), RenderTextureFormat format = RenderTextureFormat.ARGB32, int depthBuffer = 0)
    {
        renderTexture = RenderTexture.GetTemporary(width, height, depthBuffer, format);
        renderTexture.clear(clear);
    }

    public void Dispose()
    {
        RenderTexture.ReleaseTemporary(renderTexture);
        renderTexture = null;
    }


    //
    public void clear(Color color)
    {
        renderTexture.clear(color);
    }

    public void blit(Texture source, float source_a)
    {
        GraphicsEx.blit(source, source_a, renderTexture);
    }

    //尺寸不一样的图片用这功能, 图片不会被混合, 只能覆盖, 并且居中.
    public void blit(Texture source)
    {
        GraphicsEx.blit(source, renderTexture);
    }


    //
    public Texture2D getTexture2D()
    {
        return renderTexture.getTexture2D();
    }
    public void getTexture2D(Texture2D texture)
    {
        renderTexture.getTexture2D(texture);
    }

}
