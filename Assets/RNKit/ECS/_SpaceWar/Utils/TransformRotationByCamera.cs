using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class TransformRotationByCamera : MonoBehaviour
    {
        [System.Serializable]
        public struct Data
        {
            public bool isUI;
            public Transform transform;
        }
        public Data[] datas;
    }
}
