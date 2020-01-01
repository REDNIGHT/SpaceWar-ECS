using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _CopyR2L : MonoBehaviour
{
    Transform GetL() => transform.parent.parent.Find("L");



    [RN._Editor.ButtonInBeginLeftArea]
    void ClearL()
    {
        var L = GetL();
        L.destroyChildrenGOImmediate();
    }

    public bool z180 = false;
    [RN._Editor.ButtonInEndArea]
    void CopyR2L()
    {
        var L = GetL();
        var copyT = GameObject.Instantiate(transform, L);
        copyT.setLocalPositionX(-transform.localPosition.x);
        copyT.setLocalEulerAnglesY(-transform.localEulerAngles.y);
        if (z180)
        {
            copyT.setLocalEulerAnglesZ(-transform.localEulerAngles.z);
        }
    }
}
