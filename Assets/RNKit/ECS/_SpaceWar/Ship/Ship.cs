using Unity.Entities;
using Unity.Mathematics;

namespace RN.Network.SpaceWar
{
    public class ShipEntity : System.Attribute { }

    [ShipEntity]
    public struct Ship : IComponentData
    {
    }


    [System.Serializable]
    public struct ShipVelocity
    {
        public float force;
        public float maxVelocity;
        //public float stopForce;

        public float torque;
        public float maxTorque;
    }
    [System.Serializable]
    public struct ShipAccelerate
    {
        public float force;
        public float maxVelocity;
        public float consumePower;
    }

    [ShipEntity]
    public struct ShipForceControl : IComponentData
    {
        //
        public float noControlTime;
        public float noControlBeginTime => 1f;


        //
        public float3 moveDirection;


        //
        public bool accelerateFire;
        public float accelerateMessageTime;

        public bool OnAccelerate(int accelerateLevel, float fixedDeltaTime, out bool accelerateMessage)
        {
            accelerateMessage = false;

            var b = accelerateFire && accelerateLevel > 0;
            if (b)
            {
                if (accelerateMessageTime == 0f)
                {
                    accelerateMessage = true;
                }

                if (accelerateMessageTime >= 1f)
                {
                    accelerateMessageTime = 0f;
                }
                else
                {
                    accelerateMessageTime += fixedDeltaTime;
                }
            }
            else
            {
                accelerateMessageTime = 0f;
            }
            return b;
        }
    }

    [ShipEntity]
    public struct ShipTorqueControl : IComponentData
    {
        public float noControlTorqueTime;
        public float noControlTorqueBeginTime => 1f;
    }

    [ShipEntity]
    public struct ShipControlInfo : IComponentData
    {
        public float forwardScale;
        public float backScale;
        public float leftRightScale;
        public float moveComboScale;

        public float dragByLRBVelocity;

        public ShipVelocity velocity0;
        public ShipVelocity velocity1;
        public ShipVelocity velocity2;
        public ShipVelocity velocity3;

        public ShipVelocity getVelocity(int velocityLevel)
        {
            if (velocityLevel == 0)
                return velocity0;
            if (velocityLevel == 1)
                return velocity1;
            if (velocityLevel == 2)
                return velocity2;
            if (velocityLevel == 3)
                return velocity3;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            throw new System.IndexOutOfRangeException($"getVelocitye index error!  index:{velocityLevel}");
#else
            return velocity0;
#endif
        }

        public bool accelerateForwardOnly;
        public ShipAccelerate accelerate0;
        public ShipAccelerate accelerate1;
        public ShipAccelerate accelerate2;

        public ShipAccelerate getAccelerate(int velocityLevel)
        {
            if (velocityLevel == 1)
                return accelerate0;
            if (velocityLevel == 2)
                return accelerate1;
            if (velocityLevel == 3)
                return accelerate2;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            throw new System.IndexOutOfRangeException($"getAccelerate index error!  index:{velocityLevel}");
#else
            return accelerate0;
#endif
        }
    }

    [ShipEntity]
    public struct ShipMoveInput : IComponentData
    {
        public bool moveForward;
        public bool moveBack;
        public bool moveLeft;
        public bool moveRight;

        public half torqueY;
        public bool accelerate;

        public bool lost;
    }

    [System.Serializable]
    public struct ShipPower
    {
        public float max;
        public float regain;
    }

    [ShipEntity]
    public struct ShipPowers : IComponentData
    {
        public ShipPower power0;
        public ShipPower power1;
        public ShipPower power2;
        public ShipPower power3;

        public float lostInputTime;
        /// <summary>
        /// 最终的lostInputTime = lostInputTime + math.abs(power.value) * power2Time
        /// </summary>
        public float power2Time => 0.05f;

        public ShipPower get(int level)
        {
            if (level == 0)
                return power0;
            if (level == 1)
                return power1;
            if (level == 2)
                return power2;
            if (level == 3)
                return power3;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            throw new System.IndexOutOfRangeException($"get index error!  index:{level}");
#else
            return power0;
#endif
        }
    }


    [ShipEntity]
    public struct ShipLostInputState : IComponentData
    {
        public float time;
    }
}
