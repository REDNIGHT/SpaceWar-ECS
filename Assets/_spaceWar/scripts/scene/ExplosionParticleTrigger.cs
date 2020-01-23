using Unity.Jobs;
using UnityEngine;
using System.Collections;
using static UnityEngine.ParticleSystem;

namespace RN.Network.SpaceWar.Fx
{
    public class ExplosionParticleTrigger : BaseParticleTrigger
    {
        public float remainingLifetime = 0f;

        public bool executeOnes = true;
        public float forceMin = 5f;
        public float forceMax = 10f;
        //public float torqueMin = 5f;
        //public float torqueMax = 10f;

        private void LateUpdate()
        {
            if (Application.isPlaying && executeOnes)
                enabled = false;
        }

        protected override void onSchedule(ref Particle particle)
        {
            particle.remainingLifetime = remainingLifetime;

            var direction = particle.position - transform.position;
            var distance = direction.magnitude;
            direction.Normalize();

            var t = 1f - distance / radius;
            particle.velocity += direction * Mathf.LerpUnclamped(forceMin, forceMax, t);
            //particle.angularVelocity += Mathf.LerpUnclamped(torqueMin, torqueMax, t) * multiplier;
            //particle.angularVelocity3D *= Mathf.LerpUnclamped(torqueMin, torqueMax, t) * multiplier;

            //Debug.Log("angularVelocity3D=" + particle.angularVelocity3D + "  angularVelocity=" + particle.angularVelocity);
        }
    }
}