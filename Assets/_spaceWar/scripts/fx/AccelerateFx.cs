using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class AccelerateFx : MonoBehaviour, IAccelerateFx
    {
        public float rateOverDistance = 25f;
        //public float radius = 1f;

        public float drag = 0.1f;

        ParticleSystem _particleSystem;
        float defaultRateOverDistanceMultiplier;
        //float defaultRadius;
        void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            Debug.Assert(_particleSystem != null, "particleSystem != null", this);

            var emission = _particleSystem.emission;
            defaultRateOverDistanceMultiplier = emission.rateOverDistanceMultiplier;

            //var shape = particleSystem.shape;
            //defaultRadius = shape.radius;
        }

        public void OnPlayFx()
        {
            _particleSystem.time = 0f;

            //
            var shape = _particleSystem.shape;
            //shape.radius = radius;

            //
            var emission = _particleSystem.emission;

            //particleSystem.Emit((int)(rateOverDistance - emission.rateOverDistanceMultiplier));

            emission.rateOverDistanceMultiplier = rateOverDistance;
        }

        void FixedUpdate()
        {
            var d = 1f - drag * Time.fixedDeltaTime;

            var emission = _particleSystem.emission;
            if (emission.rateOverDistanceMultiplier > defaultRateOverDistanceMultiplier)
            {
                emission.rateOverDistanceMultiplier *= d;
                emission.rateOverDistanceMultiplier = Mathf.Max(emission.rateOverDistanceMultiplier, defaultRateOverDistanceMultiplier);

                //var shape = particleSystem.shape;
                //shape.radius *= d;
                //shape.radius = Mathf.Max(shape.radius, defaultRadius);
            }
        }
    }
}