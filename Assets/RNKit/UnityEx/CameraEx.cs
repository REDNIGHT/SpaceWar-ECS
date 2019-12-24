using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraEx
{
    public static Vector3 screenDirectionToWorld(this Camera self, Vector2 v, Vector3 up)
    {
        var d = new Vector3(v.x, v.y);

        var cameraT = self.transform;
        d = cameraT.rotation * d;
        var t = Vector3.ProjectOnPlane(cameraT.forward, up);
        return Quaternion.FromToRotation(cameraT.up, t) * d;
    }
}
