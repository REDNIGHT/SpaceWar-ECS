using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public static class SpriteEx
{
    public static void autoSetSprite(this Image image, Sprite sprite)
    {
        image.sprite = sprite;
    }
    public static void autoSetSprite(this SpriteRenderer spriteRenderer, Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }
    public static void autoSetSprite(this Image image, Sprite sprite, Color color)
    {
        image.sprite = sprite;
        image.color = color;
    }
    public static void autoSetSprite(this SpriteRenderer spriteRenderer, Sprite sprite, Color color)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
    }


    public static void autoSetSprite(this Component c, Sprite sprite)
    {
        var image = c.GetComponentInChildren<Image>(true);
        if (image != null)
        {
            autoSetSprite(image, sprite);
            return;
        }

        var spriteRenderer = c.GetComponentInChildren<SpriteRenderer>(true);
        if (spriteRenderer != null)
        {
            autoSetSprite(spriteRenderer, sprite);
            return;
        }

        Debug.LogError("can not find sprite component!", c);
    }

    public static void autoSetSprite(this Component c, string hierarchy, Sprite sprite)
    {
        var t = c.transform.Find(hierarchy);
        if (t != null)
            t.autoSetSprite(sprite);
        else
            Debug.LogError("can not find hierarchy=" + hierarchy, c);
    }


    public static void autoSetSprite(this Component c, Sprite sprite, Color color)
    {
        var image = c.GetComponentInChildren<Image>(true);
        if (image != null)
        {
            autoSetSprite(image, sprite, color);
            return;
        }

        var spriteRenderer = c.GetComponentInChildren<SpriteRenderer>(true);
        if (spriteRenderer != null)
        {
            autoSetSprite(spriteRenderer, sprite, color);
            return;
        }

        Debug.LogError("can not find sprite component!", c);
    }

    public static void autoSetSprite(this Component c, string hierarchy, Sprite sprite, Color color)
    {
        var t = c.transform.Find(hierarchy);
        if (t != null)
            t.autoSetSprite(sprite, color);
        else
            Debug.LogError("can not find hierarchy=" + hierarchy, c);
    }


    //
    public static Sprite autoGetSprite(this Component c)
    {
        var image = c.GetComponentInChildren<Image>(true);
        if (image != null)
            return image.sprite;

        var spriteRenderer = c.GetComponentInChildren<SpriteRenderer>(true);
        if (spriteRenderer != null)
            return spriteRenderer.sprite;

        Debug.LogError("can not find sprite component!", c);
        return null;
    }
}
