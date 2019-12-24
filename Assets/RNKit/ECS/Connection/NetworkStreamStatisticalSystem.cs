using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class NetworkStreamReceiveStatisticalSystem : JobComponentSystem
    {
        NativeArray<float> total;
        protected override void OnDestroy()
        {
            total.Dispose();
        }

        protected override void OnCreate()
        {
            total = new NativeArray<float>(2, Allocator.Persistent);
            Enabled = false;
        }


        [BurstCompile]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct ReceiveJob : IJobForEach_B<NetworkInBuffer>
        {
            public NativeArray<float> total;
            public void Execute(DynamicBuffer<NetworkInBuffer> inBuffer)
            {
                total[0] += inBuffer.Length;
                total[1] += 1;
            }
        }

        [System.Serializable]
        public class Statistical
        {
            public float totalPreConnectFrame;
            public float totalPreConnectSecond;

            public float totalPreFrame;
            public float totalPreSecond;
        }
        public Statistical statistical { get; protected set; } = new Statistical();
        public float _totalPreSecond;
        float time = 0f;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            _totalPreSecond += total[0];
            time += Time.fixedDeltaTime;
            if (time >= 1f)
            {
                time -= 1f;
                statistical.totalPreFrame = total[0];
                statistical.totalPreSecond = _totalPreSecond;

                if (total[1] > 0f)
                {
                    statistical.totalPreConnectFrame = statistical.totalPreFrame / total[1];
                    statistical.totalPreConnectSecond = statistical.totalPreSecond / total[1];
                }

                _totalPreSecond = 0f;
            }
            total[0] = 0f;
            total[1] = 0f;

            return new ReceiveJob { total = total }.ScheduleSingle(this, inputDeps);
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class NetworkStreamSendStatisticalSystem : JobComponentSystem
    {
        NativeArray<float> total;
        protected override void OnDestroy()
        {
            total.Dispose();
        }

        protected override void OnCreate()
        {
            total = new NativeArray<float>(2, Allocator.Persistent);
            Enabled = false;
        }

        [BurstCompile]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SendJob : IJobForEach_BB<NetworkReliableOutBuffer, NetworkUnreliableOutBuffer>
        {
            public NativeArray<float> total;
            public unsafe void Execute(DynamicBuffer<NetworkReliableOutBuffer> rBuffer, DynamicBuffer<NetworkUnreliableOutBuffer> uBuffer)
            {
                total[0] += rBuffer.Length;
                total[0] += uBuffer.Length;
                total[1] += 1;
            }
        }

        [System.Serializable]
        public class Statistical
        {
            public float totalPreConnectFrame;
            public float totalPreConnectSecond;

            public float totalPreFrame;
            public float totalPreSecond;
        }
        public Statistical statistical { get; protected set; } = new Statistical();
        public float _totalPreSecond;
        float time = 0f;
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            _totalPreSecond += total[0];
            time += Time.fixedDeltaTime;
            if (time >= 1f)
            {
                time -= 1f;
                statistical.totalPreFrame = total[0];
                statistical.totalPreSecond = _totalPreSecond;

                if (total[1] > 0f)
                {
                    statistical.totalPreConnectFrame = statistical.totalPreFrame / total[1];
                    statistical.totalPreConnectSecond = statistical.totalPreSecond / total[1];
                }

                _totalPreSecond = 0f;
            }
            total[0] = 0f;
            total[1] = 0f;

            return new SendJob { total = total }.ScheduleSingle(this, inputDeps);
        }
    }
}

