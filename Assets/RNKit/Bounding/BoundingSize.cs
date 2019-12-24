using UnityEngine;
using System.Collections;
namespace RN
{
    public class BoundingSize : Bounding
    {
        public Vector3 size = Vector3.one;

        //
        public override Vector3 randomLocalPosition { get { return transform.rotation * size.getRandomPoint(); } }



#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            var c = Color.yellow;
            if (UnityEditor.Selection.activeTransform != transform)
                c.a = BoundingRadius.Unselect_Color_a;

            Gizmos.color = c;
            var m = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = m;
        }
#endif

    }
}