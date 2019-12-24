using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class PhysicsEx
{

    //------------------------------------------------------------------------------------
    public static bool raycast(this Plane p, Ray r, out Vector3 point)
    {
        float enter;
        if (p.Raycast(r, out enter))
        {
            point = r.GetPoint(enter);
            return true;
        }

        point = Vector3.zero;
        return false;
    }
    public static bool raycast(this Plane p, Vector3 begin, Vector3 direction, out Vector3 point)
    {
        return p.raycast(new Ray(begin, direction), out point);
    }





    //------------------------------------------------------------------------------------
    public static bool raycast(Vector3 begin, Vector3 direction, float maxDistance, int layerMask, out RaycastHit hitInfo)
    {
        var b = Physics.Raycast(begin, direction, out hitInfo, maxDistance, layerMask);

#if UNITY_EDITOR
        if (debugDrawEnable)
        {
            drawRay(begin, direction * maxDistance);
            if (b)
                drawPoint(hitInfo.point, hitInfo.normal);
        }
#endif
        return b;
    }


    //------------------------------------------------------------------------------------
    public static bool linecast(Vector3 begin, Vector3 end, int layerMask, out RaycastHit hitInfo)
    {
        var b = Physics.Linecast(begin, end, out hitInfo, layerMask);

#if UNITY_EDITOR
        if (debugDrawEnable)
        {
            drawLine(begin, end);
            if (b)
                drawPoint(hitInfo.point, hitInfo.normal);
        }
#endif
        return b;
    }

    public static RaycastHit[] linecastAll(Vector3 begin, Vector3 end, int layerMask)
    {
        var direction = end - begin;
        var distance = direction.magnitude;

        var rhs = Physics.RaycastAll(new Ray(begin, direction), distance, layerMask);

#if UNITY_EDITOR
        if (debugDrawEnable)
        {
            drawLine(begin, end);

            foreach (var rh in rhs)
                drawPoint(rh.point, rh.normal);
        }
#endif
        return rhs;
    }


    //------------------------------------------------------------------------------------
    public static bool sphereCast(Vector3 begin, Vector3 end, float radius, int layerMask, out RaycastHit hitInfo)
    {//如果开始位置和碰撞体有相交 返回的RaycastHit.point会是0
        var direction = end - begin;
        var distance = direction.magnitude;

        var b = Physics.SphereCast(new Ray(begin, direction), radius, out hitInfo, distance, layerMask);

#if UNITY_EDITOR
        if (debugDrawEnable)
        {
            drawRay(begin, end);
            if (b)
                drawPoint(hitInfo.point, hitInfo.normal);
        }

#endif
        return b;
    }
    public static RaycastHit[] sphereCastAll(Vector3 begin, Vector3 end, float radius, int layerMask)
    {//如果开始位置和碰撞体有相交 返回的RaycastHit.point会是0
        var direction = end - begin;
        var distance = direction.magnitude;

        var rhs = Physics.SphereCastAll(new Ray(begin, direction), radius, distance, layerMask);

#if UNITY_EDITOR
        if (debugDrawEnable)
        {
            if (rhs.Length >= 4)
                drawRay(begin, end);
            if (rhs.Length >= 3)
                drawRay(begin, end);
            if (rhs.Length >= 2)
                drawRay(begin, end);
            else if (rhs.Length >= 1)
                drawRay(begin, end);
            else if (rhs.Length >= 0)
                drawRay(begin, end);

            foreach (var rh in rhs)
                drawPoint(rh.point, rh.normal);
        }
#endif
        return rhs;
    }

    public static bool sphereCastExcludeRigidbody(Vector3 begin, Vector3 end, float radius, int layerMask, out RaycastHit hitInfo)
    {
        var dis = float.MaxValue;
        hitInfo = new RaycastHit();
        foreach (var hi in PhysicsEx.sphereCastAll(begin, end, radius, layerMask))
        {//返回的物件s 不会按顺序 从近到远
            if (hi.rigidbody != null)
                continue;
            if (hi.distance < dis)
            {
                dis = hi.distance;
                hitInfo = hi;
            }
        }
        return dis != float.MaxValue;
    }

    public static bool raycastExcludeRigidbody(Vector3 begin, Vector3 end, int layerMask, out RaycastHit hitInfo)
    {
        var dis = float.MaxValue;
        hitInfo = new RaycastHit();
        foreach (var hi in PhysicsEx.linecastAll(begin, end, layerMask))
        {//返回的物件s 不会按顺序 从近到远
            if (hi.rigidbody != null)
                continue;
            if (hi.distance < dis)
            {
                dis = hi.distance;
                hitInfo = hi;
            }
        }
        return dis != float.MaxValue;
    }

    public static IEnumerable<Collider> overlapSphereExcludeRigidbody(Vector3 position, float radius, int layerMask)
    {
        var cs = Physics.OverlapSphere(position, radius, layerMask);
        foreach (var c in cs)
        {
            if (c.GetComponent<Rigidbody>() == null)
                yield return c;
        }
    }

    public static IEnumerable<Collider> overlapCone(Vector3 position, Vector3 dir, float angle, float radius, int layerMask)
    {
#if UNITY_EDITOR
        if (angle <= 0f)
            Debug.Log("angle <= 0f");
        if (radius <= 0f)
            Debug.Log("radius <= 0f");
        if (dir == Vector3.zero)
            Debug.Log("dir == Vector3.zero");
#endif

        var cs = Physics.OverlapSphere(
            position, radius,
            layerMask);

        var angleHalf = angle * 0.5f;
        foreach (var c in cs)
        {
            //
            var curDir = (c.transform.position - position);
            if (Mathf.Abs(Vector3.Angle(dir, curDir)) > angleHalf)
                continue;

            yield return c;
        }
    }


    //
    public static bool capsuleCast(this CapsuleCollider capsuleCollider, Vector3 position, Vector3 direction, float distance, int layerMask, out RaycastHit hitInfo)
    {
        if (capsuleCollider.direction != 1)
            Debug.LogError("capsuleCollider.direction != 1");

        var radius = capsuleCollider.radius;
        var height = capsuleCollider.height - radius * 2f;
        Vector3 up = capsuleCollider.transform.up;
        Vector3 p1 = position + up * -height * 0.5f;
        Vector3 p2 = p1 + up * height;

        var b = Physics.CapsuleCast(p1, p2, radius, direction, out hitInfo, distance, layerMask);

#if UNITY_EDITOR
        if (debugDrawEnable)
        {
            drawLine(position, position + direction * distance);

            /*debugDrawPointColor = Color.blue;
            PhysicsEx.drawPoint(p1, Vector3.up);
            PhysicsEx.drawPoint(p2, Vector3.up);
            debugDrawPointColor = Color.yellow;*/

            if (b)
            {
                drawPoint(hitInfo.point, hitInfo.normal);
            }
        }
#endif
        return b;
    }

    //
    public static RaycastHit[] capsuleCastAll(this CapsuleCollider capsuleCollider, Vector3 position, Vector3 direction, float distance, int layerMask)
    {
        if (capsuleCollider.direction != 1)
            Debug.LogError("capsuleCollider.direction != 1");

        var radius = capsuleCollider.radius;
        var height = capsuleCollider.height - radius * 2f;
        Vector3 up = capsuleCollider.transform.up;
        Vector3 p1 = position + up * -height * 0.5f;
        Vector3 p2 = p1 + up * height;

        var hitInfos = Physics.CapsuleCastAll(p1, p2, radius, direction, distance, layerMask);

#if UNITY_EDITOR
        if (debugDrawEnable)
        {
            drawLine(position, position + direction * distance);

            /*debugDrawPointColor = Color.blue;
            PhysicsEx.drawPoint(p1, Vector3.up);
            PhysicsEx.drawPoint(p2, Vector3.up);
            debugDrawPointColor = Color.yellow;*/

            if (hitInfos.Length > 0)
            {
                foreach (var rh in hitInfos)
                    drawPoint(rh.point, rh.normal);
            }
        }
#endif
        return hitInfos;
    }

    public static bool boxCast(this BoxCollider boxCollider, Vector3 position, Quaternion rotation, Vector3 direction, float distance, int layerMask, out RaycastHit hitInfo)
    {
        var b = Physics.BoxCast(position + boxCollider.center, boxCollider.size * 0.5f, direction, out hitInfo, rotation, distance, layerMask);

#if UNITY_EDITOR
        if (debugDrawEnable)
        {
            drawRay(position, direction * distance);
            if (b)
                drawPoint(hitInfo.point, hitInfo.normal);
        }
#endif
        return b;
    }

    public static bool sphereCast(this SphereCollider sphereCollider, Vector3 position, Vector3 direction, float distance, int layerMask, out RaycastHit hitInfo)
    {
        var b = Physics.SphereCast(position + sphereCollider.center, sphereCollider.radius, direction, out hitInfo, distance, layerMask);

#if UNITY_EDITOR
        if (debugDrawEnable)
        {
            drawRay(position, direction * distance);
            if (b)
                drawPoint(hitInfo.point, hitInfo.normal);
        }
#endif
        return b;
    }


    //
    public static RaycastHit[] sweepTestAll(this Rigidbody self, Vector3 position, Vector3 direction, float distance)
    {
        var lastPosition = self.position;
        self.position = position;

        var b = self.SweepTestAll(direction, distance);

        self.position = lastPosition;
        return b;
    }
    public static RaycastHit[] sweepTestAll(this Rigidbody self, Vector3 direction, float distance)
    {
        return self.SweepTestAll(direction, distance);
    }
    public static bool sweepTest(this Rigidbody self, Vector3 direction, float distance, out RaycastHit hitInfo)
    {
        return self.SweepTest(direction, out hitInfo, distance);
    }

    /*

    public static bool sweepTest(this Rigidbody rigidbody, Vector3 position, Vector3 direction, float distance, out RaycastHit hitInfo)
    {
        //var save_layer = rigidbody.gameObject.layer;
        //rigidbody.gameObject.layer = myLayer;

        var lastPosition = rigidbody.position;
        rigidbody.position = position;

        //{
        var b = rigidbody.SweepTest(direction, out hitInfo, distance);
        //}

        rigidbody.position = lastPosition;
        //rigidbody.gameObject.layer = save_layer;

#if UNITY_EDITOR
        if (debugDrawEnable)
        {
            drawRay(position, direction * distance);
            if (b)
                drawPoint(hitInfo.point, hitInfo.normal);
        }
#endif
        return b;
    }

    public static Vector3 moveTest(this Rigidbody rigidbody, Vector3 begin, Vector3 end)
    {
        var direction = end - begin;
        var direction_n = direction.normalized;
        var distance = direction.magnitude;

        RaycastHit hitInfo;
        if (sweepTest(rigidbody, begin, direction_n, distance, out hitInfo))
        {
            var normal = hitInfo.normal;
            normal.y = 0f;
            normal.Normalize();
            var offset = Vector3.ProjectOnPlane(direction, normal);

            return begin + offset;
        }

        return end;
    }*/


    //------------------------------------------------------------------------------------
    public static bool debugDrawEnable = false;
    public static Color debugDrawLineColor = Color.white;
    public static Color debugDrawPointColor = Color.yellow;
    public static float debugDrawDuration = 0.5f;
    public static float debugDrawPointSize = 0.05f;

    public static void drawLine(Vector3 begin, Vector3 end)
    {
        Debug.DrawLine(begin, end, debugDrawLineColor, debugDrawDuration);
    }
    public static void drawRay(Vector3 start, Vector3 dir)
    {
        Debug.DrawRay(start, dir, debugDrawLineColor, debugDrawDuration);
    }
    public static void drawRay(this Ray ray, float distance)
    {
        Debug.DrawRay(ray.origin/* + new Vector3(0f, Random.value * 0.25f, 0f)*/, ray.direction * distance, debugDrawLineColor, debugDrawDuration);
    }
    public static void drawPoint(Vector3 point, Vector3 normal)
    {
        Debug.DrawLine(point + Camera.main.transform.right * debugDrawPointSize, point - Camera.main.transform.right * debugDrawPointSize, debugDrawPointColor, debugDrawDuration);
        Debug.DrawLine(point + Camera.main.transform.up * debugDrawPointSize, point - Camera.main.transform.up * debugDrawPointSize, debugDrawPointColor, debugDrawDuration);

        Debug.DrawLine(point, point + normal * debugDrawPointSize * 2f, debugDrawPointColor, debugDrawDuration);
    }


}