using RN.Network;
using System.Collections;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class BulletFx : ActorFx<BulletSpawner>
    {
        public override void onCreateFx(BulletSpawner actorSpawner)
        {
        }

        public override void onDestroyFx(BulletSpawner actorSpawner)
        {
            StartCoroutine(playDestroyFx(actorSpawner));
        }

        IEnumerator playDestroyFx(BulletSpawner actorSpawner)
        {
            var trailT = transform.GetChild(BulletSpawner.Trail_TransformIndex);

            var trail = trailT.GetComponent<TrailRenderer>();

            var b = trail.widthMultiplier;
            var e = 0f;
            var csb = trail.startColor;
            var cse = Color.clear;
            var ceb = trail.endColor;
            var cee = Color.clear;
            foreach (var t in new TimeEquation().linear.play(trail.time))
            {
                trail.widthMultiplier = Mathf.Lerp(b, e, t);
                trail.startColor = Color.Lerp(csb, cse, t);
                trail.endColor = Color.Lerp(ceb, cee, t);
                yield return this;
            }

            GameObject.Destroy(gameObject);
        }
    }
}