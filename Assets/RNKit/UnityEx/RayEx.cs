using UnityEngine;

public static class RayEx
{
    //
    public static Ray Ray(Vector3 origin, Vector3 target)
    {
        return new Ray(origin, target - origin);
    }
    public static Ray Ray(this Ray ray, Vector3 origin, Vector3 target)
    {
        ray.origin = origin;
        ray.direction = target - origin;
        return ray;
    }
    
}
