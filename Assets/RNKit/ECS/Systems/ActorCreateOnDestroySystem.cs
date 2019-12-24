using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network
{
    public struct ActorCreateOnDestroy : IComponentData
    {
        public short actorType;
        public float3 offset;

        //todo... 
        //public float count;
    }

    /// <summary>
    /// 根据子节点的数量 还有坐标方向 来创建actor
    /// </summary>
    [System.Obsolete("//todo...")]
    public struct ActorsCreateOnDestroy : IComponentData
    {
        public short actorType;
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorCreateOnDestroyServerSystem : ComponentSystem
    {
        IActorSpawnerMap actorSpawnerMap;

        protected void OnInit(Transform root)
        {
            actorSpawnerMap = root.GetComponentInChildren<IActorSpawnerMap>();
            Debug.Assert(actorSpawnerMap != null, $"actorSpawnerMap != null  root={root}", root);
        }
        protected override void OnDestroy()
        {
            actorSpawnerMap = null;
        }

        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<ActorOwner, Translation, Rotation, ActorCreateOnDestroy, OnDestroyMessage>()
                .ForEach((ref ActorOwner actorOwner, ref Translation translation, ref Rotation rotation, ref ActorCreateOnDestroy createOnDestroy) =>
                {
                    var pos = translation.Value;
                    if (createOnDestroy.offset.Equals(default) == false)
                        pos = math.mul(rotation.Value, createOnDestroy.offset) + translation.Value;

                    var actorEntity = actorSpawnerMap.CreateInServer(createOnDestroy.actorType, actorOwner);

                    EntityManager.SetComponentData(actorEntity, new Translation { Value = pos });

                    if (EntityManager.HasComponent<Rotation>(actorEntity))
                    {
                        EntityManager.SetComponentData(actorEntity, rotation);
                    }
                });
        }
    }
}