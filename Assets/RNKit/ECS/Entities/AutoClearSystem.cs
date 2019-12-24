using System;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Unity.Entities
{
    //在每帧结束时都会清空数据
    //如果是IComponentData是移除
    //如果是IBufferElementData是清空
    public class AutoClearAttribute : Attribute { }

    [System.Obsolete("//todo...")]
    public class AutoDeleteEntityAttribute : Attribute { }

    public class ServerAutoClearAttribute : Attribute { }
    public class ClientAutoClearAttribute : Attribute { }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class AutoClearSystem<TAutoClearAttribute> : JobComponentSystem where TAutoClearAttribute : Attribute
    {
        (EntityQuery query, ComponentType component)[] componentRemoveEntityQuerys;

        (EntityQuery query, System.Func<EntityQuery, JobHandle, JobHandle> clearBuffer)[] bufferClearEntityQuerys;
        NativeArray<JobHandle> jobHandles;

        public static JobHandle ClearBuffer<T>(EntityQuery query, JobHandle dependsOn) where T : struct, IBufferElementData
        {
            return new ClearBufferJob<T>().ScheduleSingle(query, dependsOn);
        }

        //[BurstCompile]
        struct ClearBufferJob<T> : IJobForEach_B<T> where T : struct, IBufferElementData
        {
            public void Execute(DynamicBuffer<T> b0)
            {
                b0.Clear();
            }
        }

        protected override void OnCreate()
        {
            componentRemoveEntityQuerys = TypeManager.AllTypes
                .Where(x => typeof(IComponentData).IsAssignableFrom(TypeManager.GetType(x.TypeIndex)))
                .Where(x => Attribute.IsDefined(TypeManager.GetType(x.TypeIndex), typeof(TAutoClearAttribute)) || Attribute.IsDefined(TypeManager.GetType(x.TypeIndex), typeof(AutoClearAttribute)))
                .Select(x => (GetEntityQuery(new EntityQueryDesc { All = new ComponentType[] { ComponentType.ReadOnly(x.TypeIndex) }, }), ComponentType.ReadOnly(x.TypeIndex)))
                .ToArray();


            var ClearBuffer = GetType().GetMethod("ClearBuffer");
            bufferClearEntityQuerys = TypeManager.AllTypes
                .Where(x => typeof(IBufferElementData).IsAssignableFrom(TypeManager.GetType(x.TypeIndex)))
                .Where(x => Attribute.IsDefined(TypeManager.GetType(x.TypeIndex), typeof(TAutoClearAttribute)) || Attribute.IsDefined(TypeManager.GetType(x.TypeIndex), typeof(AutoClearAttribute)))
                .Select(x =>
                {
                    var query = GetEntityQuery(new EntityQueryDesc { All = new ComponentType[] { ComponentType.ReadOnly(x.TypeIndex) } });
                    query.SetFilterChanged(ComponentType.ReadOnly(x.TypeIndex));

                    var type = TypeManager.GetType(x.TypeIndex);
                    var _clearBuffer = ClearBuffer.MakeGenericMethod(type);

                    return (query, Delegate.CreateDelegate(typeof(System.Func<EntityQuery, JobHandle, JobHandle>), _clearBuffer) as System.Func<EntityQuery, JobHandle, JobHandle>);
                })
                .ToArray();

            jobHandles = new NativeArray<JobHandle>(bufferClearEntityQuerys.Length, Allocator.Persistent);
        }
        protected override void OnDestroy()
        {
            jobHandles.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //
            foreach ((var query, var component) in componentRemoveEntityQuerys)
            {
                if (query.IsEmptyIgnoreFilter)
                    continue;

                EntityManager.RemoveComponent(query, component);
            }

            //
            for (var i = 0; i < bufferClearEntityQuerys.Length; ++i)
            {
                var query = bufferClearEntityQuerys[i].query;
                jobHandles[i] = bufferClearEntityQuerys[i].clearBuffer(query, inputDeps);
            }

            return JobHandle.CombineDependencies(jobHandles);
        }
    }
}
