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
            var destroyFxT = transform.GetChild(PhysicsTriggerSpawner.DestroyFx_TransformIndex);

            playDestroyFx(destroyFxT, actorSpawner);

            this.destroyGO();
        }
    }
}