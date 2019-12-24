using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Screenshot : System.IDisposable
{
    Camera camera;
    public Screenshot(Camera c = null)
    {
        if (c == null)
            c = Camera.main;

        camera = c;


        //
        var screenshotInCamera = camera.gameObject.AddComponent<ScreenshotInCamera>();
        screenshotInCamera.screenshot = this;
        done = false;
    }

    public RenderTexture renderTexture;
    public Screenshot(int w, int h, /*RenderTextureFormat f = RenderTextureFormat.ARGB32,*/ Camera c = null)
    {
        if (c == null)
            c = Camera.main;

        camera = c;


        //
        //renderTexture = new RenderTexture(w, h, 0, f);
        renderTexture = RenderTexture.GetTemporary(w, h, 32);
        renderTexture.name = "Screenshot";
        camera.targetTexture = renderTexture;


        //
        var screenshotInCamera = camera.gameObject.AddComponent<ScreenshotInCamera>();
        screenshotInCamera.screenshot = this;
        done = false;
    }

    void destroy()
    {
        if (scaleTexture != null)
            Object.Destroy(scaleTexture);
        if (texture != null)
            Object.Destroy(texture);
        if (renderTexture != null)
        {
            RenderTexture.ReleaseTemporary(renderTexture);
            camera.targetTexture = null;
        }

        scaleTexture = null;
        texture = null;
        renderTexture = null;
    }

    public void Dispose() { destroy(); }


    class ScreenshotInCamera : MonoBehaviour
    {
        public Screenshot screenshot;

        //void OnPostRender()
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);

            if (screenshot.renderTexture == null)
            {
                screenshot.texture = new Texture2D(screenshot.camera.pixelWidth, screenshot.camera.pixelHeight);
                screenshot.texture.wrapMode = TextureWrapMode.Clamp;

                var rect = screenshot.camera.rect;
                rect = new Rect
                    (rect.xMin * screenshot.texture.width / rect.width
                    , rect.yMin * screenshot.texture.height / rect.height
                    , screenshot.texture.width
                    , screenshot.texture.height);
                screenshot.texture.ReadPixels(rect, 0, 0, false);
                screenshot.texture.Apply();
            }


            screenshot.done = true;
            this.destroy();
        }
    }

    public bool done { get; protected set; }

    Texture2D _texture;
    public Texture2D texture
    {
        set
        {
            _texture = value;
        }
        get
        {
            if (_texture == null)
                _texture = renderTexture.getTexture2D();
            return _texture;
        }
    }


    public Texture2D scaleTexture = null;
    public Screenshot clampSize(Vector2 size)
    {
        var s = calculateSize(size, new Vector2(texture.width, texture.height));

        readPixels(s, new Rect(0, 0, s.x, s.y));

        return this;
    }
    public Screenshot clampSize(float size)
    {
        var s = calculateSize(new Vector2(size, size), new Vector2(texture.width, texture.height));

        readPixels(s, new Rect(0, 0, s.x, s.y));

        return this;
    }
    public Screenshot clampSizeCenter(float size)
    {
        var s = calculateSize(new Vector2(size, size), new Vector2(texture.width, texture.height));

        readPixels(s, getCenterRect(s.x, s.y));

        return this;
    }

    void readPixels(Vector2 size, Rect rect)
    {
        //
        RenderTexture last = RenderTexture.active;

        RenderTexture downscaledRT = RenderTexture.GetTemporary((int)size.x, (int)size.y);
        Graphics.Blit(texture, downscaledRT);
        RenderTexture.active = downscaledRT;

        scaleTexture = new Texture2D((int)size.x, (int)size.y);
        scaleTexture.wrapMode = TextureWrapMode.Clamp;
        scaleTexture.ReadPixels(rect, 0, 0, false);
        scaleTexture.Apply();

        RenderTexture.ReleaseTemporary(downscaledRT);

        RenderTexture.active = last;
    }



    //图片在框里按原来比例显示
    public static Vector2 calculateSize(Vector2 clampSize, Vector2 textureSize)
    {
        Vector2 newSize = clampSize;
        if (clampSize.x * textureSize.y > clampSize.y * textureSize.x)
        {
            newSize.Set(clampSize.y * textureSize.x / textureSize.y, clampSize.y);
        }
        else if (clampSize.x * textureSize.y < clampSize.y * textureSize.x)
        {
            newSize.Set(clampSize.x, clampSize.x * textureSize.y / textureSize.x);
        }

        return newSize;
    }
    public static Vector2 calculateSize(float clampSize, Vector2 textureSize)
    {
        return calculateSize(new Vector2(clampSize, clampSize), textureSize);
    }
    //
    /*public static Vector2 getScaleSizeByMax(Vector2 size, float maxSize)
    {
        if (size.x < size.y)
        {
            var old_y = size.y;
            size.y = maxSize;
            size.x = size.x * maxSize / old_y;
        }
        else
        {
            var old_x = size.x;
            size.x = maxSize;
            size.y = size.y * maxSize / old_x;
        }
        return size;
    }
    //
    public static Vector2 getScaleSizeByMin(Vector2 size, float minSize)
    {
        if (size.x > size.y)
        {
            var old_y = size.y;
            size.y = minSize;
            size.x = size.x * minSize / old_y;
        }
        else
        {
            var old_x = size.x;
            size.x = minSize;
            size.y = size.y * minSize / old_x;
        }

        return size;
    }*/
    //
    public static Rect getCenterRect(float width, float height)
    {
        var s = width > height ? height : width;
        return new Rect((width - s) * 0.5f, (height - s) * 0.5f, s, s);
    }
}