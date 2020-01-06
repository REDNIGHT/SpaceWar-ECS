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


            //
            {
                var fx = transform.GetChild(AttributeTriggerSpawner.Fx_TransformIndex);
                var ps = fx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.transform.parent = actorSpawner.root;

                    ps.Stop();
                    var main = ps.main;
                    main.stopAction = ParticleSystemStopAction.Destroy;


#if UNITY_EDITOR
                    ps.name += "  " + name;
#endif
                }


                GameObject.Destroy(gameObject);
            }
        }
    }
}