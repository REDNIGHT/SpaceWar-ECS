using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RN.Network
{
    public partial class OnCollision : MonoBehaviour
    {
        public enum CollisionType
        {
            Enter,
            Stay,
            Exit,
        }

        public bool enterEnable;
        public bool stayEnable;
        public bool exitEnable;

        List<(CollisionType type, Collision collision)> collisions = new List<(CollisionType type, Collision collision)>();
        public IReadOnlyList<(CollisionType type, Collision collision)> buffer => collisions;

        public void Clear()
        {
            collisions.Clear();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (enterEnable)
                collisions.Add((CollisionType.Enter, collision));
        }
        private void OnCollisionStay(Collision collision)
        {
            if (stayEnable)
                collisions.Add((CollisionType.Enter, collision));
        }
        private void OnCollisionExit(Collision collision)
        {
            if (exitEnable)
                collisions.Add((CollisionType.Enter, collision));
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    class OnCollisionClearSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach((OnCollision onCollision) =>
                {
                    onCollision.Clear();
                });
        }
    }

}
