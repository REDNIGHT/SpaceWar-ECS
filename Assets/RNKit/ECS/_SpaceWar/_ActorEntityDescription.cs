using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine;

[assembly: RegisterGenericComponentType(typeof(RN.Network.ActorAttribute3<RN.Network.SpaceWar._HP>))]
[assembly: RegisterGenericComponentType(typeof(RN.Network.ActorAttribute3Modifys<RN.Network.SpaceWar._HP>))]
[assembly: RegisterGenericComponentType(typeof(RN.Network.ActorAttribute3Offset<RN.Network.SpaceWar._HP>))]
[assembly: RegisterGenericComponentType(typeof(RN.Network.ActorAttribute3<RN.Network.SpaceWar._Power>))]
[assembly: RegisterGenericComponentType(typeof(RN.Network.ActorAttribute3Modifys<RN.Network.SpaceWar._Power>))]
[assembly: RegisterGenericComponentType(typeof(RN.Network.ActorAttribute3Offset<RN.Network.SpaceWar._Power>))]
[assembly: RegisterGenericComponentType(typeof(RN.Network.ActorAttribute1<RN.Network.SpaceWar._PowerLevel>))]
[assembly: RegisterGenericComponentType(typeof(RN.Network.ActorAttribute1<RN.Network.SpaceWar._VelocityLevel>))]
[assembly: RegisterGenericComponentType(typeof(RN.Network.ActorAttribute1<RN.Network.SpaceWar._ShieldLevel>))]


namespace RN.Network.SpaceWar
{
    public struct _HP { }
    public struct _Power { }

    public struct _Drag { }

    public struct _PowerLevel { }
    public struct _VelocityLevel { }
    public struct _ShieldLevel { }


    //
    public class EntityBuilderEntity : Attribute { }

    class _EntityDescription
    {
        object[] entityDescription = new object[]
        {
            new ServerNetworkEntity(),
                new object[]//Connection
                {
                    new NetworkConnection(),                                    //: IComponentData
                    new NetworkInBuffer(),                                      //: IBufferElementData
                    new NetworkReliableOutBuffer(),                             //: IBufferElementData
                    new NetworkUnreliableOutBuffer(),                           //: IBufferElementData
                    new NetworkConnectedMessage(),                              //: IComponentData             : Message
                    new NetworkDisconnectedMessage(),                           //: IComponentData             : Message
                },
                new object[]//Components
                {
                    new NetworkVersionNetMessage(),                             //: IComponentData             : NetMessage
                    new NetworkPingNetMessage(),                                //: IComponentData             : NetMessage
                    new NetworkHeartbeat(),                                     //: IComponentData
                    new NetworkId(),                                            //: IComponentData

                    new PlayerEnterGameNetMessage(),                            //: IComponentData             : NetMessage
                    new PlayerEnterGameMessage(),                               //: IComponentData             : Message
                    
                    new Player(),                                               //: IComponentData

                    new PlayerName(),                                           //: IComponentData
                    new PlayerNameChangeNetMessage(),                           //: IComponentData             : NetMessage

                    new PlayerTeam(),                                           //: IComponentData
                    new PlayerTeamChangeNetMessage(),                           //: IComponentData             : NetMessage
                    
                    new PlayerScore(),                                          //: IComponentData
                    new PlayerKillList(),                                       //: IBufferElementData
                    
                    new PlayerActorType(),                                      //: IComponentData
                    new PlayerActorSelectNetMessage(),                          //: IComponentData             : NetMessage

                    new PlayerGameReady(),                                      //: IComponentData
                    new PlayerGameStartNetMessage(),                            //: IComponentData             : Message


                    new ObserverPosition(),                                     //: IComponentData
                    new ObserverVisibleDistance(),                              //: IComponentData
                    new ObserverCreateVisibleActorBuffer(),                     //: IBufferElementData
                    new ObserverSyncVisibleActorBuffer(),                       //: IBufferElementData
                    new ObserverDestroyVisibleActorBuffer(),                    //: IBufferElementData

                    new ActorDatas0Buffer(),                                    //: IBufferElementData
                    new ActorDatas1Buffer(),                                    //: IBufferElementData
                    //new ActorDatas2Buffer(),                                  //: IBufferElementData
                    //new ActorDatas3Buffer(),                                  //: IBufferElementData
                    //new ActorDatas4Buffer(),                                  //: IBufferElementData
                    

                    new PlayerActorArray(),                                     //: IBufferElementData
                    new Player_OnShipDestroyMessage(),                          //: IComponentData              : Message

                    new PlayerShipMoveInputNetMessage(),                        //: IComponentData              : NetMessage
                    new PlayerShipFireInputNetMessage(),                        //: IComponentData              : NetMessage
                },

            new ServerRecorderEntity(),
                new object[]//Components
                {
                    new Recorder(),                                             //: IComponentData

                    new ObserverPosition(),                                     //: IComponentData
                    new ObserverVisibleDistance { valueSq = -1f },              //: IComponentData
                    new ObserverCreateVisibleActorBuffer(),                     //: IBufferElementData
                    new ObserverSyncVisibleActorBuffer(),                       //: IBufferElementData
                    new ObserverDestroyVisibleActorBuffer(),                    //: IBufferElementData

                    new ActorDatas0Buffer(),                                    //: IBufferElementData
                    new ActorDatas1Buffer(),                                    //: IBufferElementData
                    //new ActorDatas2Buffer(),                                  //: IBufferElementData
                    //new ActorDatas3Buffer(),                                  //: IBufferElementData
                    //new ActorDatas4Buffer(),                                  //: IBufferElementData
                },

            //每个客户端只有一个
            new ClientNetworkEntity(),
                new object[]//Connection
                {
                    new NetworkConnection(),                                    //: IComponentData
                    new NetworkInBuffer(),                                      //: IBufferElementData
                    new NetworkReliableOutBuffer(),                             //: IBufferElementData
                    new NetworkUnreliableOutBuffer(),                           //: IBufferElementData
                    new NetworkConnectedMessage(),                              //: IComponentData              : Message
                    new NetworkDisconnectedMessage(),                           //: IComponentData              : Message
                },
                new object[]//Components
                {
                    new NetworkVersionResultNetMessage(),                       //: IComponentData              : NetMessage
                    new NetworkPingResultNetMessage(),                          //: IComponentData              : NetMessage
                    new NetworkId(),                                            //: IComponentData
                    
                    new PlayerEnterGameResultNetMessage(),                      //: IComponentData              : NetMessage
                    new PlayerEnterGameMessage(),                               //: IComponentData              : Message

                    new PlayerCreateNetMessages(),                              //: IBufferElementData          : NetMessage
                    new PlayerDestroyNetMessages(),                             //: IBufferElementData          : NetMessage

                    new PlayerNameNetMessages(),                                //: IBufferElementData          : NetMessage
                    new PlayerTeamNetMessages(),                                //: IBufferElementData          : NetMessage
                    new PlayerScoreNetMessages(),                               //: IBufferElementData          : NetMessage
                    
                    new PlayerGameReadyNetMessage(),                            //: IComponentData              : NetMessage
                    new PlayerGameStartNetMessage(),                            //: IComponentData              : NetMessage
                    
                    new PlayerActorType(),                                      //: IComponentData 

                    new ActorCreateSerializeNetMessage(),                       //: IBufferElementData          : NetMessage
                    new ActorDestroySerializeNetMessage(),                      //: IBufferElementData          : NetMessage

                    new ActorSyncFrame_T_NetMessage(),                          //: IBufferElementData          : NetMessage
                    new ActorSyncFrame_R_NetMessage(),                          //: IBufferElementData          : NetMessage
                    new ActorSyncFrame_T_R_NetMessage(),                        //: IBufferElementData          : NetMessage
                    new ActorSyncFrame_RB_VD_NetMessage(),                      //: IBufferElementData          : NetMessage
                    new ActorSyncFrame_RB_T_R_V_NetMessage(),                   //: IBufferElementData          : NetMessage
                    
                    new ActorDatas0Buffer(),                                    //: IBufferElementData          : NetMessage
                    new ActorDatas1Buffer(),                                    //: IBufferElementData          : NetMessage
                    //new ActorDatas2Buffer(),                                  //: IBufferElementData          : NetMessage
                    //new ActorDatas3Buffer(),                                  //: IBufferElementData          : NetMessage
                    //new ActorDatas4Buffer(),                                  //: IBufferElementData          : NetMessage

                    new WeaponInstalledStateNetMessage(),                       //: IBufferElementData          : NetMessage
                    
                    new KillInfoNetMessage(),                                   //: IBufferElementData          : NetMessage
                },


                //每个客户端都会有其他所有客户端的信息(包括自己)
            new ClientPlayerEntity(),
                new object[]
                {
                    new Player(),                                               //: IComponentData
                    new PlayerName(),                                           //: IComponentData
                    new PlayerTeam(),                                           //: IComponentData
                    new PlayerScore(),                                          //: IComponentData

                    new PlayerActorArray(),                                     //: IBufferElementData
                },


            new EntityBuilderEntity(),//server
                new object[]
                {
                    new CallTrigger{type = typeof(OnCallMessage)},              //: IComponentData
                    new EntityBuilder(),                                        //: Component
                    //...
                },
        };
    }


    /// <summary>
    /// num<0     数据发送是可靠的
    /// num>=0    数据发送是不可靠的
    /// </summary>
    public enum ActorSynDataTypes : sbyte
    {
        Ship_Hp_Power = 0,
        Ship_ForceAttribute = 1,
        Ship_AccelerateAttribute = 2,

        WeaponAttribute = 3,
    }

    public enum ActorTypes : byte
    {
        None,

        __Ship_Begin__ = 0,
        ShipA,//综合型  750hp      4个攻击插槽  包含0主炮     3个辅助插槽   根据辅助插槽可以提升到最高等级的护盾,速度,power
        ShipB,//速度型  650hp      5个攻击插槽  包含1主炮     2个辅助插槽   小型护盾(前方60度开启,防御60度, 500hp)             速度快     平移慢   转向慢    power恢复中等
        ShipC,//敏捷型  700hp      5个攻击插槽  包含0主炮     2个辅助插槽   小型护盾(360度开启,防御180度, 500hp)               速度中等   平移快   转向快    power恢复中等
        ShipD,//爆发型  725hp      6个攻击插槽  包含2主炮     2个辅助插槽   中型护盾(前方30度开启,防御45度,550hp)              速度中等   平移中   转向中    power初始值大 恢复慢
        ShipE,//防御型  800hp      4个攻击插槽  包含0主炮     2个辅助插槽   大小型护盾(360度开启,防御60度,750hp)               速度中等   平移慢   转向中    power恢复中等
        __Ship_End__,



        __Weapon_Begin__ = 16,
        Weapon_BulletA,
        Weapon_BulletB,
        Weapon_BulletC,
        Weapon_BulletD,
        //Weapon_BulletE,


        __Weapon_Laser_Begin__ = __Weapon_Begin__ + 16,
        Weapon_LaserA,
        Weapon_LaserB,
        //Weapon_LaserC,


        __Weapon_Missile_Begin__ = __Weapon_Laser_Begin__ + 16,
        Weapon_MissileA,
        Weapon_MissileB,
        Weapon_MissileC,
        Weapon_MissileD,
        Weapon_MissileE,


        __Weapon_Assist_Begin__ = __Weapon_Missile_Begin__ + 16,
        Weapon_Velocity,
        Weapon_Shield,
        Weapon_Power,
        __Weapon_End__,



        __Bullet_Begin__ = __Weapon_Assist_Begin__ + 8,
        BulletA,//单发
        BulletB,//双发
        BulletC,//散弹
        BulletD,//加农炮
        //BulletE,//穿透
        //BulletF,//
        __Bullet_End__,


        __Laser_Begin__ = __Bullet_Begin__ + 16,
        LaserA,//单发穿透细激光
        LaserB,//持续非穿透细激光
        //LaserC,
        //LaserD,
        //LaserE,
        __Laser_End__,


        __Missile_Begin__ = __Laser_Begin__ + 16,
        MissileA,//直线飞行导弹
        MissileB,//跟踪导弹
        MissileC,//指向性导弹  发射1s后转向到鼠标的方向
        MissileD,//黑洞
        MissileE,//感应雷

        //MinShield,//通过导弹或子弹发射出去的小型护盾(碰撞体)

        __Missile_End__,


        __Explosion_Begin__ = __Missile_Begin__ + 16,
        ExplosionA,//大爆炸范围
        ExplosionB,//小爆炸范围
        ExplosionC,//中等爆炸范围
        ExplosionD,//黑洞
        ExplosionE,//感应雷
        //ExplosionE,
        __Explosion_End__,


        __AttributeTrigger_Begin__ = __Explosion_Begin__ + 16,
        HpUpTriggerA,//恢复50hp 后销毁
        HpUpTriggerB,//10s恢复250hp 恢复1000hp后销毁

        //todo...  属性陷阱
        HpDownTrigger,//伤害陷阱


        __AttributeTrigger_End__,



        __PhysicsTrigger_Begin__ = __AttributeTrigger_Begin__ + 16,
        AccelerateTrigger,      //推力陷阱
        BlackHoleTriggerA,      //黑洞陷阱
        BlackHoleTriggerB,      //黑洞陷阱
        LinearDragTrigger,      //阻力陷阱
        AngularDragTrigger,     //角速度阻力陷阱

        //物理陷阱不提供伤害
        //如果需要伤害 就和AttributeTrigger一起创建
        __PhysicsTrigger_End__,



        __SceneObject_Begin__ = __PhysicsTrigger_Begin__ + 16,
        SceneObjectA,
        SceneObjectB,
        SceneObjectC,
        SceneObjectD,
        SceneObjectE,
        __SceneObject_End__,


        __Battery_Begin__ = __SceneObject_Begin__ + 16,
        BatteryA,
        BatteryB,
        //BatteryC,
        //BatteryD,
        //BatteryE,
        __Battery_End__,


        //
        Shield,



        //
        WeaponFirePrepareFx,
        ShipLostInputFx,

        //
        AttributeModifyFx,



        MaxCount,
    }

    public abstract class ActorSpawnerSpaceWar : ActorSpawner
    {
        [Header("actor type")]
        public ActorTypes _actorType;
        public override short actorType => (short)_actorType;

        public override string actorTypeName => _actorType.ToString();

        //
        public const float smoothTime = 1f;
        public const float rotationLerpT = 1f;
        
        public const float smoothTime_NORB = 0.1f;
        public const float rotationLerpT_NORB = 5f;


        protected static void _initRigidbody(Entity e, EntityManager m, float mass, float linearDrag, float angularDrag)
        {
            var rb = m.GetComponentObject<Rigidbody>(e);

            rb.mass = mass;

            rb.drag = linearDrag;
            rb.angularDrag = angularDrag;

            rb.useGravity = false;
            rb.isKinematic = false;

            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }
}

namespace RN.Network
{

    partial class ActorSpawnerMap
    {
        //public int actorTypeMaxCount { get { return (int)SpaceWar.ActorTypes.MaxCount; } }
        partial void GetActorTypeMaxCount(ref int actorTypeMaxCount)
        {
            actorTypeMaxCount = (int)SpaceWar.ActorTypes.MaxCount;
        }
        partial void GetActorTypeName(short actorType, ref string actorTypeName)
        {
            actorTypeName = ((SpaceWar.ActorTypes)actorType).ToString();
        }
    }

    partial struct ActorCreateDatas
    {
        public byte byteValueA;
        public byte byteValueB;

        public half halfValueA;
        public half halfValueB;

        public int intValueA;
        public int intValueB;


        //public half hp;
        //public half power;

        public ushort ByteValueA(byte value)
        {
            byteValueA = value;
            return ActorCreateSerialize.byteValueA;
        }
        public ushort ByteValueB(byte value)
        {
            byteValueB = value;
            return ActorCreateSerialize.byteValueB;
        }

        public ushort HalfValueA(half value)
        {
            halfValueA = value;
            return ActorCreateSerialize.halfValueA;
        }
        public ushort HalfValueB(half value)
        {
            halfValueB = value;
            return ActorCreateSerialize.halfValueB;
        }

        public ushort IntValueA(int value)
        {
            intValueA = value;
            return ActorCreateSerialize.intValueA;
        }
        public ushort IntValueB(int value)
        {
            intValueB = value;
            return ActorCreateSerialize.intValueB;
        }

        /*public ushort Lifetime(float value)
        {
            halfValueA = (half)value;
            return ActorCreateSerialize.lifetime;
        }
        public ushort Hp(float value)
        {
            hp = (half)value;
            return ActorCreateSerialize.hp;
        }
        public ushort Power(float value)
        {
            power = (half)value;
            return ActorCreateSerialize.power;
        }*/


        //
#if ACTOR_2D_SYNC
        public half2 position2d;
        public half rotation2d;
        public float3 position => new float3(position2d.x, 0f, position2d.y);
        public quaternion rotation => Quaternion.Euler(0f, rotation2d, 0f);

        public half2 linearVelicity2d;
        public half angularVelicity2d;
        public float3 linearVelicity => new float3(linearVelicity2d.x, 0f, linearVelicity2d.y);
        public float3 angularVelicity => new float3(0f, angularVelicity2d, 0f);

        public ushort Position(float3 value)
        {
            position2d = (half2)value.xz;
            return ActorCreateSerialize.position;
        }
        public ushort Rotation(quaternion value)
        {
            rotation2d = (half)((Quaternion)value).eulerAngles.y;
            return ActorCreateSerialize.rotation;
        }

        public ushort LinearVelicity(float3 value)
        {
            linearVelicity2d = (half2)value.xz;
            return ActorCreateSerialize.linearVelicity;
        }
        public ushort AngularVelicity(float3 value)
        {
            angularVelicity2d = (half)value.y;
            return ActorCreateSerialize.angularVelicity;
        }
#else
        public half3 position;
        public half4 _rotation;
        public quaternion rotation => new quaternion(_rotation);

        public float3 linearVelicity;
        public float3 angularVelicity;

        public ushort Position(float3 value)
        {
            position = (half3)value;
            return ActorCreateSerialize.position;
        }
        public ushort Rotation(quaternion value)
        {
            _rotation = (half4)value.value;
            return ActorCreateSerialize.rotation;
        }
        public ushort LinearVelicity(float3 value)
        {
            linearVelicity = (half3)value;
            return ActorCreateSerialize.linearVelicity;
        }
        public ushort AngularVelicity(float3 value)
        {
            angularVelicity = (half3)value;
            return ActorCreateSerialize.angularVelicity;
        }
#endif
    }

    partial struct ActorCreateSerialize
    {
        public const ushort byteValueA = 1 << 0;
        public const ushort byteValueB = 1 << 1;
        public const ushort halfValueA = 1 << 2;
        public const ushort halfValueB = 1 << 3;
        public const ushort intValueA = 1 << 4;
        public const ushort intValueB = 1 << 5;
        public const ushort position = 1 << 6;
        public const ushort rotation = 1 << 7;
        public const ushort linearVelicity = 1 << 8;
        public const ushort angularVelicity = 1 << 9;

        //public const ushort lifetime = 1 << 4;
        //public const ushort hp = 1 << 5;
        //public const ushort power = 1 << 6;


        public unsafe void Serialize(DataStreamWriter writer)
        {
            writer.Write(ownerPlayerId);
            writer.Write(actorType);
            writer.Write(actorId);
            writer.Write(dataMask);

            if ((dataMask & byteValueA) > 0)
                writer.Write(datas.byteValueA);
            if ((dataMask & byteValueB) > 0)
                writer.Write(datas.byteValueB);

            if ((dataMask & halfValueA) > 0)
                writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas.halfValueA), sizeof(half));
            if ((dataMask & halfValueB) > 0)
                writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas.halfValueB), sizeof(half));

            if ((dataMask & intValueA) > 0)
                writer.Write(datas.intValueA);
            if ((dataMask & intValueB) > 0)
                writer.Write(datas.intValueB);

            //if ((dataMask & lifetime) > 0)
            //    writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas.halfValueA), sizeof(half));

            //if ((dataMask & hp) > 0)
            //    writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas.hp), sizeof(half));

            //if ((dataMask & power) > 0)
            //    writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas.power), sizeof(half));

#if ACTOR_2D_SYNC
            if ((dataMask & position) > 0)
                writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas.position2d), sizeof(half2));

            if ((dataMask & rotation) > 0)
                writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas.rotation2d), sizeof(half));

            if ((dataMask & linearVelicity) > 0)
                writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas.linearVelicity2d), sizeof(half2));

            if ((dataMask & angularVelicity) > 0)
                writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas.angularVelicity2d), sizeof(half));
#else
            if ((dataMask & position) > 0)
                writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas.position), sizeof(half3));

            if ((dataMask & rotation) > 0)
                writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas._rotation), sizeof(half4));

            if ((dataMask & linearVelicity) > 0)
                writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas.linearVelicity), sizeof(half3));

            if ((dataMask & angularVelicity) > 0)
                writer.WriteBytes((byte*)UnsafeUtility.AddressOf(ref datas.angularVelicity), sizeof(half3));
#endif
        }

        public unsafe void Deserialize(DataStreamReader reader, ref DataStreamReader.Context ctx)
        {
            ownerPlayerId = reader.ReadInt(ref ctx);
            actorType = reader.ReadShort(ref ctx);
            actorId = reader.ReadInt(ref ctx);
            dataMask = reader.ReadUShort(ref ctx);

            if ((dataMask & byteValueA) > 0)
                datas.byteValueA = reader.ReadByte(ref ctx);
            if ((dataMask & byteValueB) > 0)
                datas.byteValueB = reader.ReadByte(ref ctx);

            if ((dataMask & halfValueA) > 0)
                reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas.halfValueA), sizeof(half));
            if ((dataMask & halfValueB) > 0)
                reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas.halfValueB), sizeof(half));

            if ((dataMask & intValueA) > 0)
                datas.intValueA = reader.ReadInt(ref ctx);
            if ((dataMask & intValueB) > 0)
                datas.intValueB = reader.ReadInt(ref ctx);

            //if ((dataMask & lifetime) > 0)
            //    reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas.halfValueA), sizeof(half));

            //if ((dataMask & hp) > 0)
            //    reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas.hp), sizeof(half));

            //if ((dataMask & power) > 0)
            //    reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas.power), sizeof(half));

#if ACTOR_2D_SYNC
            if ((dataMask & position) > 0)
                reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas.position2d), sizeof(half2));

            if ((dataMask & rotation) > 0)
                reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas.rotation2d), sizeof(half));

            if ((dataMask & linearVelicity) > 0)
                reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas.linearVelicity2d), sizeof(half2));

            if ((dataMask & angularVelicity) > 0)
                reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas.angularVelicity2d), sizeof(half));
#else
            if ((dataMask & position) > 0)
                reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas.position), sizeof(half3));

            if ((dataMask & rotation) > 0)
                reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas._rotation), sizeof(half4));

            if ((dataMask & linearVelicity) > 0)
                reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas.linearVelicity), sizeof(half3));

            if ((dataMask & angularVelicity) > 0)
                reader.ReadBytes(ref ctx, (byte*)UnsafeUtility.AddressOf(ref datas.angularVelicity), sizeof(half3));
#endif
        }
    }
}
