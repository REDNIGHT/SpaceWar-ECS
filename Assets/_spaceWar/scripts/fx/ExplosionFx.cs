using RN.Network;
using UnityEngine;

namespace RN.Network.SpaceWar.Fx
{
    public class ExplosionFx : ActorFx<ExplosionSpawner>
    {
        public override void onCreateFx(ExplosionSpawner actorSpawner)
        {
            Debug.Assert(actorSpawner.isClient == true, "isClient == true", this);

            var ps = GetComponent<ParticleSystem>();
            var shape = ps.shape;
            shape.radius = actorSpawner.radius;
        }

        public override void onDestroyFx(ExplosionSpawner actorSpawner)
        {
            Debug.Assert(actorSpawner.isClient == true, "isClient == true", this);
            //在客户端 爆炸效果会自己删除
        }
    }
}