using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network
{
    public interface IEntityBuilder
    {
        void Init(EntityManager entityManager);
        void Build(IActorSpawnerMap actorSpawnerMap, EntityManager entityManager);
    }

    public abstract class EntityBuilder : MonoBehaviour, IEntityBuilder
    {
        //
        public float delay = 1f;
        public float interval = 0.5f;


        //
        protected abstract short actorType { get; }
        protected virtual (float3 pos, quaternion rot) position_rotation => (transform.position, transform.rotation);


        //
        public void Init(EntityManager entityManager)
        {
            var entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new CallTrigger(delay, interval, -1, typeof(OnCallMessage)));
            entityManager.AddComponentObject(entity, this);

#if UNITY_EDITOR
            entityManager.SetName(entity, name + ":" + entity.Index);
#endif
        }

        protected abstract Transform getPoint();

        public void Build(IActorSpawnerMap actorSpawnerMap, EntityManager entityManager)
        {
            var point = getPoint();
            if (point == null)
                return;

            var actorEntity = actorSpawnerMap.CreateInServer(actorType, ActorOwner.Null);

            entityManager.SetComponentData(actorEntity, new Translation { Value = point.position });
            entityManager.SetComponentData(actorEntity, new Rotation { Value = point.rotation });

            var actorT = entityManager.GetComponentObject<GameObject>(actorEntity).transform;
            actorT.parent = point;

            Debug.Assert(actorT.localScale == Vector3.one, $"actorT.localScale == Vector3.one  actorT.localScale={actorT.localScale}", actorT);
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class EntityBuilderServerSystem<TEntityBuilder> : ComponentSystem
        where TEntityBuilder : class, IEntityBuilder
    {
        IActorSpawnerMap actorSpawnerMap;

        protected void OnInit(Transform root)
        {
            //
            actorSpawnerMap = root.GetComponentInChildren<IActorSpawnerMap>();
            Debug.Assert(actorSpawnerMap != null, $"actorSpawnerMap != null  root={root}", root);


            //
            var actorBuilders = root.GetComponentsInChildren<IEntityBuilder>();
            foreach (var ab in actorBuilders)
            {
                ab.Init(EntityManager);
            }
        }
        protected override void OnUpdate()
        {
            Entities
                .WithAll<TEntityBuilder>().WithAllReadOnly<OnCallMessage>()
                .ForEach((TEntityBuilder entityBuilder) =>
                {
                    entityBuilder.Build(actorSpawnerMap, EntityManager);
                });
        }
    }
}