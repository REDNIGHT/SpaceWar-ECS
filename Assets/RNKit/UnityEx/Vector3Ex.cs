using UnityEngine;

public static class Vector3Ex
{
    public const float kSqrEpsilon = 0.001f;

    //Approximately equal 约等于

    public static bool equals(this Vector3 l, Vector3 r, float sqrEpsilon = kSqrEpsilon)
    {
        return Vector3.SqrMagnitude(l - r) < sqrEpsilon;
    }

    public static bool isZero(this Vector3 motion, float sqrEpsilon = kSqrEpsilon)
    {
        return equals(motion, Vector3.zero, sqrEpsilon);
    }

    public static bool notZero(this Vector3 motion, float sqrEpsilon = kSqrEpsilon)
    {
        return !equals(motion, Vector3.zero, sqrEpsilon);
    }
}