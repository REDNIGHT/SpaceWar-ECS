using System.Collections;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class MissileFx : ActorFx<MissileSpawner>
    {
        public override void onCreateFx(MissileSpawner actorSpawner)
        {
        }

        public override void onDestroyFx(MissileSpawner actorSpawner)
        {
            var fxT = transform.GetChild(MissileSpawner.Fx_TransformIndex);

            continueFx(fxT, actorSpawner);

            this.destroyGO();
        }

        private void OnValidate()
        {
            var fxT = transform.GetChild(MissileSpawner.Fx_TransformIndex);
            validateContinueFx(fxT);
        }
    }
}