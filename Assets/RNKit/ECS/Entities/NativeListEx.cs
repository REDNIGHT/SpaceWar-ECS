using Unity.Collections;
using Unity.Jobs;

namespace Unity.Entities
{
    public static class NativeListEx
    {
        public static unsafe void AddRange<T>(in this NativeList<T> list, in DynamicBuffer<T> buffer) where T : struct
        {
            list.AddRange(buffer.GetUnsafePtr(), buffer.Length);
        }
    }
}
