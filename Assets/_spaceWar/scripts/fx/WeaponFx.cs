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
            var destroyFxT = transform.GetChild(WeaponSpawner.DestroyFx_TransformIndex);

            playDestroyFx(destroyFxT, actorSpawner);

            this.destroyGO();
        }
    }
}