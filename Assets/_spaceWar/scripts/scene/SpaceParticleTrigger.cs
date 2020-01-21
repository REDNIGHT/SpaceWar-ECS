using Unity.Jobs;
using UnityEngine;
using System.Collections;
using static UnityEngine.ParticleSystem;

namespace RN.Network.SpaceWar.Fx
{
    [RequireComponent(typeof(ParticleSystem), typeof(ParticleTriggerInParticleSystem))]
    public class SpaceParticleTrigger : ParticleTrigger
    {
        public float remainingLifetime = 1f;
        private IEnumerator Start()
        {
            yield return this;
            var shape = GetComponent<ParticleSystem>().shape;
            shape.radiusThickness = 0f;
        }
        protected override void onSchedule(ref Particle particles, in MinMaxCurve multiplier)
        {
            if (particles.remainingLifetime > remainingLifetime)
            {
                particles.remainingLifetime = remainingLifetime;
            }
        }
    }
}