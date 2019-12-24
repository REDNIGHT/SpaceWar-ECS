using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    /*[DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class AccelerateFxOnCreateServerSystem : ComponentSystem
    {
        public ActorTypes accelerateFxType = ActorTypes.AccelerateFx;
        ActorCreateServerSystem actorCreateSystem;
        EndFixedCommandBufferSystem endBarrier;

        protected void OnInit(Transform root)
        {
            actorCreateSystem = World.GetExistingSystem<ActorCreateServerSystem>();
            if (actorCreateSystem == null) Debug.LogError("actorCreateSystem == null");

            endBarrier = World.GetExistingSystem<EndFixedCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {
            //prepare
            Entities
                .WithAllReadOnly<AccelerateFxMessage>()
                .ForEach((ref AccelerateFxMessage accelerateFxMessage) =>
                {
                    var accelerateFxEntity = actorCreateSystem.CreateInServer((short)accelerateFxType, ActorOwner.Null);

                    EntityManager.SetComponentData(accelerateFxEntity, new ActorCreator { entity = accelerateFxMessage.shipEntity });
                    EntityManager.SetComponentData(accelerateFxEntity, new AccelerateFx { velocityLevel = accelerateFxMessage.velocityLevel });

                    EntityManager.SetComponentData(accelerateFxEntity, accelerateFxMessage.translation);
                });
        }
    }*/

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class AccelerateFxOnCreateClientSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
        }
        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<AccelerateFx, ActorCreator, OnCreateMessage>()
                .ForEach((ref ActorCreator shipCreator, ref AccelerateFx accelerateFx) =>
                {
                    var shipT = EntityManager.GetComponentObject<Transform>(shipCreator.entity);

                    var accelerateFxT = shipT.GetChild(ShipSpawner.AccelerateFx_TransformIndex);

                    var fx = accelerateFxT.GetComponent<IAccelerateFx>();
                    if (fx != null)
                    {
                        fx.OnPlayFx();
                    }
                    else
                    {
                        accelerateFxT.gameObject.SetActive(true);
                    }
                });
        }
    }
}
