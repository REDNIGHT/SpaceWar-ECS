using UnityEngine;
using System.Collections;
namespace RN
{
    public class BoundingRadius : Bounding
    {
        public float radius = 0.5f;

        public override Vector3 randomLocalPosition { get { return Random.insideUnitSphere * radius; } }


        public Vector3 randomOnUnitSphereLocalPosition { get { return Random.onUnitSphere * radius; } }
        public Vector3 randomOnUnitSpherePosition { get { return transform.position + randomOnUnitSphereLocalPosition; } }

#if UNITY_EDITOR
        float gizmosShowTime = 0f;
        float gizmosScaleAnimation = 0f;
        public const float Unselect_Color_a = 0.25f;
        public void playGizmos(float showTime = 0.5f)
        {
            gizmosShowTime = showTime;
            gizmosScaleAnimation = 0.25f;
        }
        public void stopGizmos()
        {
            gizmosShowTime = 0f;
        }
        void OnDrawGizmosSelected()
        {
            if (gizmosShowTime <= 0f)
            {
                var c = Color.yellow;
                if (UnityEditor.Selection.activeTransform != transform)
                    c.a = Unselect_Color_a;

                Gizmos.color = c;
                Gizmos.DrawWireSphere(center, radius);
            }
        }
        void OnDrawGizmos()
        {
            if (gizmosShowTime > 0f)
            {
                gizmosShowTime -= Time.deltaTime;
                gizmosScaleAnimation -= Time.deltaTime;
                if (gizmosScaleAnimation < 0f)
                    gizmosScaleAnimation = 0f;

                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(center, radius + gizmosScaleAnimation);
            }
        }
#endif

    }
}