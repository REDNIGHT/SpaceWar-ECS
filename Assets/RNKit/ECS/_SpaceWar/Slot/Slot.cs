using System;
using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

namespace RN.Network.SpaceWar
{
    [ShipEntity]
    public struct ShipSlotList : IComponentData
    {
        public byte count;
        public byte Length => 10;

        public Entity slotEntity0;
        public Entity slotEntity1;
        public Entity slotEntity2;
        public Entity slotEntity3;
        public Entity slotEntity4;
        public Entity slotEntity5;
        public Entity slotEntity6;
        public Entity slotEntity7;
        public Entity slotEntity8;
        public Entity slotEntity9;
        /*
        public Entity slotEntity10;
        public Entity slotEntity11;
        public Entity slotEntity12;
        public Entity slotEntity13;
        public Entity slotEntity14;
        public Entity slotEntity15;
        */

        public void Add(Entity slotEntity)
        {
            if (count >= Length)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                throw new IndexOutOfRangeException();
#else
                return;
#endif
            }
            this[count] = slotEntity;
            ++count;
        }

        public Entity this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return slotEntity0;
                    case 1: return slotEntity1;
                    case 2: return slotEntity2;
                    case 3: return slotEntity3;
                    case 4: return slotEntity4;
                    case 5: return slotEntity5;
                    case 6: return slotEntity6;
                    case 7: return slotEntity7;
                    case 8: return slotEntity8;
                    case 9: return slotEntity9;
                }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                throw new IndexOutOfRangeException();
#else
                return Entity.Null;
#endif
            }

            set
            {
                switch (index)
                {
                    case 0: slotEntity0 = value; return;
                    case 1: slotEntity1 = value; return;
                    case 2: slotEntity2 = value; return;
                    case 3: slotEntity3 = value; return;
                    case 4: slotEntity4 = value; return;
                    case 5: slotEntity5 = value; return;
                    case 6: slotEntity6 = value; return;
                    case 7: slotEntity7 = value; return;
                    case 8: slotEntity8 = value; return;
                    case 9: slotEntity9 = value; return;
                }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                throw new IndexOutOfRangeException();
#endif
            }
        }
    }

    public enum AimType : byte
    {
        /// <summary>
        /// 瞄准方向会跟着ship一起转
        /// </summary>
        AimWithShip,

        /// <summary>
        /// 瞄准方向不会跟着ship一起转
        /// </summary>
        AimWithoutShip,
    }

    public partial class SlotInfo : MonoBehaviour
    {
        public bool mainSlot;       //主炮插槽 按中键可以直接开炮

        /// <summary>
        /// 瞄准的角度范围
        /// </summary>
        public float angleLimitMin = 60f;

        /// <summary>
        /// 在开火的时候 超过这瞄准的角度范围武器会恢复到原始位置
        /// </summary>
        public float angleLimitMax = -1f;

        public AimType aimType;


        public bool angleLimitLine = false;

        private void Awake()
        {
            Debug.Assert(angleLimitMin > 0f, "angleLimit > 0f", this);
            if (angleLimitMax < 0f)
                angleLimitMax = angleLimitMin * 1.5f;
        }
        private void Start()
        {
            if (angleLimitLine)
            {
                transform.GetChild(0).localPosition = Vector3.zero;
                transform.GetChild(1).localPosition = Vector3.zero;

                transform.GetChild(0).setLocalEulerAnglesY(-angleLimitMin * 0.5f);
                transform.GetChild(1).setLocalEulerAnglesY(angleLimitMin * 0.5f);

                Debug.Assert(transform.GetChild(0).gameObject.activeSelf == false);
                Debug.Assert(transform.GetChild(1).gameObject.activeSelf == false);
            }
        }
    }

    [ShipEntity]
    public class SlotAngleLimitLines : MonoBehaviour
    {
        List<(LineRenderer l, LineRenderer r, float angleLimit)> lines = new List<(LineRenderer, LineRenderer, float)>();

        private void Awake()
        {
            foreach (Transform c in transform.GetChild(ShipSpawner.Slots_TransformIndex))
            {
                lines.Add((c.GetChild(0).GetComponent<LineRenderer>(), c.GetChild(1).GetComponent<LineRenderer>(), c.GetComponent<SlotInfo>().angleLimitMin));
            }
        }

        public void begin()
        {
            foreach (var line in lines)
            {
                line.l.gameObject.SetActive(true);
                line.r.gameObject.SetActive(true);
            }
        }

        public void end()
        {
            foreach (var lines in lines)
            {
                lines.l.gameObject.SetActive(false);
                lines.r.gameObject.SetActive(false);
            }
        }

        public void update(Vector3 mouse_point)
        {
            foreach (var line in lines)
            {
                var parentT = line.l.transform.parent;
                var angle = Vector3.Angle(parentT.forward, mouse_point - parentT.position);

                var cg = line.l.colorGradient;
                var alphaKeys = cg.alphaKeys;
                var index = alphaKeys.Length - 2;
                var ga = alphaKeys[index];

                ga.alpha = 1f - angle / line.angleLimit;

                alphaKeys[index] = ga;
                cg.alphaKeys = alphaKeys;
                line.l.colorGradient = cg;
                line.r.colorGradient = cg;
            }
        }
    }

    public struct Slot : IComponentData
    {
        public Entity shipEntity;

        public byte index;

        public bool main;

        /// <summary>
        /// 瞄准的角度范围
        /// </summary>
        public float halfAngleLimitMin;

        /// <summary>
        /// 在开火的时候 超过这瞄准的角度范围武器会恢复到原始位置
        /// </summary>
        public float halfAngleLimitMax;

        public AimType aimType;
    }

    public struct AssistSlot : IComponentData
    {
    }

    public struct SlotUsingState : IComponentData
    {
    }
}
