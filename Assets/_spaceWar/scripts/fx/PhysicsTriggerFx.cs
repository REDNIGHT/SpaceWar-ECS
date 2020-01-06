using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class PhysicsTriggerFx : ActorFx<PhysicsTriggerSpawner>
    {
        public override void onCreateFx(PhysicsTriggerSpawner actorSpawner)
        {
        }

        public override void onDestroyFx(PhysicsTriggerSpawner actorSpawner)
        {
            var destroyFx = transform.GetChild(PhysicsTriggerSpawner.DestroyFx_TransformIndex);
            if (destroyFx != null)
            {
                destroyFx.transform.parent = actorSpawner.root;
                destroyFx.gameObject.SetActive(true);


#if UNITY_EDITOR
                var ps = destroyFx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Debug.Assert(ps.main.stopAction == ParticleSystemStopAction.Destroy, name + "  ps.main.stopAction == ParticleSystemStopAction.Destroy", ps);

                    ps.name += "  " + name;
                }
#endif
            }
            else
            {
                Debug.LogWarning($"destroyFx == null  this={this}", this);
            }


            GameObject.Destroy(gameObject);
        }
    }
}