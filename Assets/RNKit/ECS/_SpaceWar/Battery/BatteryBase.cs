using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class BatteryBase : MonoBehaviour
    {
        public float angleVelocity = 15f;
        void FixedUpdate()
        {
            var a = transform.localEulerAngles;

            a.y += angleVelocity * Time.fixedDeltaTime;

            transform.localEulerAngles = a;
        }
    }
}