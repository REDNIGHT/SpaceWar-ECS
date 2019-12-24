using Unity.Entities;
using UnityEngine;

namespace RN.Network
{
    public class EntityBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public Entity entity { get; protected set; }
        public World world { get; protected set; }
        protected EntityManager entityManager => world?.EntityManager;

        public void Reset()
        {
            entity = Entity.Null;
            world = null;
        }

        public static void _initComponent(Entity entity, EntityManager entityManager)
        {
            var entityBehaviour = entityManager.GetComponentObject<GameObject>(entity).GetComponent<EntityBehaviour<T>>();
            entityBehaviour.entity = entity;
            entityBehaviour.world = entityManager.World;

            Debug.Assert(entityBehaviour.gameObject == entityManager.GetComponentObject<GameObject>(entity),
                $"{entityBehaviour.gameObject} == {entityManager.GetComponentObject<GameObject>(entity)}",
                entityBehaviour.gameObject);
        }

        public static void _initComponent(Component component, Entity entity, EntityManager entityManager)
        {
            Debug.Assert(component != null, "component != null");
            var entityBehaviour = component.GetComponent<EntityBehaviour<T>>();
            entityBehaviour.entity = entity;
            entityBehaviour.world = entityManager.World;
        }

        public static void _removeComponent(Entity entity, EntityManager entityManager)
        {
            var go = entityManager.GetComponentObject<GameObject>(entity);
            var entityBehaviour = go.GetComponent<EntityBehaviour<T>>();
            Debug.Assert(entityBehaviour != null, $"entityBehaviour != null  go={go}", go);

            entityBehaviour.enabled = false;
            entityBehaviour.destroy();
        }

        public static void _removeComponent(Component component, Entity entity, EntityManager entityManager)
        {
            var entityBehaviour = component.GetComponent<EntityBehaviour<T>>();
            Debug.Assert(entityBehaviour != null, $"entityBehaviour != null  component={component}", component);

            entityBehaviour.enabled = false;
            entityBehaviour.destroy();
        }

        public static bool getEntity(Component component, out Entity outEntity, World world)
        {
            outEntity = default;

            var entityBehaviour = component.GetComponent<EntityBehaviour<T>>();
            if (entityBehaviour == null)
                return false;

            if (entityBehaviour.world == null)
                return false;

            if (entityBehaviour.world != world)
            {
                Debug.LogError($"entityBehaviour.world != world  component={component}  entityBehaviour={entityBehaviour}  entityBehaviour.world={entityBehaviour?.world}  world={world}", component);
                return false;
            }

            Debug.Assert(entityBehaviour.entity != default, "entityBehaviour.entity != default", component);

            //
            outEntity = entityBehaviour.entity;
            return true;
        }
    }

    public class EntityBehaviour : EntityBehaviour<EntityBehaviour>
    {
    }
}
