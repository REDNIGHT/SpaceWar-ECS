using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.ParticleSystem;

namespace RN
{
    class ParticleTriggerManager : Singleton<ParticleTriggerManager>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]//这个只执行一次
        static void Initialize()
        {
            SceneManager.sceneLoaded += sceneLoaded;
            //SceneManager.sceneUnloaded += sceneUnloaded;
        }

        static void sceneLoaded(Scene s, LoadSceneMode m)
        {
            var go = new GameObject();
            go.hideFlags = HideFlags.HideInHierarchy;
            if (Application.isPlaying == false)
                go.hideFlags = HideFlags.HideAndDontSave;

            go.name = "ParticleTriggerSystem";
            go.AddComponent<ParticleTriggerManager>();
        }

        HashSet<ParticleTrigger> particleTriggers = new HashSet<ParticleTrigger>();
        internal void addParticleTrigger(ParticleTrigger particleTrigger)
        {
            particleTriggers.Add(particleTrigger);

            foreach (var particleTriggerInParticleSystem in particleTriggerInParticleSystems)
            {
                if (particleTriggerInParticleSystem.CompareTag(particleTrigger.tagFilter))
                    particleTriggerInParticleSystem.add(particleTrigger);
            }
        }
        internal void removeParticleTrigger(ParticleTrigger particleTrigger)
        {
            particleTriggers.Remove(particleTrigger);

            foreach (var particleTriggerInParticleSystem in particleTriggerInParticleSystems)
            {
                if (particleTriggerInParticleSystem.CompareTag(particleTrigger.tagFilter))
                    particleTriggerInParticleSystem.remove(particleTrigger);
            }
        }


        HashSet<ParticleTriggerInParticleSystem> particleTriggerInParticleSystems = new HashSet<ParticleTriggerInParticleSystem>();
        internal void addParticleTriggerInParticleSystem(ParticleTriggerInParticleSystem particleTriggerInParticleSystem)
        {
            particleTriggerInParticleSystems.Add(particleTriggerInParticleSystem);

            foreach (var particleTrigger in particleTriggers)
            {
                if (particleTriggerInParticleSystem.CompareTag(particleTrigger.tagFilter))
                {
                    particleTriggerInParticleSystem.add(particleTrigger);
                }
            }
        }
        internal void removeParticleTriggerInParticleSystem(ParticleTriggerInParticleSystem particleTriggerInParticleSystem)
        {
            particleTriggerInParticleSystems.Remove(particleTriggerInParticleSystem);

            foreach (var particleTrigger in particleTriggers)
            {
                if (particleTriggerInParticleSystem.CompareTag(particleTrigger.tagFilter))
                {
                    particleTriggerInParticleSystem.remove(particleTrigger);
                }
            }
        }
    }


    public abstract class ParticleTrigger : MonoBehaviour
    {
        public string tagFilter;

        public float radius = 1f;

        public enum Side
        {
            InSide,
            OutSide,
        }
        public Side side;


        private void OnEnable()
        {
            if (tagFilter.isNullOrEmpty() == false)
            {
                ParticleTriggerManager.singleton.addParticleTrigger(this);
            }
        }

        private void OnDisable()
        {
            if (tagFilter.isNullOrEmpty() == false)
            {
                ParticleTriggerManager.singleton.removeParticleTrigger(this);
            }
        }

        Bounds bounds
        {
            get
            {
                var s = radius * 2f;
                return new Bounds(transform.position, new Vector3(s, s, s));
            }
        }


        internal JobHandle Schedule(ParticleSystem ps, in MinMaxCurve multiplier, ParticleSystemRenderer psRenderer, JobHandle inputDeps)
        {
            if (side == Side.InSide)
            {
                if (psRenderer.bounds.Intersects(bounds))
                    return onSchedule(ps, multiplier, inputDeps);
                else
                    return inputDeps;
            }
            else
            {
                return onSchedule(ps, multiplier, inputDeps);
            }
        }

        static Particle[] _particles = new Particle[512];
        protected virtual JobHandle onSchedule(ParticleSystem ps, in MinMaxCurve multiplier, JobHandle inputDeps)
        {
            if (ps.particleCount > _particles.Length)
                _particles = new Particle[ps.particleCount];

            ps.GetParticles(_particles);

            onSchedule(_particles, ps.particleCount, multiplier);

            ps.SetParticles(_particles, ps.particleCount);
            return inputDeps;
        }

        protected virtual void onSchedule(Particle[] particles, int count, in MinMaxCurve multiplier)
        {
            var sqrRadius = radius * radius;

            for (int i = 0; i < count; ++i)
            {
                float sqrDistance = (particles[i].position - transform.position).sqrMagnitude;

                if (side == Side.InSide)
                {
                    if (sqrDistance < sqrRadius)
                    {
                        onSchedule(ref particles[i], in multiplier);
                    }
                }
                else
                {
                    if (sqrDistance > sqrRadius)
                    {
                        onSchedule(ref particles[i], in multiplier);
                    }
                }
            }
        }

        protected abstract void onSchedule(ref Particle particles, in MinMaxCurve multiplier);

        /*{
            //[BurstCompile] // Enable if using the Burst package
            struct UpdateParticlesJob : IJobParticleSystem
            {
                public Color color;
                public float size;

                public void Execute(ParticleSystemJobData particles)
                {
                    var startColors = particles.startColors;
                    var sizes = particles.sizes.x;

                    for (int i = 0; i < particles.count; i++)
                    {
                        startColors[i] = color;
                        sizes[i] = size;
                    }
                }
            }
        }*/


        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
