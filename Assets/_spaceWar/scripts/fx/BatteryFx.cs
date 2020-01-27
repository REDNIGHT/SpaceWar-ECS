using System.Collections;
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
            StartCoroutine(onDestroyFxE(actorSpawner));
        }

        IEnumerator onDestroyFxE(BatterySpawner actorSpawner)
        {
            yield return new WaitForSeconds(0.25f);

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