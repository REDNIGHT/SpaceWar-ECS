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
    }
}
