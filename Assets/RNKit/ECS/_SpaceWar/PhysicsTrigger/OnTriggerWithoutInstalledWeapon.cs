using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class OnTriggerWithoutInstalledWeapon : OnTrigger
    {
        protected override void onTrigger(Collider other, TriggerResultState state)
        {
            if (entityManager == null) return;
            if (entityManager.Exists(entity) == false) return;


            if (EntityBehaviour.getEntity(other.attachedRigidbody, out Entity transformEntity, entityManager.World))
            {
                if (entityManager.HasComponent<WeaponInstalledState>(transformEntity))
                {
                    return;
                }

                entityManager.GetBuffer<PhysicsTriggerResults>(entity).Add(new PhysicsTriggerResults { state = state, entity = transformEntity });
            }
            else
            {
                var entityBehaviour = other.GetComponent<EntityBehaviour>();
                if (entityBehaviour == null) return;

                //别的world里的Collider 进入到这Trigger
                if (world != entityBehaviour.world) return;

#if UNITY_EDITOR
                var entityName = entityManager.GetName(entityBehaviour.entity);
#else
                var entityName = "";
#endif
                Debug.LogError($"EntityBehaviour.getEntity(other={other}, ...) == false" +
                    $"\nentityBehaviour.entity={entityBehaviour.entity}  {entityName}  Exists={entityManager.Exists(entityBehaviour.entity)}" +
                    $"\nentityBehaviour.world={entityBehaviour.world}  entityManager.World={entityManager.World}" +
                    $"\nthis={this}"
                    , other);
            }
        }
    }
}
