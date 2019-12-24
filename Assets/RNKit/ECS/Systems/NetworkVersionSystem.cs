using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace RN.Network
{
    [ServerNetworkEntity]
    [ServerAutoClear]
    public struct NetworkVersionNetMessage : IComponentData
    {
        public byte valueA;
        public byte valueB;
        public byte valueC;
    }

    public struct NetworkVersionSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.client2server;
        public short Type => (short)NetworkSerializeType.NetworkVersion;

        public byte valueA;
        public byte valueB;
        public byte valueC;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.AddComponent(index, entity, new NetworkVersionNetMessage { valueA = valueA, valueB = valueB, valueC = valueC });
        }
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct NetworkVersionResultNetMessage : IComponentData
    {
        public byte valueA;
        public byte valueB;
        public byte valueC;
    }

    public unsafe struct NetworkVersionResultSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.NetworkVersionResult;

        public byte valueA;
        public byte valueB;
        public byte valueC;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.AddComponent(index, entity, new NetworkVersionResultNetMessage { valueA = valueA, valueB = valueB, valueC = valueC });
        }
    }


    [DisableAutoCreation]
    public class NetworkVersionSystem : ComponentSystem
    {
        event System.Action<bool, NetworkVersionResultNetMessage> _versionResult;
        public event System.Action<bool, NetworkVersionResultNetMessage> versionResult
        {
            add
            {
                Enabled = true;
                _versionResult += value;
            }
            remove => _versionResult -= value;
        }

        protected override void OnDestroy()
        {
            _versionResult = null;
        }


        private byte ValueA;
        private byte ValueB;
        private byte ValueC;

        EndCommandBufferSystem endBarrier;
        protected void OnInit(Transform root)
        {
            //
            if (World != ServerBootstrap.world)
            {
                World.GetExistingSystem<NetworkStreamConnectSuccessSystem>().AddSystem(this);
            }

            //
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();

            var vs = Application.version.Split('.');
            ValueA = byte.Parse(vs[0]);
            ValueB = byte.Parse(vs[1]);
            ValueC = byte.Parse(vs[2]);
        }


        protected override void OnUpdate()
        {
            if (World == ServerBootstrap.world)
            {
                //S2C
                Entities
                    .WithAll<NetworkReliableOutBuffer>().WithAllReadOnly<NetworkVersionNetMessage>()
                    .WithNone<NetworkDisconnectedMessage>()
                    .ForEach((Entity entity, DynamicBuffer<NetworkReliableOutBuffer> outBuffer, ref NetworkVersionNetMessage version) =>
                    {
                        var s = new NetworkVersionResultSerialize { valueA = ValueA, valueB = ValueB, valueC = ValueC };
                        s._DoSerialize(outBuffer);


                        if (ValueA == version.valueA
                        && ValueB == version.valueB
                        /*&& ValueC == version.ValueC*/)
                        {
                            //success
                        }
                        else
                        {
                            //faild
                            PostUpdateCommands.AddComponent(entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrorsInSystem.Version });
                        }
                    });
            }
            else
            {
                var endCommandBuffer = endBarrier.CreateCommandBuffer();

                //C2S
                Entities
                    .WithAll<NetworkReliableOutBuffer>().WithAllReadOnly<NetworkConnection, NetworkConnectedMessage>()
                    .WithNone<NetworkDisconnectedMessage, NetworkVersionNetMessage>()
                    .ForEach((Entity entity, DynamicBuffer<NetworkReliableOutBuffer> outBuffer) =>
                    {
                        var s = new NetworkVersionSerialize { valueA = ValueA, valueB = ValueB, valueC = ValueC };
                        s._DoSerialize(outBuffer);

                        EntityManager.AddComponentData(entity, new NetworkVersionNetMessage { valueA = ValueA, valueB = ValueB, valueC = ValueC });
                    });



                //Result
                Entities
                    .WithAllReadOnly<NetworkConnection, NetworkVersionNetMessage, NetworkVersionResultNetMessage>()
                    .WithNone<NetworkDisconnectedMessage>()
                    .ForEach(((Entity entity, ref NetworkVersionNetMessage version, ref NetworkVersionResultNetMessage networkVersionResultNetMessage) =>
                    {
                        var success = false;

                        if (version.valueA == networkVersionResultNetMessage.valueA
                        && version.valueB == networkVersionResultNetMessage.valueB
                        && version.valueC == networkVersionResultNetMessage.valueC)
                        {
                            //success
                            success = true;

                            Debug.Log("VersionResult=success");
                        }
                        else
                        {
                            //faild
                            success = false;
                            endCommandBuffer.AddComponent(entity, new NetworkDisconnectedMessage { error = (short)DisconnectedErrorsInSystem.Version });

                            Debug.LogError($"VersionResult=faild  ClientVersion={version.valueA}.{version.valueB}.{version.valueC}  ServerVersion={networkVersionResultNetMessage.valueA}.{networkVersionResultNetMessage.valueB}.{networkVersionResultNetMessage.valueC}");
                        }

                        _versionResult(success, networkVersionResultNetMessage);
                        _versionResult = null;
                        Enabled = false;
                    }));
            }
        }
    }
}