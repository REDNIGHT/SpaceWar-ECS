using UnityEngine;
using System.Collections;
using UnityEngine.Animations;
//using UnityEngine.Experimental.Director;

namespace RN
{
    [SharedBetweenAnimators]
    public class StateMachine : StateMachineBehaviour
    {
        //public StateMachineRoot stateMachineRoot;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            animator.transform.parent.GetComponent<StateMachineRoot>().sendStateMessage(stateInfo.fullPathHash);
        }


        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller)
        {
            Debug.LogError(this + ".OnStateMachineEnter", this);
        }
        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller)
        {
            Debug.LogError(this + ".OnStateMachineExit", this);
        }


        /*public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            animator.transform.parent.GetComponent<StateMachineRoot>().sendEndStateMessage(stateInfo.fullPathHash);
        }*/
        /*public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            Debug.Log(this + ".OnStateUpdate", this);
        }
        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            Debug.Log(this + ".OnStateMove", this);
        }
        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            Debug.Log(this + ".OnStateIK", this);
        }*/
    }
}