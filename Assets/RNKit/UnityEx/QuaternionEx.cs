using UnityEngine;
using System.Collections;

public static class QuaternionEx
{
    //-----------------------------------------------------------------------
    public static Vector3 xAxis(this Quaternion q)
    {
        //float fTx  = 2.0*x;
        float fTy = 2.0f * q.y;
        float fTz = 2.0f * q.z;
        float fTwy = fTy * q.w;
        float fTwz = fTz * q.w;
        float fTxy = fTy * q.x;
        float fTxz = fTz * q.x;
        float fTyy = fTy * q.y;
        float fTzz = fTz * q.z;

        return new Vector3(1.0f - (fTyy + fTzz), fTxy + fTwz, fTxz - fTwy);
    }
    //-----------------------------------------------------------------------
    public static Vector3 yAxis(this Quaternion q)
    {
        float fTx = 2.0f * q.x;
        float fTy = 2.0f * q.y;
        float fTz = 2.0f * q.z;
        float fTwx = fTx * q.w;
        float fTwz = fTz * q.w;
        float fTxx = fTx * q.x;
        float fTxy = fTy * q.x;
        float fTyz = fTz * q.y;
        float fTzz = fTz * q.z;

        return new Vector3(fTxy - fTwz, 1.0f - (fTxx + fTzz), fTyz + fTwx);
    }
    //-----------------------------------------------------------------------
    public static Vector3 zAxis(this Quaternion q)
    {
        float fTx = 2.0f * q.x;
        float fTy = 2.0f * q.y;
        float fTz = 2.0f * q.z;
        float fTwx = fTx * q.w;
        float fTwy = fTy * q.w;
        float fTxx = fTx * q.x;
        float fTxz = fTz * q.x;
        float fTyy = fTy * q.y;
        float fTyz = fTz * q.y;

        return new Vector3(fTxz + fTwy, fTyz - fTwx, 1.0f - (fTxx + fTyy));
    }
    //-----------------------------------------------------------------------
    public static void ToAxes(this Quaternion q, out Vector3 xaxis, out Vector3 yaxis, out Vector3 zaxis)
    {
        var kRot = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);

        xaxis.x = kRot[0, 0];
        xaxis.y = kRot[1, 0];
        xaxis.z = kRot[2, 0];

        yaxis.x = kRot[0, 1];
        yaxis.y = kRot[1, 1];
        yaxis.z = kRot[2, 1];

        zaxis.x = kRot[0, 2];
        zaxis.y = kRot[1, 2];
        zaxis.z = kRot[2, 2];
    }

#if false
    //-----------------------------------------------------------------------
    public static Quaternion operator +(Quaternion l, Quaternion r)
    {
        return new Quaternion(l.w + r.w, l.x + r.x, l.y + r.y, l.z + r.z);
    }
    //-----------------------------------------------------------------------
    public static Quaternion operator -(Quaternion l, Quaternion r)
    {
        return new Quaternion(l.w - r.w, l.x - r.x, l.y - r.y, l.z - r.z);
    }
    //-----------------------------------------------------------------------
    public static Quaternion operator *(Quaternion l, float scalar)
    {
        return new Quaternion(scalar * l.w, scalar * l.x, scalar * l.y, scalar * l.z);
    }
    //-----------------------------------------------------------------------
    public static Quaternion operator *(float scalar, Quaternion r)
    {
        return new Quaternion(scalar * r.w, scalar * r.x, scalar * r.y, scalar * r.z);
    }
    //-----------------------------------------------------------------------
    public static Quaternion operator -(Quaternion q)
    {
        return new Quaternion(-q.w, -q.x, -q.y, -q.z);
    }

    /*public static Quaternion operator *(Quaternion lhs, float rhs)
    {
        return Quaternion.Lerp(Quaternion.identity, lhs, rhs);
    }
    public static Quaternion operator +(Quaternion lhs, Quaternion rhs)
    {
        return lhs * rhs;
    }
    public static Quaternion operator -(Quaternion lhs, Quaternion rhs)
    {
        return lhs * Quaternion.Inverse(rhs);
    }*/
#endif
}
