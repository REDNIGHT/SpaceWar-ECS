using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    class ActorLocatorSystem<TActor> : ComponentSystem where TActor : struct, IComponentData
    {
        public float distanceMin = 2.5f;
        public float distanceScale = 0.01f;
        public int Locator_TransformIndex;

        protected override void OnUpdate()
        {
            var myPlayerTeamId = 0;
            Transform myActorT = null;
            ActorLocator myActorLocator = null;

            var myPlayerSingleton = GetSingleton<MyPlayerSingleton>();
            if (myPlayerSingleton.playerEntity != default)
            {
                myPlayerTeamId = EntityManager.GetComponentData<PlayerTeam>(myPlayerSingleton.playerEntity).value;

                var myActorEntity = EntityManager.GetComponentData<PlayerActorArray>(myPlayerSingleton.playerEntity).mainActorEntity;
                if (myActorEntity != default)
                {
                    myActorT = EntityManager.GetComponentObject<Transform>(myActorEntity);
                    myActorLocator = myActorT.GetChild(Locator_TransformIndex).GetComponent<ActorLocator>();
                }
            }


            var memberCount = 0;
            var cameraData = GetSingleton<CameraDataSingleton>();
            //
            Entities
                .WithAllReadOnly<TActor, ActorOwner, Transform>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Transform actorT, ref ActorOwner actorOwner) =>
                {
                    if (EntityManager.HasComponent<PlayerTeam>(actorOwner.playerEntity) == false)//断线了
                        return;

                    if (actorT == myActorT)//myself
                    {
                        myActorLocator.setMyself(cameraData);
                        return;
                    }

                    var actorLocator = actorT.GetChild(Locator_TransformIndex).GetComponent<ActorLocator>();

                    var playerName = EntityManager.GetComponentData<PlayerName>(actorOwner.playerEntity).value.ToString();
                    var playerTeamId = EntityManager.GetComponentData<PlayerTeam>(actorOwner.playerEntity).value;

                    if (playerTeamId != 0 && playerTeamId == myPlayerTeamId)//my team
                    {
                        actorLocator.setMemberMask(true, cameraData);

                        if (myActorLocator != null)
                        {
                            myActorLocator.setMyMemberArrows(playerName, actorT, ref memberCount, distanceMin, distanceScale);
                        }
                    }
                    else//other team
                    {
                        actorLocator.setMemberMask(false, cameraData);
                    }


                    actorLocator.setName(playerName, cameraData);
                });


            //
            if (myActorLocator != null)
            {
                myActorLocator.clearMyMemberArrows(memberCount);
            }
        }
    }
}
