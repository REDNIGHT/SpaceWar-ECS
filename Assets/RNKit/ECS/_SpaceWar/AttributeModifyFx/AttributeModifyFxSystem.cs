using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class AttributeModifyFxClientSystem : ComponentSystem
    {
        public float fadeOutTime = 0.5f;
        protected override void OnUpdate()
        {
            Entities
                .WithAll<TextMesh>().WithAllReadOnly<AttributeModifyFx, ActorLifetime>()
                .ForEach((ref ActorLifetime lifetime, TextMesh textMesh) =>
                {
                    if (lifetime.value < fadeOutTime)
                    {
                        var color = textMesh.color;

                        color.a = lifetime.value / fadeOutTime;

                        textMesh.color = color;


                        //textMesh.transform.forward = Camera.main.transform.forward;
                    }
                });
        }
    }
}
