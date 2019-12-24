using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    [ServerNetworkEntity]
    [ServerAutoClear]
    public struct NetworkPingNetMessage : IComponentData
    {
    }

    public struct NetworkPingSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.client2server;
        public short Type => (short)NetworkSerializeType.NetworkPing;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.AddComponent(index, entity, new NetworkPingNetMessage { });
        }
    }

    [ClientNetworkEntity]
    [ClientAutoClear]
    public struct NetworkPingResultNetMessage : IComponentData
    {
    }

    public struct NetworkPingResultSerialize : NetworkSerializeUnsafe.ISerializer
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.NetworkPingResult;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            commandBuffer.AddComponent(index, entity, new NetworkPingResultNetMessage { });
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class NetworkPingServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct PingS2CJob : IJobForEachWithEntity_EBC<NetworkUnreliableOutBuffer, NetworkPingNetMessage>
        {
            public void Execute(Entity entity, int index,
                DynamicBuffer<NetworkUnreliableOutBuffer> outBuffer, [ReadOnly] ref NetworkPingNetMessage ping)
            {
                var s = new NetworkPingResultSerialize { };
                s._DoSerialize(outBuffer);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new PingS2CJob
            {
            }
            .Schedule(this, inputDeps);
        }
    }


    //----------------------------------------------------------------------------------------------------------------------


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class NetworkPingClientSystem : ComponentSystem
    {
        //
        [System.Serializable]
        public class Ping
        {
            public float LostMaxTime = 4f;

            public float Time;
            public int LostCount;
        }

        public Ping ping = null;

        protected void OnInit(Transform root)
        {
            Enabled = false;
        }



        float sendTime = 0f;
        protected override void OnUpdate()
        {
            //c2s
            if (sendTime == 0f)
            {
                Entities
                    .WithNone<NetworkDisconnectedMessage>()
                    .ForEach((Entity entity, DynamicBuffer<NetworkUnreliableOutBuffer> outBuffer) =>
                    {
                        var s = new NetworkPingSerialize { };
                        s._DoSerialize(outBuffer);
                    });

                sendTime = Time.fixedUnscaledTime;
            }




            //Result
            if (sendTime > 0)
            {
                if (Time.fixedUnscaledTime - sendTime > ping.LostMaxTime)
                {
                    ++ping.LostCount;

                    sendTime = 0f;
                    return;
                }



                //
                Entities
                    .WithAllReadOnly<NetworkConnection, NetworkPingResultNetMessage>()
                    .WithNone<NetworkDisconnectedMessage>()
                    .ForEach((Entity entity, ref NetworkPingResultNetMessage ringResult) =>
                    {
                        var pingTime = Time.fixedUnscaledTime - sendTime;
                        ping.Time = pingTime;

                        sendTime = 0f;
                    });
            }
        }
    }
}