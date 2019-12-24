using UnityEngine;
using System.Collections;
namespace RN
{
    public static class BoundsEx
    {
        public static Vector3 getRandomPoint(this Vector3 size)
        {
            var x = Random.Range(0f, size.x) - size.x * 0.5f;
            var y = Random.Range(0f, size.y) - size.y * 0.5f;
            var z = Random.Range(0f, size.z) - size.z * 0.5f;
            return new Vector3(x, y, z);
        }

        public static Vector3 getRandomPoint(this Bounds b)
        {
            var x = Random.Range(b.min.x, b.max.x);
            var y = Random.Range(b.min.y, b.max.y);
            var z = Random.Range(b.min.z, b.max.z);
            return new Vector3(x, y, z);
        }

        public static Vector3 getRandomPoint(Vector3 center, Vector3 size)
        {
            return getRandomPoint(new Bounds(center, size));
        }

        public static Vector3 getRandomPoint(this BoxCollider box)
        {
            return getRandomPoint(new Bounds(box.center + box.transform.position, box.size));
        }

    }
}