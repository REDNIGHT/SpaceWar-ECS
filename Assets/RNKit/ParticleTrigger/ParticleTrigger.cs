using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
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

        List<BaseParticleTrigger> _particleTriggers = new List<BaseParticleTrigger>();

        public IEnumerable<BaseParticleTrigger> getParticleTriggers(string tag)
        {
            foreach (var pt in _particleTriggers)
                if (pt.tagFilter == tag)
                    yield return pt;
        }

        internal void addParticleTrigger(BaseParticleTrigger particleTrigger)
        {
            _particleTriggers.Add(particleTrigger);
        }
        internal void removeParticleTrigger(BaseParticleTrigger particleTrigger)
        {
            _particleTriggers.Remove(particleTrigger);
        }
    }

    [ExecuteInEditMode]
    public abstract class BaseParticleTrigger : MonoBehaviour
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


        internal JobHandle Schedule(ParticleSystem ps, float particleSize, ParticleSystemRenderer psRenderer, JobHandle inputDeps)
        {
            if (side == Side.InSide)
            {
                if (psRenderer.bounds.Intersects(bounds))
                    return onSchedule(ps, particleSize, inputDeps);
                else
                    return inputDeps;
            }
            else
            {
                return onSchedule(ps, particleSize, inputDeps);
            }
        }

        static Particle[] _particles = new Particle[512];
        protected virtual JobHandle onSchedule(ParticleSystem ps, float particleSize, JobHandle inputDeps)
        {
            if (ps.particleCount > _particles.Length)
                _particles = new Particle[ps.particleCount];

            ps.GetParticles(_particles);

            onSchedule(_particles, ps.particleCount, particleSize);

            ps.SetParticles(_particles, ps.particleCount);
            return inputDeps;
        }

        protected virtual void onSchedule(Particle[] particles, int count, float particleSize)
        {
            var sqrRadius = radius * radius;
            var sqrParticleSize = particleSize * particleSize;

            for (int i = 0; i < count; ++i)
            {
                if (condition(particles[i], sqrRadius, sqrParticleSize))
                {
                    onSchedule(ref particles[i]);
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

        protected abstract void onSchedule(ref Particle particle);
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
            //Gizmos.DrawWireCube(transform.position, new Vector3(radius * 2f, radius * 2f, radius * 2f));
        }
    }



    [ExecuteInEditMode]
    public class ParticleTrigger : BaseParticleTrigger
    {
        /// <summary>
        /// Particle.remainingLifetime大于overLifetime的例子才会被采用
        /// 并且新粒子的Lifetime会设置成overLifetime  如果所有ParticleTrigger.overLifetime都是一样 那么粒子就只会被触发一次
        /// </summary>
        public float overLifetime = 2.5f;
        public ParticleSystem toPS;

        private void Awake()
        {
            Debug.Assert(toPS.main.simulationSpace == ParticleSystemSimulationSpace.World, "simulationSpace == ParticleSystemSimulationSpace.World", toPS);
        }


        protected override bool condition(in Particle particle, float sqrRadius, float sqrParticleSize)
        {
            if (particle.remainingLifetime > overLifetime)
            {
                return base.condition(particle, sqrRadius, sqrParticleSize);
            }

            return false;
        }

        protected override void onSchedule(ref Particle particle)
        {
            //
#if UNITY_EDITOR
            if (Application.isPlaying == false && toPS.isPlaying == false)
                toPS.Play();
#endif
            Debug.Assert(toPS.isPlaying == true, "toPS.isPlaying", toPS);

            var p = particle;
            p.velocity = Vector3.zero;
            //p.remainingLifetime = remainingLifetime;
            p.startLifetime = overLifetime;
            var emitParams = new EmitParams
            {
                particle = p,
            };

            toPS.Emit(emitParams, 1);


            //
            particle.remainingLifetime = 0f;
        }
    }
}
