using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    class ActorLocatorSystem<TActor> : ComponentSystem where TActor : struct, IComponentData
    {
        public float distanceMin = 2f;
        public float distanceScale = 0.005f;
        public int Name_TransformIndex;
        public int MyLocator_TransformIndex;
        public int TeamLocators_TransformIndex;

        protected override void OnUpdate()
        {
            var myPlayerTeamId = 0;
            Transform myActorT = null;
            Transform myTeamLocatorsT = null;

            var myPlayerSingleton = GetSingleton<MyPlayerSingleton>();
            if (myPlayerSingleton.playerEntity != default)
            {
                myPlayerTeamId = EntityManager.GetComponentData<PlayerTeam>(myPlayerSingleton.playerEntity).value;

                var myActorEntity = EntityManager.GetComponentData<PlayerActorArray>(myPlayerSingleton.playerEntity).mainActorEntity;
                if (myActorEntity != default)
                {
                    myActorT = EntityManager.GetComponentObject<Transform>(myActorEntity);
                    myTeamLocatorsT = myActorT.GetChild(TeamLocators_TransformIndex);
                }
            }


            var myTeamPlayerCount = 0;
            var cameraControllerSingleton = GetSingleton<CameraDataSingleton>();
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
                        actorT.GetChild(Name_TransformIndex).gameObject.SetActive(false);
                        return;
                    }

                    var playerName = EntityManager.GetComponentData<PlayerName>(actorOwner.playerEntity).value.ToString();
                    var playerTeamId = EntityManager.GetComponentData<PlayerTeam>(actorOwner.playerEntity).value;

                    if (playerTeamId != 0 && playerTeamId == myPlayerTeamId)//my team
                    {
                        actorT.GetChild(MyLocator_TransformIndex).gameObject.SetActive(true);

                        if (myTeamLocatorsT)
                        {
                            var teamLocatorT = setTeamLocator(myActorT, actorT, myTeamLocatorsT, ref myTeamPlayerCount);

                            teamLocatorT.GetComponentInChildren<TextMesh>().text = playerName;
                        }
                    }
                    else//other team
                    {
                        actorT.GetChild(MyLocator_TransformIndex).gameObject.SetActive(false);
                    }


                    var nameT = actorT.GetChild(Name_TransformIndex);
                    nameT.GetComponentInChildren<TextMesh>().text = playerName;
                    nameT.transform.rotation = cameraControllerSingleton.targetRotation;
                });


            //
            if (myTeamLocatorsT != null)
            {
                for (var i = myTeamPlayerCount; i < myTeamLocatorsT.childCount; ++i)
                {
                    myTeamLocatorsT.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        Transform setTeamLocator(Transform myActorT, Transform actorT, Transform myTeamLocatorsT, ref int myTeamPlayerCount)
        {
            var teamLocatorIndex = myTeamPlayerCount;
            ++myTeamPlayerCount;

            Transform teamLocatorT = null;
            if (myTeamLocatorsT.childCount < myTeamPlayerCount)
            {
                teamLocatorT = GameObject.Instantiate(myTeamLocatorsT.GetChild(0), myTeamLocatorsT);
            }
            else
            {
                teamLocatorT = myTeamLocatorsT.GetChild(teamLocatorIndex);
            }

            teamLocatorT.gameObject.SetActive(true);
            teamLocatorT.forward = actorT.position - myActorT.position;
            teamLocatorT.GetChild(0).localPosition = new Vector3(0f, 0f, distanceMin + distanceScale * Vector3.Magnitude(actorT.position - myActorT.position));

            return teamLocatorT;

        }
    }
}
