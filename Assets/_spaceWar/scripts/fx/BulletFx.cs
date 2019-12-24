using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class BulletFx : ActorFx<BulletSpawner>
    {
        public override void onCreateFx(BulletSpawner actorSpawner)
        {
        }

        public override void onDestroyFx(BulletSpawner actorSpawner)
        {
            gameObject.SetActive(false);
            GameObject.Destroy(gameObject);
        }
    }
}