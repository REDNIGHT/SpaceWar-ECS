using Unity.Jobs;
using UnityEngine;
using System.Collections;
using static UnityEngine.ParticleSystem;

namespace RN.Network.SpaceWar.Fx
{
    [RequireComponent(typeof(ParticleSystem), typeof(ParticleTriggerInParticleSystem))]
    public class SpaceParticleTrigger : BaseParticleTrigger
    {
        public float remainingLifetime = 0f;
        public bool emitAll = true;

        private IEnumerator Start()
        {
            if (Application.isPlaying)
            {
                var ps = GetComponent<ParticleSystem>();
                if (emitAll)
                    ps.Emit(ps.main.maxParticles);

                yield return this;

                var shape = ps.shape;
                shape.radiusThickness = 0f;
            }
        }

        protected override void onSchedule(ref Particle particle)
        {
            if (particle.remainingLifetime > remainingLifetime)
            {
                particle.remainingLifetime = remainingLifetime;
            }
        }
    }
}