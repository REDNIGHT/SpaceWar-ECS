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
    }
}
