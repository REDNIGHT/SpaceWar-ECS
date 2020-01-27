using System.Collections;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class WeaponFx : ActorFx<WeaponSpawner>, IWeaponInstalledFx
    {
        public override void onCreateFx(WeaponSpawner actorSpawner)
        {
        }

        public override void onDestroyFx(WeaponSpawner actorSpawner)
        {
            StartCoroutine(onDestroyFxE(actorSpawner));
        }

        IEnumerator onDestroyFxE(WeaponSpawner actorSpawner)
        {
            yield return new WaitForSeconds(0.25f);

            var destroyFxT = transform.GetChild(WeaponSpawner.DestroyFx_TransformIndex);

            playDestroyFx(destroyFxT, actorSpawner);

            this.destroyGO();
        }

        private void OnValidate()
        {
            var destroyFxT = transform.GetChild(WeaponSpawner.DestroyFx_TransformIndex);
            validateDestroyFx(destroyFxT);
        }



        public void OnPlayInstalledFx()
        {
            GetComponents<AudioSource>()[0].Play();
        }

        public void OnPlayUninstalledFx()
        {
            GetComponents<AudioSource>()[1].Play();
        }

    }
}