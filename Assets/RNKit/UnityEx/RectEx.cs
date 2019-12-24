
using UnityEngine;

public static class RectEx
{
    public static Vector2 clamp(this Rect rect, Vector2 point)
    {
        if (point.x > rect.xMax)
            point.x = rect.xMax;
        if (point.x < rect.xMin)
            point.x = rect.xMin;

        if (point.y > rect.yMax)
            point.y = rect.yMax;
        if (point.y < rect.yMin)
            point.y = rect.yMin;

        return point;
    }
    public static Rect encapsulate(this Rect rect, Vector2 point)
    {
        if (point.x > rect.xMax)
            rect.xMax = point.x;
        if (point.x < rect.xMin)
            rect.xMin = point.x;

        if (point.y > rect.yMax)
            rect.yMax = point.y;
        if (point.y < rect.yMin)
            rect.yMin = point.y;

        return rect;
    }
}