using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class ActorLocator : MonoBehaviour
    {
        //static readonly Quaternion offset = Quaternion.Euler(90f, 0f, 0f);

        int Name_TransformIndex = 0;
        int MemberMask_TransformIndex = 1;
        int MyMemberArrows_TransformIndex = 2;

        private void Awake()
        {
            clearMyMemberArrows(0);
        }

        public void setMyself()
        {
            transform.GetChild(Name_TransformIndex).gameObject.SetActive(false);

            transform.GetChild(MemberMask_TransformIndex).gameObject.SetActive(false);
            //transform.GetChild(MemberMask_TransformIndex).rotation = cameraData.targetRotation;
        }
        public void setMyMemberArrows(string playerName, Transform actorT
            , ref int myTeamPlayerCount
            , float distanceMin, float distanceScale)
        {
            var myMemberArrowsT = transform.GetChild(MyMemberArrows_TransformIndex);


            var teamLocatorIndex = myTeamPlayerCount;
            ++myTeamPlayerCount;

            Transform memberArrowT = null;
            if (myMemberArrowsT.childCount < (teamLocatorIndex + 1))
            {
                memberArrowT = GameObject.Instantiate(myMemberArrowsT.GetChild(0), myMemberArrowsT);
            }
            else
            {
                memberArrowT = myMemberArrowsT.GetChild(teamLocatorIndex);
            }

            memberArrowT.gameObject.SetActive(true);
            memberArrowT.forward = actorT.position - transform.position;
            memberArrowT.GetChild(0).localPosition = new Vector3(0f, 0f, distanceMin + distanceScale * Vector3.Magnitude(actorT.position - transform.position));


            memberArrowT.GetComponentInChildren<TextMesh>().text = playerName;
        }
        public void clearMyMemberArrows(int memberCount)
        {
            var myMemberArrowsT = transform.GetChild(MyMemberArrows_TransformIndex);

            for (var i = memberCount; i < myMemberArrowsT.childCount; ++i)
            {
                myMemberArrowsT.GetChild(i).gameObject.SetActive(false);
            }
        }



        public void setName(string playerName)
        {
            var nameT = transform.GetChild(Name_TransformIndex);
            nameT.GetComponentInChildren<TextMesh>().text = playerName;

            //nameT.transform.rotation = cameraData.targetRotation;
        }
        public void setMemberMask(bool v)
        {
            transform.GetChild(MemberMask_TransformIndex).gameObject.SetActive(v);

            //if (v)
            //    transform.GetChild(MemberMask_TransformIndex).rotation = cameraData.targetRotation;
        }
    }
}
