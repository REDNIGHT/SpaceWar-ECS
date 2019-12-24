using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class BatteryFx : ActorFx<BatterySpawner>
    {
        public override void onCreateFx(BatterySpawner actorSpawner)
        {
        }

        public override void onDestroyFx(BatterySpawner actorSpawner)
        {
            var destroyFx = transform.GetChild(BatterySpawner.DestroyFx_TransformIndex);
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