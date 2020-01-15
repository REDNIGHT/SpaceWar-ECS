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
            var destroyFxT = transform.GetChild(BatterySpawner.DestroyFx_TransformIndex);

            playDestroyFx(destroyFxT, actorSpawner);

            this.destroyGO();
        }

        private void OnValidate()
        {
            var destroyFxT = transform.GetChild(BatterySpawner.DestroyFx_TransformIndex);
            validateDestroyFx(destroyFxT);
        }
    }
}