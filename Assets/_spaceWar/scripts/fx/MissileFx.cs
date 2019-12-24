using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class MissileFx : ActorFx<MissileSpawner>
    {
        public override void onCreateFx(MissileSpawner actorSpawner)
        {
        }

        public override void onDestroyFx(MissileSpawner actorSpawner)
        {
            var ps = transform.GetChild(MissileSpawner.fx_TransformIndex).GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.transform.parent = actorSpawner.root;
                ps.Stop();
            }
            else
            {
                Debug.LogWarning($"ps == null  this={this}", this);
            }

            GameObject.Destroy(gameObject);
        }
    }

}