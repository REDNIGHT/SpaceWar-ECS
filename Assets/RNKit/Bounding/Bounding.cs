using UnityEngine;
using System.Collections;

namespace RN
{
    public abstract class Bounding : MonoBehaviour
    {
        public virtual Vector3 center { get { return transform.position; } }

        public abstract Vector3 randomLocalPosition { get; }

        public Vector3 randomPosition { get { return center + randomLocalPosition; } }
    }
}