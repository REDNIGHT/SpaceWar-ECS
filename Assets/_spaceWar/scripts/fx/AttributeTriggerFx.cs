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
            var fxT = transform.GetChild(AttributeTriggerSpawner.Fx_TransformIndex);
            var destroyFxT = transform.GetChild(AttributeTriggerSpawner.DestroyFx_TransformIndex);

            playDestroyFx(destroyFxT, actorSpawner);
            continueFx(fxT, actorSpawner);

            this.destroyGO();
        }
    }
}