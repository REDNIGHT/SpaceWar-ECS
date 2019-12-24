using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class WeaponEntity : System.Attribute { }

    public enum WeaponType : byte
    {
        Attack,
        Shield,
        Power,
        Velocity,
    }

    [WeaponEntity]
    public struct Weapon : IComponentData
    {
        public WeaponType type;
        public bool autoFire;//右键自动开火
    }

    [WeaponEntity]
    public struct WeaponAttribute : IComponentData
    {
        public short hp;
        public short lastHp;

        public short itemCount;
        public short lastItemCount;
    }

#if false
    [WeaponEntity]
    public struct WeaponExplosionSelf : IComponentData
    {
        public ActorOwner lastShipActorOwner;
        public Entity lastShipEntity;
    }
#endif

    [WeaponEntity]
    public struct WeaponInstalledState : IComponentData
    {
        public ActorOwner shipActorOwner;

        public Entity shipEntity => slot.shipEntity;

        public Entity slotEntity;
        public Slot slot;

        /// <summary>
        /// 右键自动开火
        /// </summary>
        public bool autoFire;

        //temp
        public SyncActorType syncType;

        //
        //public float3 firePosition;
        public float3 fireDirection;
        public bool calculateLocalRotation;
        public quaternion fireLocalRotation;
    }

    [WeaponEntity]
    public struct OnWeaponInstallMessage : IComponentData
    {
    }
    [WeaponEntity]
    public struct OnWeaponUninstallMessage : IComponentData
    {
    }


    //
    [WeaponEntity]
    public struct Weapon_OnShipDestroyMessage : IComponentData
    {
    }



    //
    [WeaponEntity]
    public struct WeaponShield : IComponentData
    {
        public byte level;//从0开始
    }



    //firePrepare > fireInterval时才会有这Message
    [WeaponEntity]
    public struct OnWeaponControlFirePrepareMessage : IComponentData
    {
    }
#if false
    //fireActorTypes是None时才会有这Message
    [WeaponEntity]
    public struct OnWeaponControlFireOnMessage : IComponentData
    {
    }
    //fireActorTypes是None时才会有这Message
    [WeaponEntity]
    public struct OnWeaponControlFireOffMessage : IComponentData
    {
    }
    //fireActorTypes是None时才会有这Message
    [WeaponEntity]
    public struct WeaponControlInFireState : IComponentData
    {
        public float duration;
    }
#endif

    //[WeaponEntity]
    //这数据暂时只在服务器上用 而且只有部分地方用上 不会同步到客户端
    [LaserEntity]
    public struct WeaponCreator : IComponentData
    {
        public Entity entity;
        //public int creatorId;
    }

    public enum FireType : byte
    {
        None,
        Fire,
        Uninstall,
    }
    public struct WeaponInput : IComponentData
    {
        public FireType fireType;
        public float3 firePosition;

        public FireType lastFireType;
    }

    [WeaponEntity]
    public struct WeaponControlInfo : IComponentData
    {
        public float fireInputTime;
        public float firePrepare;
        public float fireInterval;
        //public float fireDuration;
        public bool needPrepareMessage;

        /// <summary>
        /// 第二次或以上开火时 是否需要准备时间
        /// </summary>
        public bool needPrepareEachLoop;


        public float consumePower;
        public float consumePowerByMainSlot;
        public ActorTypes fireActorType;
        public ActorTypes fireActorTypeByMainSlot;
        public float attributeScaleByMainSlot;

        public bool fireDirectionByShip;
        public bool fireDirectionBySlot => !fireDirectionByShip;

        public WeaponControlInfo
            (
                float fireInputTime,
                float firePrepare,
                float fireInterval,
                //float fireDuration,
                bool needPrepareMessage,
                bool needPrepareEachLoop,

                float consumePower,
                ActorTypes fireActorType,
                float consumePowerByMainSlot,
                ActorTypes fireActorTypeByMainSlot,
                float attributeScaleByMainSlot,

                bool fireDirectionByShip
            )
        {
            this.fireInputTime = fireInputTime;
            this.firePrepare = firePrepare;
            this.fireInterval = fireInterval;
            //this.fireDuration = fireDuration;
            this.needPrepareMessage = needPrepareMessage;
            this.needPrepareEachLoop = needPrepareEachLoop;

            this.consumePower = consumePower;
            this.fireActorType = fireActorType;
            this.consumePowerByMainSlot = consumePowerByMainSlot;
            this.fireActorTypeByMainSlot = fireActorTypeByMainSlot;
            this.attributeScaleByMainSlot = attributeScaleByMainSlot;

            this.fireDirectionByShip = fireDirectionByShip;
        }

        public float GetConsumePower(bool mainSlot) => mainSlot ? consumePowerByMainSlot : consumePower;
        public ActorTypes GetFireActorType(bool mainSlot) => mainSlot ? fireActorTypeByMainSlot : fireActorType;
        public float GetAttributeScale(bool mainSlot) => mainSlot ? attributeScaleByMainSlot : 1f;
    }


    [WeaponEntity]
    public struct WeaponControl : IComponentData
    {
        //public bool fireByOutOfSlotAngle;

        public float inputTime;
        public float inputTimeMax;
        public float inputTimePresent => inputTimeMax == 0f ? 0f : inputTime / inputTimeMax;
        //public float prepare;
        public float time;
        public bool inFire => inputTime > 0f || time > 0f;

        public void DoFire(in WeaponControlInfo data)
        {
            if (states == States.Idle)
            {
                states = States.On;
                time = data.firePrepare;
            }

            inputTime = data.firePrepare + data.fireInputTime;
            inputTimeMax = inputTime;
        }


        public enum States
        {
            Idle,
            On,
            Prepareing,
            CoolDown,
        }
        States states;

        public enum FireEvent
        {
            None,
            Prepare,
            Fire,
        }
        public FireEvent OnFire(float fixedDeltaTime, in WeaponControlInfo data)
        {
            var outEvent = FireEvent.None;
            if (inputTime > 0f || states == States.On || states == States.Prepareing)
            {
                inputTime -= fixedDeltaTime;
                time -= fixedDeltaTime;

                if (states == States.On)
                {
                    if (data.needPrepareMessage)
                        outEvent = FireEvent.Prepare;

                    states = States.Prepareing;
                }
                else if (states == States.Prepareing)
                {
                    if (time <= 0)//end
                    {
                        outEvent = FireEvent.Fire;

                        states = States.CoolDown;
                        time += data.fireInterval;
                    }
                }
                else if (states == States.CoolDown)
                {
                    if (time <= 0)//end
                    {
                        if (data.needPrepareEachLoop)
                        {
                            states = States.On;
                            time += data.firePrepare;
                        }
                        else
                        {
                            outEvent = FireEvent.Fire;

                            states = States.CoolDown;
                            time += data.fireInterval;
                        }
                    }
                }
            }
            else
            {
                if (time > 0f)
                {
                    time -= fixedDeltaTime;

                    if (time <= 0f)
                    {
                        states = States.Idle;
                    }
                }
            }
            return outEvent;
        }

        public void reset()
        {
            inputTime = 0f;
            time = 0f;
            states = States.Idle;
        }
    }


    //
    public struct FireCreateData : IComponentData
    {
        public ActorOwner actorOwner;

        public Entity shipEntity;

        public Entity weaponEntity;
        public ActorTypes fireActorType;
        public float3 firePosition;

        public float attributeOffsetScale;
    }


    //
    public struct WeaponFireFx : IComponentData { }

    public interface IWeaponFireFx
    {
        void OnPlayFx(Entity bulletEntity, in WeaponCreator weaponCreator, IActorSpawnerMap actorSpawnerMap, EntityManager entityManager);
    }


}
