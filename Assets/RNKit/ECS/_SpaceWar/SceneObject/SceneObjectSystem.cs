using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class SceneObjectAutoResetServerSystem : ComponentSystem
    {
        //在这半径范围内没有角色才会重置
        public LayerMask actorLayerMask = -1;
        public float radius = 100f;

        public float interval = 5f;
        float time = 0f;
        Collider[] results = new Collider[1];
        protected override void OnUpdate()
        {
            Entities
                .WithAll<SceneObjectAutoReset>().WithAllReadOnly<Translation, Rotation, OnCreateMessage>()
                .ForEach((ref Translation translation, ref Rotation rotation, ref SceneObjectAutoReset autoReset) =>
                {
                    autoReset.defaultPosition = translation.Value;
                    autoReset.defaultRotation = rotation.Value;
                });



            time += Time.fixedDeltaTime;
            if (time >= interval)
            {
                time -= interval;

                Entities
                    .WithAll<Translation, Rotation>().WithAllReadOnly<SceneObjectAutoReset>()
                    .ForEach((ref Translation translation, ref Rotation rotation, ref SceneObjectAutoReset autoReset) =>
                    {
                        int numFound = Physics.OverlapSphereNonAlloc(translation.Value, radius, results, actorLayerMask);
                        if (numFound == 0)
                        {
                            translation.Value = autoReset.defaultPosition;
                            rotation.Value = autoReset.defaultRotation;
                        }
                    });
            }
        }
    }

}
