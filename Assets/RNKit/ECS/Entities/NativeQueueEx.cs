using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Unity.Entities
{
    public static class NativeQueueEx
    {
        public static NativeArray<T> ToArray<T>(in this NativeQueue<T> queue, Allocator allocator) where T : struct
        {
            queue.ToArray(out NativeArray<T> array, allocator);
            return array;
        }

        public static void ToArray<T>(in this NativeQueue<T> queue, out NativeArray<T> array, Allocator allocator) where T : struct
        {
            array = new NativeArray<T>(queue.Count, allocator);

            queue.ToArray(ref array);
        }

        public static void ToArray<T>(in this NativeQueue<T> queue, ref NativeArray<T> array) where T : struct
        {
            var index = 0;
            while (queue.Count > 0)
            {
                array[index] = queue.Dequeue();
                ++index;
            }
        }

        [BurstCompile]
        struct Queue2ListJob<T> : IJob where T : struct
        {
            public NativeQueue<T> queue;
            public NativeList<T> list;
            public void Execute()
            {
                while (queue.Count > 0)
                {
                    list.Add(queue.Dequeue());
                }
            }
        }

        public static JobHandle ToListJob<T>(in this NativeQueue<T> queue, ref NativeList<T> list, JobHandle inputDeps) where T : struct
        {
            inputDeps = new Queue2ListJob<T>
            {
                queue = queue,
                list = list,
            }
            .Schedule(inputDeps);

            return inputDeps;
        }



        [BurstCompile]
        struct Queues2ArrayJob<T> : IJob where T : struct
        {
            public NativeQueue<T> queueA;
            public NativeQueue<T> queueB;
            public NativeList<T> list;
            public void Execute()
            {
                while (queueA.Count > 0)
                {
                    list.Add(queueA.Dequeue());
                }
                while (queueB.Count > 0)
                {
                    list.Add(queueB.Dequeue());
                }
            }
        }

        public static JobHandle ToListJob<T>(in NativeQueue<T> queueA, in NativeQueue<T> queueB, ref NativeList<T> list, JobHandle inputDeps) where T : struct
        {
            inputDeps = new Queues2ArrayJob<T>
            {
                queueA = queueA,
                queueB = queueB,
                list = list,
            }
            .Schedule(inputDeps);

            return inputDeps;
        }


        public static void EnqueueRange<T>(in this NativeQueue<T>.ParallelWriter queue, in NativeList<T> list) where T : struct
        {
            for (int i = 0; i < list.Length; ++i)
            {
                queue.Enqueue(list[i]);
            }
        }
        public static void EnqueueRange<T>(in this NativeQueue<T>.ParallelWriter queue, in DynamicBuffer<T> buffer) where T : struct
        {
            for (int i = 0; i < buffer.Length; ++i)
            {
                queue.Enqueue(buffer[i]);
            }
        }
    }
}
