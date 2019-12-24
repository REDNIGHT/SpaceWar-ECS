using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RN.Network
{
    public struct PlayerObserverPositionSerialize : NetworkSerializeUnsafe.ISerializer//在ActorSpawner.OnActorSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.client2server;
        public short Type => (short)NetworkSerializeType.PlayerObserverPosition;

        public half2 observerPosition;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.SetComponent(index, entity, new ObserverPosition { value = new float3(observerPosition.x, 0f, observerPosition.x) });
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerInputServerSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return inputDeps;
        }
    }

    public struct ObserverSingleton : IComponentData
    {
        public half2 position;
        public half2 lastPosition;
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerInputClientSystem : ComponentSystem
    {
        protected void OnInit(Transform root)
        {
            World.GetExistingSystem<NetworkStreamConnectSuccessSystem>().AddSystem(this);

            var singletonEntity = GetSingletonEntity<MyPlayerSingleton>();
            EntityManager.AddComponentData(singletonEntity, new ObserverSingleton { position = half2.zero, lastPosition = half2.zero });
        }
        protected override void OnUpdate()
        {
            var observerSingleton = GetSingleton<ObserverSingleton>();
            //
            if (observerSingleton.position.Equals(observerSingleton.lastPosition) == false)
            {
                observerSingleton.lastPosition = observerSingleton.position;
                SetSingleton(observerSingleton);

                //
                Entities
                    .WithAllReadOnly<NetworkConnection>()
                    .WithNone<NetworkDisconnectedMessage>()
                    .ForEach((DynamicBuffer<NetworkUnreliableOutBuffer> outBuffer) =>
                    {
                        var s = new PlayerObserverPositionSerialize { observerPosition = observerSingleton.position };
                        s._DoSerialize(outBuffer);
                    });
            }

        }
    }
}