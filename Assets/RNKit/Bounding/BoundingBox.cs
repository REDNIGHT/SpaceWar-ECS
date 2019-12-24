using UnityEngine;
using System.Collections;
namespace RN
{
    [System.Obsolete("Please use BoundingSize instead")]
    public class BoundingBox : Bounding
    {
        public Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 0.5f);

        public override Vector3 center { get { return transform.position + _bounds.center; } }

        public override Vector3 randomLocalPosition { get { return transform.rotation * _bounds.getRandomPoint(); } }


#if UNITY_EDITOR
        [System.NonSerialized]
        public Color lineColor = Color.yellow;
        void OnDrawGizmosSelected()
        {
            Gizmos.color = lineColor;
            var m = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, _bounds.size);
            Gizmos.matrix = m;
        }
#endif
    }
}