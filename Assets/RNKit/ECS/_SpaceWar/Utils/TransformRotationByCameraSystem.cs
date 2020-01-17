using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class TransformRotationByCameraSystem : ComponentSystem
    {
        static readonly Quaternion offset = Quaternion.Euler(90f, 0f, 0f);

        protected override void OnUpdate()
        {
            var cameraData = GetSingleton<CameraDataSingleton>();

            Entities
                .WithAllReadOnly<TransformRotationByCamera>()
                .WithNone<OnDestroyMessage>()
                .ForEach((TransformRotationByCamera transformRotationByCamera) =>
                {
                    foreach (var d in transformRotationByCamera.datas)
                        update(d.transform, d.isUI, cameraData.targetRotation);
                });
        }

        void update(Transform transform, bool isUI, in Quaternion targetRotation)
        {
            if (transform != null && transform.gameObject.activeSelf)
            {
                if (isUI)
                    transform.rotation = targetRotation * offset;
                else
                    transform.rotation = targetRotation;
            }
        }
    }
}
