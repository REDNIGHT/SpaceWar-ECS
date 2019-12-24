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
                //ps.Stop();
                destroyFx.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"destroyFx == null  this={this}", this);
            }


            GameObject.Destroy(gameObject);
        }
    }
}