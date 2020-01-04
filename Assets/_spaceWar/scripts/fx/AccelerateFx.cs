using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class AccelerateFx : MonoBehaviour, IAccelerateFx
    {
        public const float dragA = 0.05f;
        public const float dragB = 2500f;
        public const float minLifetime = 0.5f;
        public const float maxLifetime = 1.5f;
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
                main.startLifetimeMultiplier = minLifetime;

                var emission = ps.emission;
                emission.rateOverDistanceMultiplier = minRateOverDistance;
            }
        }

        public void OnPlayFx()
        {
            foreach (var ps in _particleSystems)
            {
                var main = ps.main;
                main.startLifetimeMultiplier = maxLifetime;

                var emission = ps.emission;
                emission.rateOverDistanceMultiplier = maxRateOverDistance;
            }
        }

        void FixedUpdate()
        {
            var dA = dragA * Time.fixedDeltaTime;
            var dB = dragB * Time.fixedDeltaTime;

            foreach (var ps in _particleSystems)
            {
                var main = ps.main;
                if (main.startLifetimeMultiplier > minLifetime)
                {
                    main.startLifetimeMultiplier -= dA;
                    if (main.startLifetimeMultiplier < minLifetime)
                        main.startLifetimeMultiplier = minLifetime;

                    var emission = ps.emission;
                    emission.rateOverDistanceMultiplier -= dB;
                    if (emission.rateOverDistanceMultiplier < minRateOverDistance)
                        emission.rateOverDistanceMultiplier = minRateOverDistance;
                }
            }
        }
    }
}
