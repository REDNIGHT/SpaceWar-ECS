using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class WeaponFx : ActorFx<WeaponSpawner>
    {
        public override void onCreateFx(WeaponSpawner actorSpawner)
        {
        }

        public override void onDestroyFx(WeaponSpawner actorSpawner)
        {
            var destroyFx = transform.GetChild(WeaponSpawner.DestroyFx_TransformIndex).GetComponent<ParticleSystem>();
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