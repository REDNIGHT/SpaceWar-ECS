using Unity.Entities;

namespace RN.Network.SpaceWar
{
    public class WeaponFirePrepareFxEntity : System.Attribute { }

    [WeaponFirePrepareFxEntity]
    public struct WeaponFirePrepareFx : IComponentData
    {
        //public WeaponType weaponType;
        public byte level;
    }

    public interface IWeaponFirePrepareFx
    {
        void OnPlayFx(Entity weaponFirePrepareEntity, in WeaponCreator weaponCreator, IActorSpawnerMap actorSpawnerMap, EntityManager entityManager);
    }
}
