
using UnityEngine;

namespace Unity.Entities
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class DestroyMessageSystem : ComponentSystem
    {
        EntityQuery onDestroyMessageQuery;
        protected override void OnCreate()
        {
            onDestroyMessageQuery = GetEntityQuery(new EntityQueryDesc
            {
                Any = new ComponentType[] { ComponentType.ReadOnly<OnDestroyMessage>(), ComponentType.ReadOnly<OnDestroyWithoutMessage>() },
            });
        }
        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(onDestroyMessageQuery);
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class GameObject_OnDestroyMessageSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<GameObject, OnDestroyMessage>()
                .ForEach((GameObject gameObject) =>
                {
                    gameObject.SetActive(false);//Physics里要等下一帧才删除  所以这里SetActive(false)
                    GameObject.Destroy(gameObject);
                });
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class GameObject_OnDestroyWithoutMessageSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<GameObject, OnDestroyWithoutMessage>()
                .ForEach((GameObject gameObject) =>
                {
                    gameObject.SetActive(false);//Physics里要等下一帧才删除  所以这里SetActive(false)
                    GameObject.Destroy(gameObject);
                });
        }
    }
}
