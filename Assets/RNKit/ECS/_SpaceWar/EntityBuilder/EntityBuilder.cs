using System.Collections.Generic;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class EntityBuilder : Network.EntityBuilder
    {
        [System.Serializable]
        public struct ActorTypeInfo
        {
            public ActorTypes actorType;
            public int weight;
        }
        public enum SelectType
        {
            Random,
            Next,
        }
        public SelectType selectType = SelectType.Random;

        public ActorTypeInfo[] actorTypes;
        int nextIndex = 0;
        protected override short actorType
        {
            get
            {
                if (selectType == SelectType.Next)
                {
                    if (nextIndex >= actorTypes.Length)
                        nextIndex = 0;
                    return (short)actorTypes[nextIndex++].actorType;
                }
                else
                {
                    return (short)RandomEx.weightedChoice(actorTypes, (x) => x.weight).actorType;
                }
            }
        }

        void OnValidate()
        {
            for (var i = 0; i < actorTypes.Length; ++i)
            {
                var actorType = actorTypes[i].actorType;

                if (actorType > ActorTypes.__Weapon_Begin__ && actorType < ActorTypes.__Weapon_End__)
                    continue;
                if (actorType > ActorTypes.__AttributeTrigger_Begin__ && actorType < ActorTypes.__AttributeTrigger_End__)
                    continue;
                if (actorType > ActorTypes.__PhysicsTrigger_Begin__ && actorType < ActorTypes.__PhysicsTrigger_End__)
                    continue;
                if (actorType > ActorTypes.__SceneObject_Begin__ && actorType < ActorTypes.__SceneObject_End__)
                    continue;
                if (actorType > ActorTypes.__Battery_Begin__ && actorType < ActorTypes.__Battery_End__)
                    continue;

                Debug.LogError($"actorTypes[{i}].actorType != {actorTypes[i].actorType}", this);

                actorTypes[i].actorType = ActorTypes.None;
            }
        }

        public Transform points;
        private void Awake()
        {
            if (points == null)
                points = transform;
        }

        public List<Transform> pointList = new List<Transform>();
        protected override Transform getPoint()
        {
            pointList.Clear();
            foreach (Transform c in points)
            {
                if (c.childCount > 0)
                    continue;
                if (c.gameObject.activeSelf == false)
                    continue;

                pointList.Add(c);
            }

            if (pointList.Count == 0)
                return null;

            if (pointList.Count == 1)
                return pointList[0];

            return RandomEx.weightedChoice(pointList, (x) => 1);
        }
    }
}