using Unity.Mathematics;
using UnityEngine;

public static class RigidbodyEx
{
    /// <summary>
    ///超出maxVelocity范围的速度会继续保留 不会参与到控制的计算中  
    ///例如刚体被某个功能加速 速度飞快 但同时通过AddForce也能部分被控制
    ///如果没有这段代码 离开加速速触发器后 刚体马上就能被完全控制 加速效果很快就没了
    /// </summary>
    /// <param name="maxVelocity"></param>
    /// <param name="controlVelocity"></param>
    public static void ClampLinearVelocity(float maxVelocity, ref float3 linearVelocity)
    {
        if (math.lengthsq(linearVelocity) > math.pow(maxVelocity, 2f))
        {
            linearVelocity = math.normalize(linearVelocity) * maxVelocity;
        }
    }

    /// <summary>
    ///direction经过normalize后 maxVelocity的表现就正确  适合做wasd的移动
    ///direction没经过normalize 运动会更加平滑 适合做跟随目标 也可以直接用下面一个AddForce
    /// </summary>
    public static float3 AddForce(float force, float maxVelocity, float3 direction, in float3 curVelocity)
    {
        direction = direction * maxVelocity;
        direction = direction - curVelocity;

        return direction * force;
    }
    public static float3 AddForce(float force, float maxVelocity, in float3 origin, in float3 target, in float3 curVelocity)
    {
        var direction = target - origin;
        return AddForce(force, maxVelocity, direction, curVelocity);
    }

    public static float3 AddTorque(float torque, float maxTorque, in float3 curDirectionN, in float3 targetDirectionN, in float3 curVelocity)
    {
        var torqueCross = math.cross(curDirectionN, targetDirectionN);
        return AddForce(torque, maxTorque, torqueCross, curVelocity);
    }

    public static void AddForce(this Rigidbody rigidbody, float force, float maxVelocity, in float3 controlDirectionN)
    {
        rigidbody.AddForce(AddForce(force, maxVelocity, controlDirectionN, rigidbody.velocity));
    }

    public static void AddForce(this Rigidbody rigidbody, float force, float velocity, in float3 origin, in float3 target)
    {
        rigidbody.AddForce(AddForce(force, velocity, origin, target, rigidbody.velocity));
    }

    public static void AddTorque(this Rigidbody rigidbody, float torque, float maxTorque, in float3 curDirectionN, in float3 targetDirectionN)
    {
        rigidbody.AddTorque(AddTorque(torque, maxTorque, curDirectionN, targetDirectionN, rigidbody.angularVelocity));
    }
}

