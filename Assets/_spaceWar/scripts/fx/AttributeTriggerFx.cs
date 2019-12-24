using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class AttributeTriggerFx : ActorFx<AttributeTriggerSpawner>
    {
        public override void onCreateFx(AttributeTriggerSpawner actorSpawner)
        {
        }

        public override void onDestroyFx(AttributeTriggerSpawner actorSpawner)
        {
            var destroyFx = transform.GetChild(AttributeTriggerSpawner.DestroyFx_TransformIndex);
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