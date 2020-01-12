using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class ForceFx : MonoBehaviour, IForceFx
    {
        float max = 10f;
        float forceMax = 0.75f;
        float torqueMax = 1f;
        float accelerate = 4f;
        Animator[] animatorsL;
        Animator[] animatorsR;
        Animator[] animatorsM;

        public const int M_Fx_TransformIndex = 0;
        public const int L_Fx_TransformIndex = 1;
        public const int R_Fx_TransformIndex = 2;
        void Awake()
        {
            animatorsL = transform.GetChild(L_Fx_TransformIndex).GetComponentsInChildren<Animator>();
            Debug.Assert(animatorsL.Length > 0, "animatorsL.Length > 0  " + name, this);

            animatorsR = transform.GetChild(R_Fx_TransformIndex).GetComponentsInChildren<Animator>();
            Debug.Assert(animatorsR.Length > 0, "animatorsR.Length > 0  " + name, this);

            animatorsM = transform.GetChild(M_Fx_TransformIndex).GetComponentsInChildren<Animator>();

            foreach(var ps in transform.GetComponentsInChildren<ParticleSystem>())
            {
                var e = ps.emission;
                e.rateOverTimeMultiplier = 0;
            }
        }


        static int forceFxHash = Animator.StringToHash("forceFx");
        public void OnPlayFx(in ShipForceAttribute forceAttribute)
        {
            foreach (var animator in animatorsL)
            {
                var v = forceAttribute.force * forceMax;
                var torque = forceAttribute.torque * torqueMax;
                if (torque > 0f)
                    v += torque;
                if (forceAttribute.accelerate)
                    v += accelerate;
                animator.SetFloat(forceFxHash, v / max);
            }
            foreach (var animator in animatorsR)
            {
                var v = forceAttribute.force * forceMax;
                var torque = -forceAttribute.torque * torqueMax;
                if (torque > 0f)
                    v += torque;
                if (forceAttribute.accelerate)
                    v += accelerate;
                animator.SetFloat(forceFxHash, v / max);
            }
            foreach (var animator in animatorsM)
            {
                var v = forceAttribute.force * forceMax;
                if (forceAttribute.accelerate)
                    v += accelerate;
                animator.SetFloat(forceFxHash, v / max);
            }
        }
    }
}
