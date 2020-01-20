using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace RN
{
    public class ParticleTriggerInParticleSystem : MonoBehaviour
    {
        public MinMaxCurve multiplier = new MinMaxCurve(1f);
        public List<ParticleTrigger> particleTriggers;

        ParticleSystem ps;
        ParticleSystemRenderer psRenderer;
        private void Awake()
        {
            if (particleTriggers == null)
                particleTriggers = new List<ParticleTrigger>();

            ps = GetComponent<ParticleSystem>();
            Debug.Assert(ps != null, "ps != null", this);
            psRenderer = GetComponent<ParticleSystemRenderer>();
            Debug.Assert(psRenderer != null, "psRenderer != null", this);
        }

        internal void add(ParticleTrigger particleTrigger)
        {
            particleTriggers.Add(particleTrigger);
        }
        internal void remove(ParticleTrigger particleTrigger)
        {
            particleTriggers.Remove(particleTrigger);
        }


        //
        private void OnEnable()
        {
            if (CompareTag("Untagged") == false)
            {
                ParticleTriggerManager.singleton.addParticleTriggerInParticleSystem(this);
            }
        }

        private void OnDisable()
        {
            if (CompareTag("Untagged") == false)
            {
                ParticleTriggerManager.singleton.removeParticleTriggerInParticleSystem(this);
            }
        }

        //
        void Update()
        //void OnParticleUpdateJobScheduled()
        {
            JobHandle inputDeps = default;
            foreach (var particleTrigger in particleTriggers)
            {
                inputDeps = particleTrigger.Schedule(ps, psRenderer, inputDeps);
            }
        }
    }
}
