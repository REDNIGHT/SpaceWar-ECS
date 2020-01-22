using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class ExplosionFx : ActorFx<ExplosionSpawner>
    {
        public override void onCreateFx(ExplosionSpawner actorSpawner)
        {
            Debug.Assert(actorSpawner.isClient == true, "isClient == true", this);

            foreach (var ps in GetComponentsInChildren<ParticleSystem>())
            {
                if (ps.name == "_")
                    continue;

                var shape = ps.shape;
                shape.radius = actorSpawner.radius;
            }

            var explosionParticleTrigger = GetComponentInChildren<ExplosionParticleTrigger>();
            if (explosionParticleTrigger != null)
                explosionParticleTrigger.radius = 5f + actorSpawner.radius;
        }

        public override void onDestroyFx(ExplosionSpawner actorSpawner)
        {
            Debug.Assert(actorSpawner.isClient == true, "isClient == true", this);
        }
    }
}