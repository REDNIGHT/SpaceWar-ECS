using System.Collections;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public abstract class ActorFx<T> : MonoBehaviour, IActorFx where T : ActorSpawner
    {
        public void OnCreateFx(ActorSpawner actorSpawner)
        {
            var _actorSpawner = actorSpawner as T;
            Debug.Assert(_actorSpawner != null, $"_actorSpawner != null  {typeof(T)}.  actorSpawner={actorSpawner}  this={this}", this);

            onCreateFx(_actorSpawner);
        }
        public void OnDestroyFx(ActorSpawner actorSpawner)
        {
            var _actorSpawner = actorSpawner as T;
            Debug.Assert(_actorSpawner != null, $"_actorSpawner != null  {typeof(T)}.  actorSpawner={actorSpawner}  this={this}", this);

            onDestroyFx(_actorSpawner);
        }

        public abstract void onCreateFx(T actorSpawner);
        public abstract void onDestroyFx(T actorSpawner);

        protected void playDestroyFx(Transform destroyFxT, T actorSpawner)
        {
#if UNITY_EDITOR
            destroyFxT.name += "  " + name;
#endif

            destroyFxT.transform.parent = actorSpawner.root;
            destroyFxT.gameObject.SetActive(true);

            if (destroyFxT.GetComponentInChildren<ParticleSystem>() != null)
            {
                destroyFxT.autoDestroyRootParticleSystem(false);
            }
            else if (destroyFxT.GetComponentInChildren<ParticleSystemForceField>() != null)
            {
                destroyFxT.destroyGO(2f);
            }
        }

        protected void continueFx(Transform fxT, T actorSpawner)
        {
#if UNITY_EDITOR
            fxT.name += "  " + name;
#endif
            fxT.transform.parent = actorSpawner.root;

            fxT.autoDestroyRootParticleSystem(true);
        }

        protected void continueMultiFxs(Transform fxT, T actorSpawner)
        {
#if UNITY_EDITOR
            fxT.name += "  " + name;
#endif
            fxT.transform.parent = actorSpawner.root;

            fxT.autoDestroyMultiParticleSystem(true);
        }



        protected void validateDestroyFx(Transform destroyFxT)
        {
            destroyFxT.gameObject.SetActive(false);

            var ps = destroyFxT.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                if (main.stopAction == ParticleSystemStopAction.Destroy)
                {
                    Debug.LogWarning(ps.name + ".main.stopAction == ParticleSystemStopAction.Destroy", ps);
                    main.stopAction = ParticleSystemStopAction.None;
                }
            }
            else if (destroyFxT.GetComponentInChildren<ParticleSystemForceField>() != null)
            {
            }
        }

        protected void validateContinueFx(Transform continueFxT)
        {
            var ps = continueFxT.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                if (main.stopAction == ParticleSystemStopAction.Destroy)
                {
                    Debug.LogWarning(ps.name + ".main.stopAction == ParticleSystemStopAction.Destroy", ps);
                    main.stopAction = ParticleSystemStopAction.None;
                }
            }
        }

        protected void validateContinueMultiFxs(Transform continueFxT)
        {
            var pss = continueFxT.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in pss)
            {
                var main = ps.main;
                if (main.stopAction == ParticleSystemStopAction.Destroy)
                {
                    Debug.LogWarning(ps.name + ".main.stopAction == ParticleSystemStopAction.Destroy", ps);
                    main.stopAction = ParticleSystemStopAction.None;
                }
            }
        }
    }
}
