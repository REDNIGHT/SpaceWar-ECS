using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace RN
{
    [ExecuteInEditMode]
    public class ParticleTriggerInParticleSystem : MonoBehaviour
    {
        public float multiplier = 1f;
        public float size = 0f;
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

        //
        void Update()
        //void OnParticleUpdateJobScheduled()
        {
            JobHandle inputDeps = default;

            foreach (var particleTrigger in particleTriggers)
            {
                inputDeps = particleTrigger.Schedule(ps, multiplier, size, psRenderer, inputDeps);
            }


#if UNITY_EDITOR
            if (Application.isPlaying == false) ParticleTriggerManager.autoCreate();
#endif
            foreach (var particleTrigger in ParticleTriggerManager.singleton.getParticleTriggers(tag))
            {
                inputDeps = particleTrigger.Schedule(ps, multiplier, size, psRenderer, inputDeps);
            }
        }
    }
}
