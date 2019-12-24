using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RN
{
    public class StateMachineRoot : MonoBehaviour
    {
        protected void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
                Debug.LogError("animator == null", this);

            /*var controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;

            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    foreach (var behaviour in state.state.behaviours)
                    {
                        var sm = behaviour as StateMachine;
                        if (sm != null)
                            sm.stateMachineRoot = this;
                    }

                    foreach (var stateMachine in layer.stateMachine.stateMachines)
                    {
                        foreach (var behaviour in stateMachine.stateMachine.behaviours)
                        {
                            var sm = behaviour as StateMachine;
                            if (sm != null)
                                sm.stateMachineRoot = this;
                        }
                    }
                }
            }*/

#if UNITY_EDITOR
            errTest();
#endif
        }


        //这里为什么不直接用StateNode 而是用 KeyValuePair<int, StateNode>
        //这样做可以多个hash对应一个StateNode
        List<KeyValuePair<int, StateNode>> states = new List<KeyValuePair<int, StateNode>>();

        /*public StateNode getState(string stateName)
        {
            foreach (var state in states)
            {
                if (state.Value.stateName == stateName)
                    return state.Value;
            }
            return null;
        }*/

        protected StateNode getState(int stateHash)
        {
            foreach (var state in states)
            {
                if (state.Key == stateHash)
                    return state.Value;
            }
            return null;
        }

        public void add(int hash, StateNode state)
        {
#if UNITY_EDITOR
            foreach (var s in states)
                if (s.Key == state.hash)
                    Debug.LogError("s.stateHash == stateHash  state=" + state, state);
#endif
            states.Add(new KeyValuePair<int, StateNode>(hash, state));
        }

        public void remove(int hash, StateNode state)
        {
            foreach (var s in states)
            {
                if (s.Key == hash && s.Value == state)
                {
                    states.Remove(s);
                    break;
                }
            }
        }


        //
#if UNITY_EDITOR
        //
        [RN._Editor.ButtonInBeginLeftArea()]
        static void logState()
        {
            _logState = true;
        }
        static bool _logState = false;
#endif


        public void sendStateMessage(int stateHash)
        {
            //end
            {
                if (vCurState != null)
                {
                    vCurState.BroadcastMessage("onEndState", vCurState, SendMessageOptions.DontRequireReceiver);
                    //Debug.Log(transform.name + "." + vCurState.name + ".onEndState", vCurState);
                }
            }

            //begin
            {
                var state = getState(stateHash);

                _stateChange(state);

#if UNITY_EDITOR
                if (_logState)
                    Debug.Log(transform.name + "." + state.name, state);
#endif

                this.SendMessage("onStateChange", state, SendMessageOptions.DontRequireReceiver);
                state.BroadcastMessage("onBeginState", state, SendMessageOptions.DontRequireReceiver);
            }
        }

        //
        StateNode vCurState;
        public StateNode curState { get { return vCurState; } }
        public string curStateName { get { return vCurState.name; } }
        StateNode vLastState;
        public StateNode lastState { get { return vLastState; } }

        //public bool inAnyStateTransition { get { return vCurState == null ? false : vCurState.inThisTransition; } }


        //
        void _stateChange(StateNode state)
        {
            if (state.layer != 0)
            {
                Debug.LogError("//todo...  state.layer != 0", this);
                return;
            }

            //
            vLastState = vCurState;
            vCurState = state;
        }




        //--------------------------------------------------------------------------------------------
        //
        public Animator animator { get; protected set; }
        public void forcePlayState(string stateName, float transitionDuration, float normalizedTime_ = 0f, int layer = 0)
        {
            if (stateName.Length == 0)
                Debug.LogError("stateName.Length == 0", this);
            animator.CrossFade(stateName, transitionDuration, layer, normalizedTime_);
        }
        public float speed
        {
            get { return animator.speed; }
            set
            {
                animator.speed = value;
            }
        }



#if UNITY_EDITOR
        //
        [RN._Editor.ButtonInBeginLeftArea()]
        protected void addRNStateMachine()
        {
            var _animator = GetComponentInChildren<Animator>();
            var controller = _animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;

            foreach (var layer in controller.layers)
            {
                addRNStateMachine(layer.stateMachine);

                foreach (var stateMachine in layer.stateMachine.stateMachines)
                    addRNStateMachine(stateMachine.stateMachine);
            }
        }
        void addRNStateMachine(UnityEditor.Animations.AnimatorStateMachine stateMachine)
        {
            foreach (var state in stateMachine.states)
            {
                var has = false;
                foreach (var behaviour in state.state.behaviours)
                {
                    if (behaviour is StateMachine)
                    {
                        has = true;
                        break;
                    }
                }


                //
                if (has == false)
                    state.state.AddStateMachineBehaviour<StateMachine>();
            }
        }


        protected void errTest()
        {
            var controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;

            foreach (var layer in controller.layers)
            {
                errTest(layer.stateMachine, controller, layer);

                foreach (var stateMachine in layer.stateMachine.stateMachines)
                    errTest(stateMachine.stateMachine, controller, layer);
            }
        }

        protected void errTest
            (UnityEditor.Animations.AnimatorStateMachine stateMachine
            , UnityEditor.Animations.AnimatorController controller
            , UnityEditor.Animations.AnimatorControllerLayer layer)
        {
            foreach (var state in stateMachine.states)
            {
                var has = false;
                foreach (var behaviour in state.state.behaviours)
                    if (behaviour is StateMachine)
                    {
                        if (has)
                            Debug.LogError("have more RN.StateMachine.  pls remove more RN.StateMachine " + controller.name + "." + layer.name + "." + stateMachine.name + "." + state.state.name
                                + "\nor click [ContextMenu(add RN.StateMachine to controller)] in script StateMachineRoot", controller);
                        has = true;
                    }


                if (has == false)
                {
                    Debug.LogError("can't find RN.StateMachine.  pls add RN.StateMachine " + controller.name + "." + layer.name + "." + stateMachine.name + "." + state.state.name
                        + "\nor click [ContextMenu(add RN.StateMachine to controller)] in script StateMachineRoot", controller);
                }
            }
        }
#endif
    }
}