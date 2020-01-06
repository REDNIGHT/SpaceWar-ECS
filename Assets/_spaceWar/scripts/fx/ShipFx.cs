using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class ShipFx : ActorFx<ShipSpawner>
    {
        private void Awake()
        {
            var shieldRoot = transform.GetChild(ShipSpawner.ShieldRoot_TransformIndex);
            foreach (Transform s in shieldRoot)
            {
                s.gameObject.SetActive(false);
            }
        }

        public override void onCreateFx(ShipSpawner actorSpawner)
        {
        }

        public override void onDestroyFx(ShipSpawner actorSpawner)
        {
            var destroyFx = transform.GetChild(ShipSpawner.DestroyFx_TransformIndex);
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