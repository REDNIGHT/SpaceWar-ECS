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
            var fxT = transform.GetChild(MissileSpawner.fx_TransformIndex);

            continueFx(fxT, actorSpawner);

            this.destroyGO();
        }
    }
}