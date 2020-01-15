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
            var destroyFxT = transform.GetChild(ShipSpawner.DestroyFx_TransformIndex);
            var forceFxT = transform.GetChild(ShipSpawner.ForceFx_TransformIndex);


            playDestroyFx(destroyFxT, actorSpawner);
            continueMultiFxs(forceFxT, actorSpawner);


            //
            this.destroyGO();
        }


        private void OnValidate()
        {
            var destroyFxT = transform.GetChild(ShipSpawner.DestroyFx_TransformIndex);
            validateDestroyFx(destroyFxT);

            var forceFxT = transform.GetChild(ShipSpawner.ForceFx_TransformIndex);
            validateContinueFx(forceFxT);
        }
    }
}