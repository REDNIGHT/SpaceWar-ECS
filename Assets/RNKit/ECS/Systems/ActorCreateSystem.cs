using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network
{
#if false
    public struct ActorSpawnerData : IComponentData
    {
        public short actorType;
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ActorCreateServerSystem : ComponentSystem
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
        }

        EndCommandBufferSystem endBarrier;
        protected void OnInit(Transform root)
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();

            entityArchetype = EntityManager.CreateArchetype(typeof(ActorSpawnerData), typeof(ActorOwner), typeof(Translation), typeof(Rotation));
        }

        public Entity CreateInServer(short actorType, in ActorOwner actorOwner)
        {
            return actorSpawnerMap.CreateInServer(actorType, actorOwner);
        }


        EntityArchetype entityArchetype;
        public void CreateInServer(short actorType, in ActorOwner actorOwner, in Translation translation, in Rotation rotation)
        {
            var entity = EntityManager.CreateEntity(entityArchetype);

            EntityManager.SetComponentData(entity, new ActorSpawnerData { actorType = actorType });
            EntityManager.SetComponentData(entity, actorOwner);
            EntityManager.SetComponentData(entity, translation);
            EntityManager.SetComponentData(entity, rotation);
        }

        /*List<(ActorOwner actorOwner, short actorType, System.Action<Entity>)> deferredList = new List<(ActorOwner actorOwner, short actorType, System.Action<Entity>)>();
        public void CreateInServer(short actorType, in ActorOwner actorOwner, System.Action<Entity> deferredAction)
        {
            deferredList.Add((actorOwner, actorType, deferredAction));
        }*/

        protected override void OnUpdate()
        {
            var endCommandBuffer = endBarrier.CreateCommandBuffer();
            //
            Entities
                .WithAllReadOnly<ActorSpawnerData, ActorOwner, Translation, Rotation>()
                .ForEach((Entity entity, ref ActorSpawnerData actorSpawnerData, ref ActorOwner actorOwner, ref Translation translation, ref Rotation rotation) =>
                {
                    var actorEntity = actorSpawnerMap.CreateInServer(actorSpawnerData.actorType, actorOwner);

                    EntityManager.SetComponentData(actorEntity, translation);

                    if (EntityManager.HasComponent<Rotation>(actorEntity))
                    {
                        EntityManager.SetComponentData(actorEntity, rotation);
                    }

                    endCommandBuffer.DestroyEntity(entity);
                });



            //
            /*foreach ((var actorOwner, var actorType, var deferredAction) in deferredList)
            {
                var actorEntity = actorSpawners.CreateInServer(actorType, actorOwner, endCommandBuffer);

                deferredAction(actorEntity);
            }

            deferredList.Clear();*/
        }
    }
#endif
}
