using Unity.Collections;

namespace Unity.Entities
{
    public static class EntityQueryEx
    {
        public static NativeList<T> AllBufferElementToList<T>(in this EntityQueryBuilder entityQueryBuilder, Allocator allocator)
            where T : struct, IBufferElementData
        {
            var list = new NativeList<T>(allocator);

            entityQueryBuilder.ForEach((DynamicBuffer<T> buffer) =>
            {
                list.AddRange(buffer);
            });

            return list;
        }

        public static NativeList<T> AllBufferElementToList<T>(this EntityQuery entityQuery, in EntityManager entityManager, Allocator allocator)
            where T : struct, IBufferElementData
        {
            var list = new NativeList<T>(allocator);

            using (var entities = entityQuery.ToEntityArray(Allocator.TempJob))
            {
                for (var i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    //if (!entityManager.Exists(entity)) continue;
                    var c0 = entityManager.GetBuffer<T>(entity);
                    list.AddRange(c0);
                }
            }
            return list;
        }

        //慎用  DynamicBuffer会在对Entity的创建删除时失效
        public static NativeArray<DynamicBuffer<T>> ToBufferArray<T>(this EntityQuery entityQuery, in EntityManager entityManager, Allocator allocator)
            where T : struct, IBufferElementData
        {
            using (var entities = entityQuery.ToEntityArray(Allocator.TempJob))
            {
                var array = new NativeArray<DynamicBuffer<T>>(entities.Length, Allocator.Temp);

                for (var i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    //if (!entityManager.Exists(entity)) continue;
                    var c0 = entityManager.GetBuffer<T>(entity);
                    array[i] = c0;
                }

                return array;
            }
        }
    }
}
