using UnityEngine;
using Unity.Networking.Transport;
using Unity.Networking.Transport.LowLevel.Unsafe;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using System;

namespace RN.Network
{
    public enum SC_CS
    {
        server2client,
        client2server,
    }
    public interface IExecute
    {
        void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer);
    }

    public static class NetworkSerializeUnsafe
    {
        public interface ISerializer : IExecute
        {
            SC_CS sc_cs { get; }

            short Type { get; }
        }


        public static unsafe void _DoDeserialize<T>(this ref T _this, DataStreamReader reader, ref DataStreamReader.Context ctx)
            where T : unmanaged, ISerializer
        {
            var Tsize = sizeof(T);
            //var Tsize = UnsafeUtility.SizeOf<T>();
            var unreadLength = reader.Length - reader.GetBytesRead(ref ctx);
            if (Tsize > unreadLength)
            {
                throw new Exception($"T_size > unreadLength  unreadLength={unreadLength}  T_size={Tsize}  T={typeof(T)}");
            }

            var buffer = stackalloc byte[Tsize];
            reader.ReadBytes(ref ctx, buffer, Tsize);

            UnsafeUtility.MemCpy(UnsafeUtility.AddressOf(ref _this), buffer, Tsize);
        }


        const int type_size = sizeof(short);
        public static unsafe void _DoSerialize<T, TBufferElementData>(this ref T _this, DynamicBuffer<TBufferElementData> out_buffer)
            where T : unmanaged, ISerializer
            where TBufferElementData : struct, IBufferElementData
        {
            var Tsize = sizeof(T);
            //var Tsize = UnsafeUtility.SizeOf<T>();

            var prev_out_buffer_length = out_buffer.Length;
            out_buffer.ResizeUninitialized(prev_out_buffer_length + type_size + Tsize);

            //Debug.Assert(out_buffer.Length == (prev_out_buffer_length + type_size + Tsize));

            byte* out_buffer_ptr = (byte*)out_buffer.GetUnsafePtr();
            var _this_type = _this.Type;
            UnsafeUtility.MemCpy(out_buffer_ptr + prev_out_buffer_length, &_this_type, type_size);
            UnsafeUtility.MemCpy(out_buffer_ptr + prev_out_buffer_length + type_size, UnsafeUtility.AddressOf(ref _this), Tsize);
        }


        public static void AutoAddBufferMessage<T>(int index, Entity entity, in T data, EntityCommandBuffer.Concurrent commandBuffer)
            where T : struct, IBufferElementData
        {
            commandBuffer.AddBuffer<T>(index, entity).Add(data);
        }
    }


    public static class NetworkSerialize
    {
        public interface ISerializer : IExecute
        {
            SC_CS sc_cs { get; }

            short Type { get; }

            void Serialize(DataStreamWriter writer);
            void Deserialize(DataStreamReader reader, ref DataStreamReader.Context ctx);
        }


        public static void _DoSerializeInJob<T, TBufferElementData>(this ref T _this, DynamicBuffer<TBufferElementData> out_buffer)
            where T : struct, ISerializer
            where TBufferElementData : struct, IBufferElementData
        {
            var writer = new DataStreamWriter(128, Allocator.Temp);

            writer.Write(_this.Type);
            _this.Serialize(writer);

            memCpy(out_buffer, writer);

            //writer.Dispose();
        }

        public static void _DoSerialize<T, TBufferElementData>(this ref T _this, DynamicBuffer<TBufferElementData> out_buffer)
            where T : struct, ISerializer
            where TBufferElementData : struct, IBufferElementData
        {
            using (var writer = new DataStreamWriter(128, Allocator.Temp))
            {
                writer.Write(_this.Type);
                _this.Serialize(writer);

                memCpy(out_buffer, writer);
            }
        }

        static unsafe void memCpy<T>(DynamicBuffer<T> destination, DataStreamWriter source) where T : struct
        {
            var prev_destination_length = destination.Length;
            destination.ResizeUninitialized(destination.Length + source.Length);
            byte* destination_ptr = (byte*)destination.GetUnsafePtr();
            UnsafeUtility.MemCpy(destination_ptr + prev_destination_length, source.GetUnsafeReadOnlyPtr(), source.Length);
        }
    }

#if true
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public partial class NetworkStreamSerializeSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }


        const int type_size = sizeof(short);

        //[BurstCompile]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        partial struct SerializeJob : IJobForEachWithEntity_EB<NetworkInBuffer>
        {
            public EntityCommandBuffer.Concurrent commandBuffer;

            public unsafe void Execute(Entity entity, int index, DynamicBuffer<NetworkInBuffer> inBuffer)
            {
                if (inBuffer.Length <= 0) return;

                var ctx = default(DataStreamReader.Context);
                var reader = DataStreamUnsafeUtility.CreateReaderFromExistingData((byte*)inBuffer.GetUnsafePtr(), inBuffer.Length);
                inBuffer.Clear();


                var unreadLength = 0;
                while ((unreadLength = reader.Length - reader.GetBytesRead(ref ctx)) > 0)
                {
                    if (type_size > unreadLength)
                    {
                        commandBuffer.AddComponent(index, entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrors.Serialize_Type_Size });
                        break;
                    }


                    //
                    try
                    {
                        short type = reader.ReadShort(ref ctx);
                        OnExecute(index, entity, type, reader, ref ctx, commandBuffer);
                    }
                    catch (Exception e)
                    {
                        commandBuffer.AddComponent(index, entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrors.Serialize_Exception });
                        Debug.LogException(e);
                        break;
                    }
                }
            }

            partial void OnExecute
                (
                    int index, Entity entity,
                    short type,
                    DataStreamReader reader, ref DataStreamReader.Context ctx,
                    EntityCommandBuffer.Concurrent commandBuffer
                );
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            using (var commandBuffer = new EntityCommandBuffer(Allocator.TempJob))
            {
                inputDeps = new SerializeJob
                {
                    //isServer = World == ServerBootstrap.world,
                    commandBuffer = commandBuffer.ToConcurrent(),
                }
                .Schedule(this, inputDeps);

                inputDeps.Complete();
                commandBuffer.Playback(EntityManager);
            }

            return inputDeps;
        }
    }

#else
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public partial class NetworkStreamSerializeSystem : ComponentSystem
    {
        partial void GetSerializeCollectionCount(ref int serializeCollectionCount);
        partial void GetSerializeType(short serializeType, ref string serializeTypeStr);

        const int type_size = sizeof(short);

        //
        public delegate IExecute Serialize(DataStreamReader reader, ref DataStreamReader.Context ctx);
        private Serialize[] serializeCollection;
        //static int serializeCollectionCount = (int)NetworkSerializeType._MaxCount;

        public NetworkStreamSerializeSystem SetSerializerUnsafe<T>(T serializer) where T : unmanaged, NetworkSerializeUnsafe.ISerializer
        {
            if (serializeCollection[serializer.Type] != null)
            {
                var serializeType = "";
                GetSerializeType(serializer.Type, ref serializeType);
                Debug.LogError($"serializeCollection[{serializeType}] != null");
                return null;
            }

            if (ClientBootstrap.worlds != null && ClientBootstrap.worlds.find(x => x == World) != null)
            {
                if (serializer.sc_cs == SC_CS.server2client)
                {
                    serializeCollection[serializer.Type] = (DataStreamReader reader, ref DataStreamReader.Context ctx) =>
                    {
                        serializer._DoDeserialize(reader, ref ctx);
                        return serializer;
                    };

                    return this;
                }
            }

            if (ServerBootstrap.world != null && ServerBootstrap.world == World)
            {
                if (serializer.sc_cs == SC_CS.client2server)
                {
                    serializeCollection[serializer.Type] = (DataStreamReader reader, ref DataStreamReader.Context ctx) =>
                    {
                        serializer._DoDeserialize(reader, ref ctx);
                        return serializer;
                    };
                    return this;
                }
            }

            //Debug.LogError($"{World.Name}=>error:  sc_cs={serializer.sc_cs}  Type={serializer.Type}");
            return this;
        }
        public NetworkStreamSerializeSystem SetSerializer<T>(T serializer) where T : struct, NetworkSerialize.ISerializer
        {
            if (serializeCollection[serializer.Type] != null)
            {
                var serializeType = "";
                GetSerializeType(serializer.Type, ref serializeType);
                Debug.LogError($"serializeCollection[{serializeType}] != null");
                return null;
            }

            if (ClientBootstrap.worlds != null && ClientBootstrap.worlds.find(x => x == World) != null)
            {
                if (serializer.sc_cs == SC_CS.server2client)
                {
                    serializeCollection[serializer.Type] = (DataStreamReader reader, ref DataStreamReader.Context ctx) =>
                    {
                        serializer.Deserialize(reader, ref ctx);
                        return serializer;
                    };
                    return this;
                }
            }

            if (ServerBootstrap.world != null && ServerBootstrap.world == World)
            {
                if (serializer.sc_cs == SC_CS.client2server)
                {
                    serializeCollection[serializer.Type] = (DataStreamReader reader, ref DataStreamReader.Context ctx) =>
                    {
                        serializer.Deserialize(reader, ref ctx);
                        return serializer;
                    };
                    return this;
                }
            }

            //Debug.LogError($"{World.Name}=>error:  sc_cs={serializer.sc_cs}  Type={serializer.Type}");
            return this;
        }

        EndFixedCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            var serializeCollectionCount = 0;
            GetSerializeCollectionCount(ref serializeCollectionCount);
            serializeCollection = new Serialize[serializeCollectionCount];

            endBarrier = World.GetExistingSystem<EndFixedCommandBufferSystem>();
        }

        partial struct SerializeJob
        {
            partial void OnExecute
                (
                    int index, Entity entity,
                    short type,
                    DataStreamReader read, ref DataStreamReader.Context ctx,
                    EntityCommandBuffer.Concurrent commandBuffer, EntityCommandBuffer.Concurrent endCommandBuffer,
                    ref bool error
                );
        }
        SerializeJob serializeJob = new SerializeJob { };
        protected override unsafe void OnUpdate()
        {
            var commandBuffer = PostUpdateCommands.ToConcurrent();
            var endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent();

            var index = 0;
            Entities
                .WithAll<NetworkInBuffer>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity entity, DynamicBuffer<NetworkInBuffer> inBuffer) =>
                {
                    if (inBuffer.Length <= 0) return;
#if true
                    using (var writer = new DataStreamWriter(inBuffer.Length, Allocator.Temp))
                    {
                        writer.WriteBytes((byte*)inBuffer.GetUnsafePtr(), inBuffer.Length);
                        inBuffer.Clear();
                        var reader = new DataStreamReader(writer, 0, writer.Length);
#else
                    {
                        var reader = DataStreamUnsafeUtility.CreateReaderFromExistingData((byte*)inBuffer.GetUnsafePtr(), inBuffer.Length);
#endif
                        var ctx = default(DataStreamReader.Context);

                        var unreadLength = 0;
                        while ((unreadLength = reader.Length - reader.GetBytesRead(ref ctx)) > 0)
                        {
                            if (type_size > unreadLength)
                            {
                                Debug.LogError($"{World.Name} => type_size > unreadLength  type_size={type_size}  unreadLength={unreadLength}");
                                commandBuffer.AddComponent(index, entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrors.T_size_Greater_inBufferLength });
                                break;
                            }


                            short type = reader.ReadShort(ref ctx);
#if UNITY_EDITOR
                            if (type != (short)NetworkSerializeType.SyncFrame_P_R_RB
                            && type != (short)NetworkSerializeType.PlayerActorFireInput
                            && type != (short)NetworkSerializeType.PlayerActorMoveInput
                            && type != (short)NetworkSerializeType.NetworkId
                            && type != (short)NetworkSerializeType.NetworkVersion
                            && type != (short)NetworkSerializeType.NetworkVersionResult
                            && type != (short)NetworkSerializeType.PlayerEnterGame
                            && type != (short)NetworkSerializeType.PlayerCreate
                            && type != (short)NetworkSerializeType.PlayerName
                            && type != (short)NetworkSerializeType.PlayerTeam
                            && type != (short)NetworkSerializeType.PlayerGameReady
                            && type != (short)NetworkSerializeType.PlayerGameStart
                            && type != (short)NetworkSerializeType.ActorCreate
                            && type != (short)NetworkSerializeType.ActorDestroy
                            )
                            {
                                var _serializeTypeStr = "";
                                GetSerializeType(type, ref _serializeTypeStr);
                                Debug.Log($"{World.Name} => NetworkStreamSerializeSystem type={_serializeTypeStr}");
                            }
#endif

                            if (type >= serializeCollection.Length)
                            {
                                var serializeTypeStr = "";
                                GetSerializeType(type, ref serializeTypeStr);
                                Debug.LogError($"{World.Name} => type >= serializeCollection.Length  type={serializeTypeStr}  serializeCollection.Length={serializeCollection.Length}");
                                commandBuffer.AddComponent(index, entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrors.Unknow_Serialize_Type_A });
                                break;
                            }
                            if (serializeCollection[type] == null)
                            {
                                var serializeTypeStr = "";
                                GetSerializeType(type, ref serializeTypeStr);
                                Debug.LogError($"{World.Name} => serializeCollection[{serializeTypeStr}] == null  type={type}  unreadLength={unreadLength}  reader.Length={reader.Length}");

                                var shortstr = "";
                                while ((unreadLength = reader.Length - reader.GetBytesRead(ref ctx)) >= 2)
                                {
                                    shortstr += reader.ReadShort(ref ctx) + ",";
                                }
                                Debug.LogError($"{World.Name} => shortstr={shortstr}");

                                commandBuffer.AddComponent(index, entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrors.Unknow_Serialize_Type_B });
                                break;
                            }

                            try
                            {
                                var execute = serializeCollection[type](reader, ref ctx);
                                execute.Execute(index, entity, commandBuffer);

                                /*bool error = false;
                                serializeJob.OnExecute(index, entity, type, reader, ref ctx, commandBuffer, endCommandBuffer, ref error);
                                if (error)
                                {
                                    commandBuffer.AddComponent(entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrors.Serialize_Exception });
                                    break;
                                }*/
                            }
                            catch (Exception e)
                            {
                                var serializeTypeStr = "";
                                GetSerializeType(type, ref serializeTypeStr);
                                Debug.LogError($"{World.Name} => error={serializeTypeStr}");
                                Debug.LogException(e);
                                commandBuffer.AddComponent(0, entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrors.Serialize_Exception });

                                break;
                            }
                        }
                    }
                });
        }
    }
#endif
}