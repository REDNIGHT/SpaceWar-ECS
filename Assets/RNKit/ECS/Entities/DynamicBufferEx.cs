namespace Unity.Entities
{
    public static class DynamicBufferEx
    {
        public static void ResizeInitialized<T>(this DynamicBuffer<T> buffer, int length) where T : struct
        {
            buffer.ResizeUninitialized(length);

            for (var i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = default;
            }
        }
    }
}