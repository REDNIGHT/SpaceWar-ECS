using Unity.Entities;
using Unity.Mathematics;

namespace RN.Network.SpaceWar
{
    public class ShieldEntity : System.Attribute { }

    [ShieldEntity]
    public struct Shield : IComponentData
    {
        public short curLevel;
    }

    [System.Serializable]
    public struct ShipShield
    {
        public float hp;

        public float prepare;
        public float interval;
        public float duration;
        public float consumePower;

        public float torque;
        public float maxTorque;
    }

    [ShipEntity]
    public struct ShipShields : IComponentData
    {
        public ShipShield shield0;
        public ShipShield shield1;
        public ShipShield shield2;

        public (float lifetime, float hp) get(int index)
        {
            if (index == 0)
                return (shield0.duration, shield0.hp);
            if (index == 1)
                return (shield1.duration, shield1.hp);
            if (index == 2)
                return (shield2.duration, shield1.hp);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            throw new System.IndexOutOfRangeException($"ShipShields.get index error!  index:{index}");
#else
            return (shield0.duration, shield0.hp);
#endif
        }

        void get(in ShipShield shield, ref WeaponControlInfo weaponControl, ref ControlTorqueAngular controlTorqueAngular)
        {
            weaponControl.firePrepare = shield.prepare;
            weaponControl.fireInterval = shield.interval;
            //weaponControl.fireDuration = shield.duration;
            weaponControl.consumePower = shield.consumePower;

            controlTorqueAngular.torque = shield.torque;
            controlTorqueAngular.maxTorque = shield.maxTorque;
        }

        public void get(int index, ref WeaponControlInfo weaponControl, ref ControlTorqueAngular controlTorqueAngular)
        {
            if (index == 0)
            {
                get(shield0, ref weaponControl, ref controlTorqueAngular);
                return;
            }

            if (index == 1)
            {
                get(shield1, ref weaponControl, ref controlTorqueAngular);
                return;
            }

            if (index == 2)
            {
                get(shield2, ref weaponControl, ref controlTorqueAngular);
                return;
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            throw new System.IndexOutOfRangeException($"ShipShields.get index error!  index:{index}");
#else
            get(shield0, ref weaponControl, ref controlTorqueAngular);
#endif
        }
    }

    [ShieldEntity]
    public struct Shield_R_Temp : IComponentData
    {
        public quaternion rotation;
    }



    public interface IShieldFx
    {
        void OnDestroyFx();
    }
}
