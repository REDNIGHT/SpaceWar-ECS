using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class AccelerateFx : MonoBehaviour, IAccelerateFx
    {
        const float dampTime = 0.9f;
        Animator[] animators;
        static int accelerateFxHash = Animator.StringToHash("accelerateFx");
        void Awake()
        {
            animators = GetComponentsInChildren<Animator>();
            Debug.Assert(animators.Length > 0, "animators.Length > 0  " + name, this);
        }

        public void OnPlayFx()
        {
            foreach (var animator in animators)
            {
                animator.SetFloat(accelerateFxHash, 1f);
            }
        }

        void FixedUpdate()
        {
            foreach (var animator in animators)
            {
                animator.SetFloat(accelerateFxHash, 0f, dampTime, Time.fixedDeltaTime);
            }
        }
    }
}
