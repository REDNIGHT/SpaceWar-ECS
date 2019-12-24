using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace RN.Network
{
    /// <summary>
    /// 初始化流程
    /// Translation -> Transform_In -> Transform -> Transform_Out -> Translation
    /// </summary>

    public struct Transform_Out : IComponentData
    {
    }
    public struct Transform_In : IComponentData
    {
    }
    [AutoClear]
    public struct Transform_In_OnCreate : IComponentData
    {
    }

    /*public struct TransformLerp_In : IComponentData
    {
        public float lerpT;
        public float rotationLerpT;
    }*/
    public struct TransformSmooth_In : IComponentData
    {
        public float smoothTime;
        //public float maxSpeed;
        public Vector3 currentVelocity;

        //public float ignoreDistance;

        //
        public float rotationLerpT;
    }

    /// <summary>
    /// 下面6个都是只能从Transform里取出数据 暂时没有写入功能
    /// </summary>
    public struct ParentTransform_Out : IComponentData
    {
        public float3 value;
    }

    public struct ParentRotation_Out : IComponentData
    {
        public quaternion value;
    }

    public struct ChildTransform_Out : IComponentData
    {
        public sbyte childIndex0;
        public sbyte childIndex1;
        public float3 value;
    }

    public struct ChildRotation_Out : IComponentData
    {
        public sbyte childIndex0;
        public sbyte childIndex1;
        public quaternion value;
    }

    [System.Obsolete("//todo...")]
    public struct TargetTransform_Out : IComponentData
    {
        public Entity entity;
        public float3 value;
    }
    [System.Obsolete("//todo...")]
    public struct TargetRotation_Out : IComponentData
    {
        public Entity entity;
        public quaternion value;
    }



    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class TransformOutSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            //
            Entities
                .WithAll<Translation>().WithAllReadOnly<Transform_Out, Transform>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Transform transform, ref Translation translation) =>
                {
                    translation.Value = transform.position;
                });

            Entities
                .WithAll<Rotation>().WithAllReadOnly<Transform_Out, Transform>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Transform transform, ref Rotation rotation) =>
                {
                    rotation.Value = transform.rotation;
                });


            //
            Entities
                .WithAll<ParentTransform_Out>().WithAllReadOnly<Transform>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Transform transform, ref ParentTransform_Out parentTranslation) =>
                {
                    parentTranslation.value = transform.parent.position;
                });

            Entities
                .WithAll<ParentRotation_Out>().WithAllReadOnly<Transform>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Transform transform, ref ParentRotation_Out parentRotation) =>
                {
                    parentRotation.value = transform.parent.rotation;
                });

            Entities
                .WithAll<ChildTransform_Out>().WithAllReadOnly<Transform>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Transform transform, ref ChildTransform_Out childTransform) =>
                {
                    var childT = transform.GetChild(childTransform.childIndex0);
                    if (childTransform.childIndex1 >= 0)
                        childT = childT.GetChild(childTransform.childIndex0);

                    childTransform.value = childT.position;
                });

            Entities
                .WithAll<ChildRotation_Out>().WithAllReadOnly<Transform>()
                .WithNone<OnDestroyMessage>()
                .ForEach((Transform transform, ref ChildRotation_Out childRotation) =>
                {
                    var childT = transform.GetChild(childRotation.childIndex0);
                    if (childRotation.childIndex1 >= 0)
                        childT = childT.GetChild(childRotation.childIndex0);

                    childRotation.value = childT.rotation;
                });
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class TransformInSystem : ComponentSystem
    {
        float3 offset;
        protected void OnInit(Transform root)
        {
            if (ServerBootstrap.world != World)
            {
                offset = root.position;
            }
        }

        protected override void OnUpdate()
        {
            //
            Entities
                .WithAll<Transform>().WithAllReadOnly<Translation>()
                .WithAnyReadOnly<Transform_In, Transform_In_OnCreate>()
                .ForEach((Transform transform, ref Translation translation) =>
                {
                    transform.position = offset + translation.Value;
                });

            Entities
                .WithAll<Transform>().WithAllReadOnly<Rotation>()
                .WithAnyReadOnly<Transform_In, Transform_In_OnCreate>()
                .ForEach((Transform transform, ref Rotation rotation) =>
                {
                    transform.rotation = rotation.Value;
                });


            //
            Entities
                .WithAll<Transform>().WithAllReadOnly<Translation, TransformSmooth_In, OnCreateMessage>()
                .ForEach((Transform transform, ref Translation translation) =>
                {
                    transform.position = offset + translation.Value;
                });

            Entities
                .WithAll<Transform>().WithAllReadOnly<Rotation, TransformSmooth_In, OnCreateMessage>()
                .ForEach((Transform transform, ref Rotation rotation) =>
                {
                    transform.rotation = rotation.Value;
                });

            //
            Entities
                .WithAll<Transform, TransformSmooth_In>().WithAllReadOnly<Translation>()
                .WithNone<OnCreateMessage/*创建时 在最上面两段代码执行 所有这里需要排除*/, OnDestroyMessage>()
                .ForEach((Transform transform, ref Translation translation, ref TransformSmooth_In transformSmooth_In) =>
                {
                    var targetPosition = offset + translation.Value;
                    var smoothTime = transformSmooth_In.smoothTime;

                    transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref transformSmooth_In.currentVelocity, smoothTime, Mathf.Infinity, Time.fixedDeltaTime);
                });


            Entities
                .WithAll<Transform>().WithAllReadOnly<TransformSmooth_In, Rotation>()
                .WithNone<OnCreateMessage/*创建时 在最上面两段代码执行 所有这里需要排除*/, OnDestroyMessage>()
                .ForEach((Transform transform, ref Rotation rotation, ref TransformSmooth_In transformSmooth_In) =>
                {
                    transform.rotation = Quaternion.LerpUnclamped(transform.rotation, rotation.Value, transformSmooth_In.rotationLerpT * Time.fixedDeltaTime);
                    //transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation.Value, transformSmooth_In.rotationLerpT * Time.fixedDeltaTime * rotationLerpScale);
                });
        }

        const float rotationLerpScale = 20f;
    }
}
