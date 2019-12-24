using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShieldOnUpdateClientSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<Shield, WeaponCreator, ShieldRoot>()
                .ForEach((ref WeaponCreator weaponCreator, ShieldRoot shieldRoot) =>
                {
                    var weaponT = EntityManager.GetComponentObject<Transform>(weaponCreator.entity);
                    shieldRoot.rotation = weaponT.rotation;
                });
        }
    }
}
