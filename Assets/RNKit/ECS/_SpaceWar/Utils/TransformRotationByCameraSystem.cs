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
                    foreach (var t in transformRotationByCamera.transforms)
                    {
                        Debug.Assert(t != null, "t != null", transformRotationByCamera);

                        if (t.gameObject.activeSelf)
                        {
                            update(t, t is RectTransform, cameraData.targetRotation);
                        }
                    }
                });
        }

        void update(Transform transform, bool isUI, in Quaternion targetRotation)
        {
            if (isUI)
                transform.rotation = targetRotation * offset;
            else
                transform.rotation = targetRotation;
        }
    }
}
