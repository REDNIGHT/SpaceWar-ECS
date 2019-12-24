using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RN
{
    public abstract class StateNode<TActor> : StateNode where TActor : StateMachineRoot
    {
        public TActor actor { get { return stateMachineRoot as TActor; } }
    }


    public class StateNode : MonoBehaviour
    {
        public Animator animator { get { return stateMachineRoot.animator; } }
        public StateMachineRoot stateMachineRoot { get; protected set; }

        protected void Awake()
        {
            stateMachineRoot = GetComponentInParent<StateMachineRoot>();
            if (stateMachineRoot == null)
                Debug.LogError("animatorEx == null", this);

            //
            /*rename();
            if (_stateName.Length == 0)
                Debug.LogError("stateName.Length == 0", this);*/

            vHash = Animator.StringToHash(animator.GetLayerName(layer) + "." + stateName);
        }

        protected void OnEnable()
        {
            stateMachineRoot.add(hash, this);
        }
        protected void OnDisable()
        {
            stateMachineRoot.remove(hash, this);
        }


        //protected void onBeginState() { }
        //protected void onEndState() { }

        internal virtual void onStateUpdate() { }
        internal virtual void onAnimatorMove() { }
        internal virtual void onStateLateUpdate() { }

        //---------------------------------------------------------------------------------------------
        [Header("StateNode")]
        public int layer = 0;
        public string stateName { get { return transform.name; } }
        protected int vHash = 0;
        public int hash { get { return vHash; } }

        //public bool animationMove = true;


        //
        /*protected void rename()
        {
            if (_stateName.isNullOrEmpty())
                _stateName = transform.name;
        }*/




        //---------------------------------------------------------------------------------------------
        public void play(float transitionDuration = 0.1f)
        {
            //Debug.Log(this + ".forcePlay");
            /*Debug.Log(name + "  stateHash=" + stateHash
                + "  transitionDuration=" + transitionDuration
                + "  layer=" + layer
                + "  normalizedTime=" + normalizedTime
                , this);*/
            animator.CrossFadeInFixedTime(hash, transitionDuration, layer);
        }
        [RN._Editor.ButtonInEndArea1]
        void _play()
        {
            play();
        }

        /*protected void doMyTrigger()
        {
            //Debug.Log(this + ".doMyTrigger");
            animator.SetTrigger(name);
        }

        protected void doMyBool(bool v, float stopTime = -1f)
        {
            //Debug.Log(this + ".doMyBool=" + v);
            animator.SetBool(name, v);
            if (stopTime > 0f)
                StartCoroutine(stopBool(stopTime, !v));
        }
        protected IEnumerator stopBool(float stopTime, bool v)
        {
            yield return new WaitForSeconds(stopTime);
            animator.SetBool(name, v);
        }
        protected bool getMyBool()
        {
            return animator.GetBool(name);
        }*/


        /*static int anyHash = Animator.StringToHash("any");
        protected void doAnyTrigger()
        {
            //Debug.Log(this + ".doAnyTrigger", this);
            animator.SetTrigger(anyHash);
        }*/





        //---------------------------------------------------------------------------------------------
        protected virtual bool hashEquals(int fullPathHash)
        {
            return fullPathHash == hash;
        }

        public AnimatorStateInfo animatorStateInfo
        {
            get
            {
                var info = new AnimatorStateInfo();

                if (animator.IsInTransition(layer))
                {
                    var nextAnimatorStateInfo = animator.GetNextAnimatorStateInfo(layer);
                    if (hashEquals(nextAnimatorStateInfo.fullPathHash))
                    {
                        return nextAnimatorStateInfo;
                    }
                    else
                    {
                        var curAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                        if (hashEquals(curAnimatorStateInfo.fullPathHash))
                        {
                            return curAnimatorStateInfo;
                        }
                    }
                }
                else
                {
                    var curAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                    if (hashEquals(curAnimatorStateInfo.fullPathHash))
                    {
                        return curAnimatorStateInfo;
                    }
                }


                Debug.LogError("can not find this AnimatorStateInfo  this=" + this, this);
                return info;
            }
        }
        public bool inThisState
        {
            get
            {
                if (animator.IsInTransition(layer))
                {
                    var nextAnimatorStateInfo = animator.GetNextAnimatorStateInfo(layer);
                    if (hashEquals(nextAnimatorStateInfo.fullPathHash))
                    {
                        return true;
                    }
                }

                var currAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                return hashEquals(currAnimatorStateInfo.fullPathHash);
            }
        }
        /*public bool inLastState
        {
            get
            {
                if (animator.IsInTransition(layer))
                {
                    var currAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                    return hashEquals(currAnimatorStateInfo.fullPathHash);
                }

                return false;
            }
        }*/
        public bool inThisTransition//当前动作是不包含过度阶段
        {
            get
            {
                if (animator.IsInTransition(layer))
                {
                    var nextAnimatorStateInfo = animator.GetNextAnimatorStateInfo(layer);
                    return hashEquals(nextAnimatorStateInfo.fullPathHash);
                }

                //Debug.LogError("not in this State", this);
                return false;
            }
        }

        public float normalizedTime
        {
            get
            {
                if (animator.IsInTransition(layer))
                {
                    var nextAnimatorStateInfo = animator.GetNextAnimatorStateInfo(layer);
                    if (hashEquals(nextAnimatorStateInfo.fullPathHash))
                    {
                        return nextAnimatorStateInfo.normalizedTime;
                    }
                }

                var curAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                if (hashEquals(curAnimatorStateInfo.fullPathHash))
                {
                    return curAnimatorStateInfo.normalizedTime;
                }

                Debug.LogError("not in this State", this);
                return -1f;
            }
        }
        public float curNormalizedTime
        {
            get
            {
                var curAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                if (hashEquals(curAnimatorStateInfo.fullPathHash))
                {
                    return curAnimatorStateInfo.normalizedTime;
                }

                Debug.LogError("not in this State", this);
                return -1f;
            }
        }



        //---------------------------------------------------------------------------------------------
        /*protected void onBeginState()
        {
            //
            //self.setAnimatorSpeed(_speed);

            //
            playAudio(0);
            playParticleSystem(0);
        }
        protected void onEndState()
        {
            playAudio(-1);
            playParticleSystem(-1);
        }
        protected virtual void playAudio(int index)
        {
            var a = GetComponentInChildren<AudioSource>(true);
            if (a == null)
                return;

            if (index == -1)
            {
                a.Stop();
                return;
            }

            a.Play();
        }
        protected virtual void playParticleSystem(int index)
        {
            var ps = GetComponentInChildren<ParticleSystem>(true);
            if (ps == null)
                return;

            if (index == -1)
            {
                ps.Stop();
                return;
            }

            ps.gameObject.SetActive(true);
            ps.Play();
        }*/
    }
}