
using Unity.Entities;
using UnityEngine;

namespace RN.Network
{
    /*[DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorDestroyServerSystem : ComponentSystem
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
            //
            Entities
                .WithAllReadOnly<Actor, OnDestroyMessage>()
                //.WithNone<>()
                .ForEach((Entity actorEntity, ref Actor actor) =>
                {
                    actorSpawnerMap.DestroyInServer(actorEntity, actor.actorType);
                });
        }
    }*/

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorDestroyClientSystem : ComponentSystem
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
            //
            Entities
                .WithAllReadOnly<Actor>()
                .WithAnyReadOnly<OnDestroyMessage, OnDestroyWithoutMessage>()
                //.WithNone<>()
                .ForEach((Entity actorEntity, ref Actor actor) =>
                {
                    actorSpawnerMap.DestroyInClient(actorEntity, actor.actorType);
                });
        }
    }

}
