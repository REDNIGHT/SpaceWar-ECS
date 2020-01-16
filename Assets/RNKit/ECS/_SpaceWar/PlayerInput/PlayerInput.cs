
using Unity.Entities;
using Unity.Mathematics;

namespace RN.Network.SpaceWar
{
    [ServerNetworkEntity]
    public struct PlayerShipMoveInputNetMessage : IComponentData
    {
        //public half2 observerPosition;

        public ShipMoveInput shipMoveInput;
    }

    public struct PlayerActorMoveInputSerialize : NetworkSerializeUnsafe.ISerializer//在ActorSpawner.OnActorSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.client2server;
        public short Type => (short)NetworkSerializeType.PlayerActorMoveInput;

        public half2 observerPosition;

        public ShipMoveInput shipMoveInput;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.SetComponent(index, entity, new ObserverPosition { value = new float3(observerPosition.x, 0f, observerPosition.y) });
            commandBuffer.SetComponent(index, entity, new PlayerShipMoveInputNetMessage { shipMoveInput = shipMoveInput });
        }
    }

    public enum FireAction : byte
    {
        none = 0,

        autoFire,
        mainFire,
        //mouseButton2,

        shield,



        __fireSlotIndexBegin = 10,
        fireSlotIndex0 = __fireSlotIndexBegin,
        fireSlotIndex1,
        fireSlotIndex2,
        fireSlotIndex3,
        fireSlotIndex4,
        fireSlotIndex5,
        fireSlotIndex6,
        //fireSlotIndex7,
        //fireSlotIndex8,
        //fireSlotIndex9,
        __fireSlotIndexEnd,


        __uninstallSlotIndexBegin = 30,
        uninstallSlotIndex0 = __uninstallSlotIndexBegin,
        uninstallSlotIndex1,
        uninstallSlotIndex2,
        uninstallSlotIndex3,
        uninstallSlotIndex4,
        uninstallSlotIndex5,
        uninstallSlotIndex6,
        //uninstallSlotIndex7,
        //uninstallSlotIndex8,
        //uninstallSlotIndex9,
        __uninstallSlotIndexEnd,


        __uninstallAssistSlotIndexBegin = 50,
        uninstallAssistSlotIndex0 = __uninstallAssistSlotIndexBegin,
        uninstallAssistSlotIndex1,
        uninstallAssistSlotIndex2,
        //uninstallAssistSlotIndex3,
        //uninstallAssistSlotIndex4,
        //uninstallAssistSlotIndex5,
        //uninstallAssistSlotIndex6,
        //uninstallAssistSlotIndex7,
        //uninstallAssistSlotIndex8,
        //uninstallAssistSlotIndex9,
        __uninstallAssistSlotIndexEnd,
    }

    [ServerNetworkEntity]
    public struct PlayerShipFireInputNetMessage : IComponentData
    {
        public half2 firePosition;
        public FireAction fireAction;
    }

    public struct PlayerActorFireInputSerialize : NetworkSerializeUnsafe.ISerializer//在ActorSpawner.OnActorSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.client2server;
        public short Type => (short)NetworkSerializeType.PlayerActorFireInput;

        public half2 firePosition;
        public FireAction fireAction;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.SetComponent(index, entity, new PlayerShipFireInputNetMessage { firePosition = firePosition, fireAction = fireAction });
        }
    }

    public struct MouseDataSingleton : IComponentData
    {
        public float3 point;
        public float3 direction;
        public float distance;
    }


}
