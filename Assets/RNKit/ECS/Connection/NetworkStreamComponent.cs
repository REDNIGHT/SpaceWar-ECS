using Unity.Entities;


namespace RN.Network
{
    //
    public struct NetworkConnection : IComponentData
    {
        public Unity.Networking.Transport.NetworkConnection value;
    }


    //
    [ClientAutoClear]
    public struct NetworkConnectMessage : IComponentData { }
    [AutoClear]
    public struct NetworkConnectedMessage : IComponentData { }
    [AutoClear]
    public struct NetworkDisconnectedMessage : IComponentData
    {
        public short error;
    }


    //
    public struct NetworkInBuffer : IBufferElementData
    {
        public byte value;
    }

    //ReliablePipeline
    public struct NetworkReliableOutBuffer : IBufferElementData
    {
        public byte value;
    }

    //UnreliablePipeline
    public struct NetworkUnreliableOutBuffer : IBufferElementData
    {
        public byte value;
    }
}