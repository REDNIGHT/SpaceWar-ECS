using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class ShieldRoot : MonoBehaviour
    {
        public Quaternion rotation;
        private void Update()
        {
            Debug.Assert(rotation != default, $"rotation != default  检测ShieldRoot.eanble是否false", this);
            transform.rotation = rotation;
        }
    }
}
