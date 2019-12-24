using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network
{
    //下面几个ActorDatasXBuffer 的数据发送不能有顺序

    [ServerNetworkEntity]
    [AutoClear]//server和client都需要清空
    public struct ActorDatas0Buffer : IBufferElementData
    {
        public int actorId;
        public sbyte synDataType;
        public bool reliable => synDataType < 0;
        public bool unreliable => synDataType >= 0;
    }
    public struct ActorSyncFrame_Datas0_Serialize : NetworkSerializeUnsafe.ISerializer//在PrefabSpawner.OnPrefabSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorSyncFrame_Datas0;

        public int actorId;//entity Index
        public sbyte synDataType;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new ActorDatas0Buffer { actorId = actorId, synDataType = synDataType }, commandBuffer);
        }
    }

    [ServerNetworkEntity]
    [StructLayout(LayoutKind.Explicit)]
    [AutoClear]//server和client都需要清空
    public struct ActorDatas1Buffer : IBufferElementData
    {
        [FieldOffset(0)] public int actorId;
        [FieldOffset(4)] public sbyte synDataType;
        public bool reliable => synDataType < 0;
        public bool unreliable => synDataType >= 0;

        [FieldOffset(6)] public float floatValue;

        [FieldOffset(6)] public short shortValueA;
        [FieldOffset(8)] public short shortValueB;

        [FieldOffset(6)] public half halfValueA;
        [FieldOffset(8)] public half halfValueB;

        [FieldOffset(6)] public byte byteValueA;
        [FieldOffset(7)] public byte byteValueB;
        [FieldOffset(8)] public byte byteValueC;
        [FieldOffset(9)] public byte byteValueD;
    }
    public struct ActorSyncFrame_Datas1_Serialize : NetworkSerializeUnsafe.ISerializer//在PrefabSpawner.OnPrefabSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorSyncFrame_Datas1;

        public int actorId;//entity Index
        public sbyte synDataType;
        public float floatValue;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new ActorDatas1Buffer { actorId = actorId, synDataType = synDataType, floatValue = floatValue }, commandBuffer);
        }
    }

#if false
    [ServerNetworkEntity]
    [AutoClear]//server和client都需要清空
    public struct ActorDatas2Buffer : IBufferElementData
    {
        public int actorId;
        public sbyte synDataType;
        public bool reliable => synDataType < 0;
        public bool unreliable => synDataType >= 0;

        public float2 float2Value;
    }
    public struct ActorSyncFrame_Datas2_Serialize : NetworkSerializeUnsafe.ISerializer//在PrefabSpawner.OnPrefabSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorSyncFrame_Datas2;

        public int actorId;//entity Index
        public sbyte synDataType;
        public float2 float2Value;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new ActorDatas2Buffer { actorId = actorId, synDataType = synDataType, float2Value = float2Value }, commandBuffer);
        }
    }

    [ServerNetworkEntity]
    [AutoClear]//server和client都需要清空
    public struct ActorDatas3Buffer : IBufferElementData
    {
        public int actorId;
        public sbyte synDataType;
        public bool reliable => synDataType < 0;
        public bool unreliable => synDataType >= 0;

        public float3 float3Value;
    }
    public struct ActorSyncFrame_Datas3_Serialize : NetworkSerializeUnsafe.ISerializer//在PrefabSpawner.OnPrefabSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorSynsFrame_Data3;

        public int actorId;//entity Index
        public sbyte synDataType;
        public float3 float3Value;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new ActorDatas3Buffer { actorId = actorId, synDataType = synDataType, float3Value = float3Value },commandBuffer);
        }
    }

    [ServerNetworkEntity]
    [AutoClear]//server和client都需要清空
    public struct ActorDatas4Buffer : IBufferElementData
    {
        public int actorId;
        public sbyte synDataType;
        public bool reliable => synDataType < 0;
        public bool unreliable => synDataType >= 0;

        public float4 float4Value;
    }
    public struct ActorSyncFrame_Datas4_Serialize : NetworkSerializeUnsafe.ISerializer//在PrefabSpawner.OnPrefabSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorSynsFrame_Data4;

        public int actorId;//entity Index
        public sbyte synDataType;
        public float4 float4Value;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new ActorDatas4Buffer { actorId = actorId, synDataType = synDataType, float4Value = float4Value }, commandBuffer);
        }
    }
#endif


    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct ActorSyncFrame_T_NetMessage : IBufferElementData
    {
        public int actorId;//entity Index

        public float3 position;
    }
    public struct ActorSyncFrame_T_Serialize : NetworkSerializeUnsafe.ISerializer//在PrefabSpawner.OnPrefabSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorSyncFrame_T;

        public int actorId;//entity Index

#if !ACTOR_2D_SYNC
        half3 _position;
        public float3 position
        {
            get => _position;
            set => _position = (half3)value;
        }
#else
        half2 _position;
        public float3 position
        {
            get => new float3(_position.x, 0f, _position.y);
            set => _position = (half2)value.xz;
        }
#endif

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new ActorSyncFrame_T_NetMessage { actorId = actorId, position = position }, commandBuffer);
        }
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct ActorSyncFrame_R_NetMessage : IBufferElementData
    {
        public int actorId;//entity Index

        public quaternion rotation;
    }
    public struct ActorSyncFrame_R_Serialize : NetworkSerializeUnsafe.ISerializer//在PrefabSpawner.OnPrefabSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorSyncFrame_R;

        public int actorId;//entity Index

#if !ACTOR_2D_SYNC
        half4 _rotation;
        public quaternion rotation
        {
            get => (float4)_rotation;
            set => _rotation = (half4)value.value;
        }
#else
        half _rotation;
        public quaternion rotation
        {
            get => Quaternion.Euler(0f, _rotation, 0f);
            set => _rotation = (half)((Quaternion)value).eulerAngles.y;
        }
#endif

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new ActorSyncFrame_R_NetMessage { actorId = actorId, rotation = rotation }, commandBuffer);
        }
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct ActorSyncFrame_T_R_NetMessage : IBufferElementData
    {
        public int actorId;//entity Index

        public float3 position;
        public quaternion rotation;
    }
    public struct ActorSyncFrame_T_R_Serialize : NetworkSerializeUnsafe.ISerializer//在PrefabSpawner.OnPrefabSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorSyncFrame_T_R;

        public int actorId;//entity Index

#if !ACTOR_2D_SYNC
        half3 _position;
        half4 _rotation;

        public float3 position
        {
            get => _position;
            set => _position = (half3)value;
        }
        public quaternion rotation
        {
            get => (float4)_rotation;
            set => _rotation = (half4)value.value;
        }
#else
        half2 _position;
        half _rotation;

        public float3 position
        {
            get => new float3(_position.x, 0f, _position.y);
            set => _position = (half2)value.xz;
        }
        public quaternion rotation
        {
            get => Quaternion.Euler(0f, _rotation, 0f);
            set => _rotation = (half)((Quaternion)value).eulerAngles.y;
        }
#endif

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new ActorSyncFrame_T_R_NetMessage { actorId = actorId, position = position, rotation = rotation }, commandBuffer);
        }
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct ActorSyncFrame_RB_VD_NetMessage : IBufferElementData
    {
        public int actorId;//entity Index

        public float3 linearVelocity;
        public float3 angularVelocity;
    }
    public struct ActorSyncFrame_RB_VD_Serialize : NetworkSerializeUnsafe.ISerializer//在PrefabSpawner.OnPrefabSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorSyncFrame_RB_VD;

        public int actorId;//entity Index

#if !ACTOR_2D_SYNC
        half3 _linearVelocity;
        half3 _angularVelocity;

        public float3 linearVelocity
        {
            get => _linearVelocity;
            set => _linearVelocity = (half3)value;
        }
        public float3 angularVelocity
        {
            get => _angularVelocity;
            set => _angularVelocity = (half3)value;
        }
#else
        half2 _linearVelocity;
        half _angularVelocity;

        public float3 linearVelocity
        {
            get => new float3(_linearVelocity.x, 0f, _linearVelocity.y);
            set => _linearVelocity = (half2)value.xz;
        }
        public float3 angularVelocity
        {
            get => new float3(0f, _angularVelocity, 0f);
            set => _angularVelocity = (half)value.y;
        }
#endif

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity, new ActorSyncFrame_RB_VD_NetMessage { actorId = actorId, linearVelocity = linearVelocity, angularVelocity = angularVelocity, }, commandBuffer);
        }
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct ActorSyncFrame_RB_T_V_NetMessage : IBufferElementData
    {
        public int actorId;//entity Index

        public float3 position;
        public float3 linearVelocity;
    }
    public struct ActorSyncFrame_RB_T_V_Serialize : NetworkSerializeUnsafe.ISerializer//在PrefabSpawner.OnPrefabSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorSyncFrame_RB_T_V;

        public int actorId;//entity Index

#if !ACTOR_2D_SYNC
        half3 _position;
        half3 _linearVelocity;

        public float3 position
        {
            get => _position;
            set => _position = (half3)value;
        }

        public float3 linearVelocity
        {
            get => _linearVelocity;
            set => _linearVelocity = (half3)value;
        }
#else
        public half2 _position;
        public half2 _linearVelocity;

        public float3 position
        {
            get => new float3(_position.x, 0f, _position.y);
            set => _position = (half2)value.xz;
        }

        public float3 linearVelocity
        {
            get => new float3(_linearVelocity.x, 0f, _linearVelocity.y);
            set => _linearVelocity = (half2)value.xz;
        }
#endif

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity,
                new ActorSyncFrame_RB_T_V_NetMessage { actorId = actorId, position = position, linearVelocity = linearVelocity },
                commandBuffer);
        }
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct ActorSyncFrame_RB_R_V_NetMessage : IBufferElementData
    {
        public int actorId;//entity Index

        public quaternion rotation;
        public float3 angularVelocity;
    }
    public struct ActorSyncFrame_RB_R_V_Serialize : NetworkSerializeUnsafe.ISerializer//在PrefabSpawner.OnPrefabSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorSyncFrame_RB_R_V;

        public int actorId;//entity Index

#if !ACTOR_2D_SYNC
        half4 _rotation;
        half3 _angularVelocity;

        public quaternion rotation
        {
            get => (float4)_rotation;
            set => _rotation = (half4)value.value;
        }

        public float3 angularVelocity
        {
            get => _angularVelocity;
            set => _angularVelocity = (half3)value;
        }
#else
        public half _rotation;
        public half _angularVelocity;

        public quaternion rotation
        {
            get => Quaternion.Euler(0f, _rotation, 0f);
            set => _rotation = (half)((Quaternion)value).eulerAngles.y;
        }

        public float3 angularVelocity
        {
            get => new float3(0f, _angularVelocity, 0f);
            set => _angularVelocity = (half)value.y;
        }
#endif

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity,
                new ActorSyncFrame_RB_R_V_NetMessage { actorId = actorId, rotation = rotation, angularVelocity = angularVelocity },
                commandBuffer);
        }
    }


    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct ActorSyncFrame_RB_T_R_V_NetMessage : IBufferElementData
    {
        public int actorId;//entity Index

        public float3 position;
        public quaternion rotation;
        public float3 linearVelocity;
        public float3 angularVelocity;
    }
    public struct ActorSyncFrame_RB_T_R_V_Serialize : NetworkSerializeUnsafe.ISerializer//在PrefabSpawner.OnPrefabSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.ActorSyncFrame_RB_T_R_V;

        public int actorId;//entity Index

#if !ACTOR_2D_SYNC
        half3 _position;
        half4 _rotation;
        half3 _linearVelocity;
        half3 _angularVelocity;

        public float3 position
        {
            get => _position;
            set => _position = (half3)value;
        }
        public quaternion rotation
        {
            get => (float4)_rotation;
            set => _rotation = (half4)value.value;
        }

        public float3 linearVelocity
        {
            get => _linearVelocity;
            set => _linearVelocity = (half3)value;
        }
        public float3 angularVelocity
        {
            get => _angularVelocity;
            set => _angularVelocity = (half3)value;
        }
#else
        public half2 _position;
        public half _rotation;
        public half2 _linearVelocity;
        public half _angularVelocity;

        public float3 position
        {
            get => new float3(_position.x, 0f, _position.y);
            set => _position = (half2)value.xz;
        }
        public quaternion rotation
        {
            get => Quaternion.Euler(0f, _rotation, 0f);
            set => _rotation = (half)((Quaternion)value).eulerAngles.y;
        }

        public float3 linearVelocity
        {
            get => new float3(_linearVelocity.x, 0f, _linearVelocity.y);
            set => _linearVelocity = (half2)value.xz;
        }
        public float3 angularVelocity
        {
            get => new float3(0f, _angularVelocity, 0f);
            set => _angularVelocity = (half)value.y;
        }
#endif

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity,
                new ActorSyncFrame_RB_T_R_V_NetMessage { actorId = actorId, position = position, rotation = rotation, linearVelocity = linearVelocity, angularVelocity = angularVelocity },
                commandBuffer);
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public partial class ActorSyncDatasServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        public struct UnreliableSyncActorDatasJob : IJobForEach_BBB
            <
            NetworkUnreliableOutBuffer,
            ActorDatas0Buffer,
            ActorDatas1Buffer
            //ActorDatas2Buffer,
            //ActorDatas3Buffer,
            //ActorDatas4Buffer,
            >
        {
            public void Execute
                (
                DynamicBuffer<NetworkUnreliableOutBuffer> outBuffer,
                [ReadOnly]DynamicBuffer<ActorDatas0Buffer> syncActorDatas0Buffer,
                [ReadOnly]DynamicBuffer<ActorDatas1Buffer> syncActorDatas1Buffer
                //[ReadOnly]DynamicBuffer<ActorDatas2Buffer> syncActorDatas2Buffer,
                //[ReadOnly]DynamicBuffer<ActorDatas3Buffer> syncActorDatas3Buffer,
                //[ReadOnly]DynamicBuffer<ActorDatas4Buffer> syncActorDatas4Buffer,
                )
            {
                for (int i = 0; i < syncActorDatas0Buffer.Length; ++i)
                {
                    if (syncActorDatas0Buffer[i].unreliable == false) continue;

                    var actorId = syncActorDatas0Buffer[i].actorId;
                    var synDataType = syncActorDatas0Buffer[i].synDataType;

                    var s = new ActorSyncFrame_Datas0_Serialize { actorId = actorId, synDataType = synDataType };
                    s._DoSerialize(outBuffer);
                }
                for (int i = 0; i < syncActorDatas1Buffer.Length; ++i)
                {
                    if (syncActorDatas1Buffer[i].unreliable == false) continue;

                    var actorId = syncActorDatas1Buffer[i].actorId;
                    var synDataType = syncActorDatas1Buffer[i].synDataType;
                    var datas = syncActorDatas1Buffer[i].floatValue;

                    var s = new ActorSyncFrame_Datas1_Serialize { actorId = actorId, synDataType = synDataType, floatValue = datas };
                    s._DoSerialize(outBuffer);
                }
                /*for (int i = 0; i < syncActorDatas2Buffer.Length; ++i)
                {
                    if (syncActorDatas2Buffer[i].unreliable == false) continue;

                    var actorId = syncActorDatas2Buffer[i].actorId;
                    var synDataType = syncActorDatas2Buffer[i].synDataType;
                    var datas = syncActorDatas2Buffer[i].datas;

                    var s = new ActorSyncFrame_Datas2_Serialize { actorId = actorId, synDataType = synDataType, float2Value = datas };
                    s._DoSerialize(outBuffer);
                }
                for (int i = 0; i < syncActorDatas3Buffer.Length; ++i)
                {
                    if (syncActorDatas3Buffer[i].unreliable == false) continue;

                    var actorId = syncActorDatas3Buffer[i].actorId;
                    var synDataType = syncActorDatas3Buffer[i].synDataType;
                    var datas = syncActorDatas3Buffer[i].datas;

                    var s = new ActorDatas3SyncFrameSerialize { actorId = actorId, synDataType = synDataType, datas = datas };
                    s._DoSerialize(outBuffer);
                }
                for (int i = 0; i < syncActorDatas4Buffer.Length; ++i)
                {
                    if (syncActorDatas4Buffer[i].unreliable == false) continue;

                    var actorId = syncActorDatas4Buffer[i].actorId;
                    var synDataType = syncActorDatas4Buffer[i].synDataType;
                    var datas = syncActorDatas4Buffer[i].datas;

                    var s = new ActorDatas4SyncFrameSerialize { actorId = actorId, synDataType = synDataType, datas = datas };
                    s._DoSerialize(outBuffer);
                }*/
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        public struct ReliableSyncActorDatasJob : IJobForEach_BBB
            <
            NetworkReliableOutBuffer,
            ActorDatas0Buffer,
            ActorDatas1Buffer
            //ActorDatas2Buffer,
            //ActorDatas3Buffer,
            //ActorDatas4Buffer,
            >
        {
            public void Execute
                (
                DynamicBuffer<NetworkReliableOutBuffer> outBuffer,
                [ReadOnly]DynamicBuffer<ActorDatas0Buffer> syncActorDatas0Buffer,
                [ReadOnly]DynamicBuffer<ActorDatas1Buffer> syncActorDatas1Buffer
                //[ReadOnly]DynamicBuffer<ActorDatas2Buffer> syncActorDatas2Buffer,
                //[ReadOnly]DynamicBuffer<ActorDatas3Buffer> syncActorDatas3Buffer,
                //[ReadOnly]DynamicBuffer<ActorDatas4Buffer> syncActorDatas4Buffer,
                )
            {
                for (int i = 0; i < syncActorDatas0Buffer.Length; ++i)
                {
                    if (syncActorDatas0Buffer[i].reliable == false) continue;

                    var actorId = syncActorDatas0Buffer[i].actorId;
                    var synDataType = syncActorDatas0Buffer[i].synDataType;

                    var s = new ActorSyncFrame_Datas0_Serialize { actorId = actorId, synDataType = synDataType };
                    s._DoSerialize(outBuffer);
                }
                for (int i = 0; i < syncActorDatas1Buffer.Length; ++i)
                {
                    if (syncActorDatas1Buffer[i].reliable == false) continue;

                    var actorId = syncActorDatas1Buffer[i].actorId;
                    var synDataType = syncActorDatas1Buffer[i].synDataType;
                    var datas = syncActorDatas1Buffer[i].floatValue;

                    var s = new ActorSyncFrame_Datas1_Serialize { actorId = actorId, synDataType = synDataType, floatValue = datas };
                    s._DoSerialize(outBuffer);
                }
                /*for (int i = 0; i < syncActorDatas2Buffer.Length; ++i)
                {
                    if (syncActorDatas2Buffer[i].reliable == false) continue;

                    var actorId = syncActorDatas2Buffer[i].actorId;
                    var synDataType = syncActorDatas2Buffer[i].synDataType;
                    var datas = syncActorDatas2Buffer[i].datas;

                    var s = new ActorSyncFrame_Datas2_Serialize { actorId = actorId, synDataType = synDataType, float2Value = datas };
                    s._DoSerialize(outBuffer);
                }
                for (int i = 0; i < syncActorDatas3Buffer.Length; ++i)
                {
                    if (syncActorDatas3Buffer[i].reliable == false) continue;

                    var actorId = syncActorDatas3Buffer[i].actorId;
                    var synDataType = syncActorDatas3Buffer[i].synDataType;
                    var datas = syncActorDatas3Buffer[i].datas;

                    var s = new ActorDatas3SyncFrameSerialize { actorId = actorId, synDataType = synDataType, datas = datas };
                    s._DoSerialize(outBuffer);
                }
                for (int i = 0; i < syncActorDatas4Buffer.Length; ++i)
                {
                    if (syncActorDatas4Buffer[i].reliable == false) continue;

                    var actorId = syncActorDatas4Buffer[i].actorId;
                    var synDataType = syncActorDatas4Buffer[i].synDataType;
                    var datas = syncActorDatas4Buffer[i].datas;

                    var s = new ActorDatas4SyncFrameSerialize { actorId = actorId, synDataType = synDataType, datas = datas };
                    s._DoSerialize(outBuffer);
                }*/
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SyncActorJob : IJobForEach_BB<NetworkUnreliableOutBuffer, ObserverSyncVisibleActorBuffer>
        {
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Rotation> rotationFromEntity;
            [ReadOnly] public ComponentDataFromEntity<RigidbodyVelocity> rigidbodyVelocityFromEntity;

            public void Execute(DynamicBuffer<NetworkUnreliableOutBuffer> outBuffer, [ReadOnly]DynamicBuffer<ObserverSyncVisibleActorBuffer> synVisibleActor)
            {
                for (int i = 0; i < synVisibleActor.Length; ++i)
                {
                    var synDataType = synVisibleActor[i].synActorType;
                    var actorEntity = synVisibleActor[i].actorEntity;


                    if (synDataType == SyncActorType.Translation)
                    {
                        var position = translationFromEntity[actorEntity].Value;

                        var s = new ActorSyncFrame_T_Serialize { actorId = actorEntity.Index, position = position };
                        s._DoSerialize(outBuffer);
                    }
                    else if (synDataType == SyncActorType.Rotation)
                    {
                        var position = translationFromEntity[actorEntity].Value;
                        var rotation = rotationFromEntity[actorEntity].Value;

                        var s = new ActorSyncFrame_R_Serialize { actorId = actorEntity.Index, rotation = rotation };
                        s._DoSerialize(outBuffer);
                    }
                    else if (synDataType == SyncActorType.Translation_Rotation)
                    {
                        var position = translationFromEntity[actorEntity].Value;
                        var rotation = rotationFromEntity[actorEntity].Value;

                        var s = new ActorSyncFrame_T_R_Serialize { actorId = actorEntity.Index, position = position, rotation = rotation };
                        s._DoSerialize(outBuffer);
                    }
                    else if (synDataType == SyncActorType.RB_VelocityDirection)
                    {
                        var rigidbodyVelocity = rigidbodyVelocityFromEntity[actorEntity];
                        var linearVelocity = rigidbodyVelocity.linear;
                        var angularVelocity = rigidbodyVelocity.angular;

                        var s = new ActorSyncFrame_RB_VD_Serialize
                        {
                            actorId = actorEntity.Index,
                            linearVelocity = linearVelocity,
                            angularVelocity = angularVelocity
                        };
                        s._DoSerialize(outBuffer);
                    }
                    else if (synDataType == SyncActorType.RB_Translation_Velocity)
                    {
                        var position = translationFromEntity[actorEntity].Value;

                        var rigidbodyVelocity = rigidbodyVelocityFromEntity[actorEntity];
                        var linearVelocity = rigidbodyVelocity.linear;

                        var s = new ActorSyncFrame_RB_T_V_Serialize
                        {
                            actorId = actorEntity.Index,
                            position = position,
                            linearVelocity = linearVelocity,
                        };
                        s._DoSerialize(outBuffer);
                    }
                    else if (synDataType == SyncActorType.RB_Rotation_Velocity)
                    {
                        var rotation = rotationFromEntity[actorEntity].Value;

                        var rigidbodyVelocity = rigidbodyVelocityFromEntity[actorEntity];
                        var angularVelocity = rigidbodyVelocity.angular;

                        var s = new ActorSyncFrame_RB_R_V_Serialize
                        {
                            actorId = actorEntity.Index,
                            rotation = rotation,
                            angularVelocity = angularVelocity
                        };
                        s._DoSerialize(outBuffer);
                    }
                    else if (synDataType == SyncActorType.RB_Translation_Rotation_Velocity)
                    {
                        var position = translationFromEntity[actorEntity].Value;
                        var rotation = rotationFromEntity[actorEntity].Value;

                        var rigidbodyVelocity = rigidbodyVelocityFromEntity[actorEntity];
                        var linearVelocity = rigidbodyVelocity.linear;
                        var angularVelocity = rigidbodyVelocity.angular;

                        var s = new ActorSyncFrame_RB_T_R_V_Serialize
                        {
                            actorId = actorEntity.Index,
                            position = position,
                            rotation = rotation,
                            linearVelocity = linearVelocity,
                            angularVelocity = angularVelocity
                        };
                        s._DoSerialize(outBuffer);
                    }
                    else
                    {
                        throw new System.Exception($"synDataType={synDataType}");
                    }
                }
            }
        }

        protected override void OnCreate()
        {
            enterGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerEnterGameMessage>()/*, ComponentType.ReadOnly<Player>()*/ },
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });
        }
        EntityQuery enterGameQuery;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //enter Game
            if (enterGameQuery.IsEmptyIgnoreFilter == false)
            {
                EntityManager.AddComponent<ActorDatas0Buffer>(enterGameQuery);
                EntityManager.AddComponent<ActorDatas1Buffer>(enterGameQuery);
                //EntityManager.AddComponent<ActorDatas2Buffer>(enterGameQuery);
                //EntityManager.AddComponent<ActorDatas3Buffer>(enterGameQuery);
                //EntityManager.AddComponent<ActorDatas4Buffer>(enterGameQuery);
            }


            //
            var inputDepsA = new SyncActorJob
            {
                translationFromEntity = GetComponentDataFromEntity<Translation>(true),
                rotationFromEntity = GetComponentDataFromEntity<Rotation>(true),
                rigidbodyVelocityFromEntity = GetComponentDataFromEntity<RigidbodyVelocity>(true),
            }
            .Schedule(this, inputDeps);

            var inputDepsB = new ReliableSyncActorDatasJob
            {
            }
            .Schedule(this, inputDeps);

            var inputDepsC = new UnreliableSyncActorDatasJob
            {
            }
            .Schedule(this, inputDepsA);

            return JobHandle.CombineDependencies(inputDepsB, inputDepsC);
        }

    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public partial class ActorSyncDatasClientSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(NetworkConnection))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        public struct Sync_Translation_Job : IJobForEach_BBBB<ActorSyncFrame_T_NetMessage, ActorSyncFrame_RB_T_V_NetMessage, ActorSyncFrame_T_R_NetMessage, ActorSyncFrame_RB_T_R_V_NetMessage>
        {
            public float fixedDeltaTime;
            [ReadOnly] public NativeHashMap<int, Entity> actorEntityFromPlayerId;
            public ComponentDataFromEntity<Translation> translationFromEntity;
            public void Execute(
                [ReadOnly]DynamicBuffer<ActorSyncFrame_T_NetMessage> T_NetMessages,
                [ReadOnly]DynamicBuffer<ActorSyncFrame_RB_T_V_NetMessage> RB_T_V_NetMessages,
                [ReadOnly]DynamicBuffer<ActorSyncFrame_T_R_NetMessage> RP_NetMessages,
                [ReadOnly]DynamicBuffer<ActorSyncFrame_RB_T_R_V_NetMessage> RB_T_R_V_NetMessages)
            {
                for (int i = 0; i < T_NetMessages.Length; ++i)
                {
                    if (actorEntityFromPlayerId.TryGetValue(T_NetMessages[i].actorId, out Entity actorEntity))
                    {
                        translationFromEntity[actorEntity] = new Translation { Value = T_NetMessages[i].position };
                    }
                }
                for (int i = 0; i < RP_NetMessages.Length; ++i)
                {
                    if (actorEntityFromPlayerId.TryGetValue(RP_NetMessages[i].actorId, out Entity actorEntity))
                    {
                        translationFromEntity[actorEntity] = new Translation { Value = RP_NetMessages[i].position };
                    }
                }
                for (int i = 0; i < RB_T_V_NetMessages.Length; ++i)
                {
                    if (actorEntityFromPlayerId.TryGetValue(RB_T_V_NetMessages[i].actorId, out Entity actorEntity))
                    {
                        translationFromEntity[actorEntity] = new Translation { Value = RB_T_V_NetMessages[i].position + RB_T_V_NetMessages[i].linearVelocity * fixedDeltaTime };//预判刚体位置
                    }
                }
                for (int i = 0; i < RB_T_R_V_NetMessages.Length; ++i)
                {
                    if (actorEntityFromPlayerId.TryGetValue(RB_T_R_V_NetMessages[i].actorId, out Entity actorEntity))
                    {
                        translationFromEntity[actorEntity] = new Translation { Value = RB_T_R_V_NetMessages[i].position + RB_T_R_V_NetMessages[i].linearVelocity * fixedDeltaTime };//预判刚体位置
                    }
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(NetworkConnection))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        public struct Sync_Rotation_Job : IJobForEach_BBBB<ActorSyncFrame_R_NetMessage, ActorSyncFrame_T_R_NetMessage, ActorSyncFrame_RB_R_V_NetMessage, ActorSyncFrame_RB_T_R_V_NetMessage>
        {
            public float fixedDeltaTime;
            [ReadOnly] public NativeHashMap<int, Entity> actorEntityFromPlayerId;
            public ComponentDataFromEntity<Rotation> rotationFromEntity;
            public void Execute(
                [ReadOnly]DynamicBuffer<ActorSyncFrame_R_NetMessage> R_NetMessages,
                [ReadOnly]DynamicBuffer<ActorSyncFrame_T_R_NetMessage> T_R_NetMessages,
                [ReadOnly]DynamicBuffer<ActorSyncFrame_RB_R_V_NetMessage> RB_R_V_NetMessages,
                [ReadOnly]DynamicBuffer<ActorSyncFrame_RB_T_R_V_NetMessage> RB_T_R_V_NetMessages)
            {
                for (int i = 0; i < R_NetMessages.Length; ++i)
                {
                    if (actorEntityFromPlayerId.TryGetValue(R_NetMessages[i].actorId, out Entity actorEntity))
                    {
                        rotationFromEntity[actorEntity] = new Rotation { Value = R_NetMessages[i].rotation };
                    }
                }
                for (int i = 0; i < T_R_NetMessages.Length; ++i)
                {
                    if (actorEntityFromPlayerId.TryGetValue(T_R_NetMessages[i].actorId, out Entity actorEntity))
                    {
                        rotationFromEntity[actorEntity] = new Rotation { Value = T_R_NetMessages[i].rotation };
                    }
                }
                for (int i = 0; i < RB_R_V_NetMessages.Length; ++i)
                {
                    if (actorEntityFromPlayerId.TryGetValue(RB_R_V_NetMessages[i].actorId, out Entity actorEntity))
                    {
                        rotationFromEntity[actorEntity] = new Rotation { Value = RB_R_V_NetMessages[i].rotation * Quaternion.Euler(RB_R_V_NetMessages[i].angularVelocity * fixedDeltaTime) };
                    }
                }
                for (int i = 0; i < RB_T_R_V_NetMessages.Length; ++i)
                {
                    if (actorEntityFromPlayerId.TryGetValue(RB_T_R_V_NetMessages[i].actorId, out Entity actorEntity))
                    {
                        rotationFromEntity[actorEntity] = new Rotation { Value = RB_T_R_V_NetMessages[i].rotation * Quaternion.Euler(RB_T_R_V_NetMessages[i].angularVelocity * fixedDeltaTime) };
                    }
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(NetworkConnection))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        public struct Sync_RigidbodyVelocity_Job : IJobForEach_BBBB<ActorSyncFrame_RB_VD_NetMessage, ActorSyncFrame_RB_T_V_NetMessage, ActorSyncFrame_RB_R_V_NetMessage, ActorSyncFrame_RB_T_R_V_NetMessage>
        {
            [ReadOnly] public NativeHashMap<int, Entity> actorEntityFromPlayerId;
            public ComponentDataFromEntity<RigidbodyVelocity> rigidbodyVelocityFromEntity;
            public void Execute(
                [ReadOnly]DynamicBuffer<ActorSyncFrame_RB_VD_NetMessage> RB_VD_NetMessages,
                [ReadOnly]DynamicBuffer<ActorSyncFrame_RB_T_V_NetMessage> RB_T_V_NetMessages,
                [ReadOnly]DynamicBuffer<ActorSyncFrame_RB_R_V_NetMessage> RB_R_V_NetMessages,
                [ReadOnly]DynamicBuffer<ActorSyncFrame_RB_T_R_V_NetMessage> RB_T_R_V_NetMessages)
            {
                for (int i = 0; i < RB_VD_NetMessages.Length; ++i)
                {
                    if (actorEntityFromPlayerId.TryGetValue(RB_VD_NetMessages[i].actorId, out Entity actorEntity))
                    {
                        rigidbodyVelocityFromEntity[actorEntity] = new RigidbodyVelocity { linear = RB_VD_NetMessages[i].linearVelocity, angular = RB_VD_NetMessages[i].angularVelocity };
                    }
                }
                for (int i = 0; i < RB_T_V_NetMessages.Length; ++i)
                {
                    if (actorEntityFromPlayerId.TryGetValue(RB_T_V_NetMessages[i].actorId, out Entity actorEntity))
                    {
                        rigidbodyVelocityFromEntity[actorEntity] = new RigidbodyVelocity { linear = RB_T_V_NetMessages[i].linearVelocity };
                    }
                }
                for (int i = 0; i < RB_R_V_NetMessages.Length; ++i)
                {
                    if (actorEntityFromPlayerId.TryGetValue(RB_R_V_NetMessages[i].actorId, out Entity actorEntity))
                    {
                        rigidbodyVelocityFromEntity[actorEntity] = new RigidbodyVelocity { angular = RB_R_V_NetMessages[i].angularVelocity };
                    }
                }
                for (int i = 0; i < RB_T_R_V_NetMessages.Length; ++i)
                {
                    if (actorEntityFromPlayerId.TryGetValue(RB_T_R_V_NetMessages[i].actorId, out Entity actorEntity))
                    {
                        rigidbodyVelocityFromEntity[actorEntity] = new RigidbodyVelocity { linear = RB_T_R_V_NetMessages[i].linearVelocity, angular = RB_T_R_V_NetMessages[i].angularVelocity };
                    }
                }
            }
        }

        ActorSyncCreateClientSystem actorClientSystem;

        EntityQuery networkConnectedMessageQuery;

        protected override void OnCreate()
        {
            actorClientSystem = World.GetExistingSystem<ActorSyncCreateClientSystem>();

            networkConnectedMessageQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<NetworkConnection>(), ComponentType.ReadOnly<NetworkConnectedMessage>() },
                None = new ComponentType[] { ComponentType.ReadOnly<NetworkDisconnectedMessage>() },
            });
        }

        void onNetworkConnectedMessage()
        {
            if (networkConnectedMessageQuery.IsEmptyIgnoreFilter == false)
            {
                using (var entitys = networkConnectedMessageQuery.ToEntityArray(Allocator.TempJob))
                {
                    EntityManager.AddComponent<ActorSyncFrame_T_NetMessage>(entitys);
                    EntityManager.AddComponent<ActorSyncFrame_R_NetMessage>(entitys);
                    EntityManager.AddComponent<ActorSyncFrame_T_R_NetMessage>(entitys);
                    EntityManager.AddComponent<ActorSyncFrame_RB_VD_NetMessage>(entitys);
                    EntityManager.AddComponent<ActorSyncFrame_RB_R_V_NetMessage>(entitys);
                    EntityManager.AddComponent<ActorSyncFrame_RB_T_V_NetMessage>(entitys);
                    EntityManager.AddComponent<ActorSyncFrame_RB_T_R_V_NetMessage>(entitys);

                    EntityManager.AddComponent<ActorDatas0Buffer>(entitys);
                    EntityManager.AddComponent<ActorDatas1Buffer>(entitys);
                    //EntityManager.AddComponent<ActorDatas2Buffer>(entitys);
                    //EntityManager.AddComponent<ActorDatas3Buffer>(entitys);
                    //EntityManager.AddComponent<ActorDatas4Buffer>(entitys);
                }
            }
        }

        public float translationPredictScale = 1.5f;
        public float rotationPredictScale = 75f;
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //
            onNetworkConnectedMessage();

            //
            var inputDepsA = new Sync_Translation_Job
            {
                fixedDeltaTime = Time.fixedDeltaTime * translationPredictScale,
                actorEntityFromPlayerId = actorClientSystem.actorEntityFromActorId,
                translationFromEntity = GetComponentDataFromEntity<Translation>(),
            }
            .ScheduleSingle(this, inputDeps);

            var inputDepsB = new Sync_Rotation_Job
            {
                fixedDeltaTime = Time.fixedDeltaTime * rotationPredictScale,
                actorEntityFromPlayerId = actorClientSystem.actorEntityFromActorId,
                rotationFromEntity = GetComponentDataFromEntity<Rotation>(),
            }
            .ScheduleSingle(this, inputDeps);

            var inputDepsC = new Sync_RigidbodyVelocity_Job
            {
                actorEntityFromPlayerId = actorClientSystem.actorEntityFromActorId,
                rigidbodyVelocityFromEntity = GetComponentDataFromEntity<RigidbodyVelocity>(),
            }
            .ScheduleSingle(this, inputDeps);


            return JobHandle.CombineDependencies(inputDepsA, inputDepsB, inputDepsC);
        }
    }
}