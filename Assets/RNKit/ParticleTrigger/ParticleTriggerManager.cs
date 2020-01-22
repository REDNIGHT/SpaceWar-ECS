using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.ParticleSystem;

namespace RN
{
    public class ParticleTriggerManager : Singleton<ParticleTriggerManager>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]//这个只执行一次
        static void Initialize()
        {
            ParticleTriggerManager.autoCreate();
            GameObject.DontDestroyOnLoad(ParticleTriggerManager.singletonGO);
        }

        HashSet<ParticleTrigger> _particleTriggers = new HashSet<ParticleTrigger>();

        public IEnumerable<ParticleTrigger> getParticleTriggers(string tag)
        {
            foreach (var pt in _particleTriggers)
                if (pt.tagFilter == tag)
                    yield return pt;
        }

        internal void addParticleTrigger(ParticleTrigger particleTrigger)
        {
            _particleTriggers.Add(particleTrigger);
        }
        internal void removeParticleTrigger(ParticleTrigger particleTrigger)
        {
            _particleTriggers.Remove(particleTrigger);
        }
    }


    [ExecuteInEditMode]
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


        protected void OnEnable()
        {
            if (tagFilter.isNullOrEmpty() == false)
            {
#if UNITY_EDITOR
                if (Application.isPlaying == false) ParticleTriggerManager.autoCreate();
#endif
                ParticleTriggerManager.singleton.addParticleTrigger(this);
            }
        }

        private void OnDisable()
        {
            if (tagFilter.isNullOrEmpty() == false && ParticleTriggerManager.singleton != null)
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


        internal JobHandle Schedule(ParticleSystem ps, float multiplier, float particleSize, ParticleSystemRenderer psRenderer, JobHandle inputDeps)
        {
            if (side == Side.InSide)
            {
                if (psRenderer.bounds.Intersects(bounds))
                    return onSchedule(ps, multiplier, particleSize, inputDeps);
                else
                    return inputDeps;
            }
            else
            {
                return onSchedule(ps, multiplier, particleSize, inputDeps);
            }
        }

        static Particle[] _particles = new Particle[512];
        protected virtual JobHandle onSchedule(ParticleSystem ps, float multiplier, float particleSize, JobHandle inputDeps)
        {
            if (ps.particleCount > _particles.Length)
                _particles = new Particle[ps.particleCount];

            ps.GetParticles(_particles);

            onSchedule(_particles, ps.particleCount, multiplier, particleSize);

            ps.SetParticles(_particles, ps.particleCount);
            return inputDeps;
        }

        protected virtual void onSchedule(Particle[] particles, int count, float multiplier, float particleSize)
        {
            var sqrRadius = radius * radius;
            var sqrParticleSize = particleSize * particleSize;

            for (int i = 0; i < count; ++i)
            {
                if (condition(particles[i], sqrRadius, sqrParticleSize))
                {
                    onSchedule(ref particles[i], multiplier);
                }
            }
        }

        protected virtual bool condition(in Particle particle, float sqrRadius, float sqrParticleSize)
        {
            float sqrDistance = (particle.position - transform.position).sqrMagnitude;

            if (side == Side.InSide)
            {
                if (sqrDistance - sqrParticleSize < sqrRadius)
                {
                    return true;
                }
            }
            else
            {
                if (sqrDistance - sqrParticleSize > sqrRadius)
                {
                    return true;
                }
            }

            return false;
        }

        protected abstract void onSchedule(ref Particle particle, float multiplier);

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
