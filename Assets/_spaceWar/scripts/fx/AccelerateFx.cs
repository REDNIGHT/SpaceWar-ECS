using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class AccelerateFx : MonoBehaviour, IAccelerateFx
    {
        public const float drag = 0.01f;
        public const float defaultLifetime = 0.2f;
        public const float minRateOverDistance = 5f;
        public const float maxRateOverDistance = 50f;
        ParticleSystem[] _particleSystems;
        void Awake()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
            Debug.Assert(_particleSystems.Length > 0, "_particleSystems.Length > 0", this);

            foreach (var ps in _particleSystems)
            {
                var main = ps.main;
                main.startLifetimeMultiplier = defaultLifetime;

                var emission = ps.emission;
                emission.rateOverDistanceMultiplier = minRateOverDistance;
            }
        }

        public void OnPlayFx()
        {
            foreach (var ps in _particleSystems)
            {
                var main = ps.main;
                main.startLifetimeMultiplier = 1f;

                var emission = ps.emission;
                emission.rateOverDistanceMultiplier = maxRateOverDistance;
            }
        }

        void FixedUpdate()
        {
            var dA = drag * Time.fixedDeltaTime;
            var dB = dA * 2500f;
            foreach (var ps in _particleSystems)
            {
                var main = ps.main;
                if (main.startLifetimeMultiplier > defaultLifetime)
                {
                    main.startLifetimeMultiplier -= dA;
                    if (main.startLifetimeMultiplier < defaultLifetime)
                        main.startLifetimeMultiplier = defaultLifetime;

                    var emission = ps.emission;
                    emission.rateOverDistanceMultiplier -= dB;
                    if (emission.rateOverDistanceMultiplier < minRateOverDistance)
                        emission.rateOverDistanceMultiplier = minRateOverDistance;
                }
            }
        }
    }
}
