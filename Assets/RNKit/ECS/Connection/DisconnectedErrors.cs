
namespace RN.Network
{
    public enum DisconnectedErrors : short
    {
        Disconnect = 0,

        Accept,
        
        Receive_InBufferLength,

        Serialize_Type_Size,
        Serialize_Exception,

        MaxCount = 100,
    }
}