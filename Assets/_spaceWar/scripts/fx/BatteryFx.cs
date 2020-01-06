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