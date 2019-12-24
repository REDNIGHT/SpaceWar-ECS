using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class WeaponFireCreateServerSystem : ComponentSystem
    {
        IActorSpawnerMap actorSpawnerMap;

        protected void OnInit(Transform root)
        {
            actorSpawnerMap = root.GetComponentInChildren<IActorSpawnerMap>();
            Debug.Assert(actorSpawnerMap != null, $"actorSpawnerMap != null  root={root}", root);
        }


        void fireCreate(Transform firePoint, in FireCreateData fireCreateData)
        {
            //炮塔的fireCreateData.actorOwner.playerEntity是空的
            //if (EntityManager.Exists(fireCreateData.actorOwner.playerEntity) == false)
            //    Debug.LogError($"EntityManager.Exists({EntityManager.GetName(fireCreateData.actorOwner.playerEntity)}) == false");//test...  玩家可能下线

            if (EntityManager.Exists(fireCreateData.shipEntity) == false)
            {
                Debug.LogError($"EntityManager.Exists(fireCreateData.shipEntity) == false");//飞船可能死亡
                return;
            }

            //
            var actorEntity = actorSpawnerMap.CreateInServer((short)fireCreateData.fireActorType, fireCreateData.actorOwner);

            EntityManager.SetComponentData(actorEntity, new Translation { Value = firePoint.position });
            EntityManager.SetComponentData(actorEntity, new Rotation { Value = firePoint.rotation });

            if (EntityManager.HasComponent<WeaponCreator>(actorEntity))
            {
                EntityManager.SetComponentData(actorEntity, new WeaponCreator { entity = fireCreateData.weaponEntity });
            }
            if (EntityManager.HasComponent<ActorCreator>(actorEntity))
            {
                EntityManager.SetComponentData(actorEntity, new ActorCreator { entity = fireCreateData.shipEntity });
            }
            if (EntityManager.HasComponent<TracePoint>(actorEntity))
            {
                EntityManager.SetComponentData(actorEntity, new TracePoint { value = fireCreateData.firePosition });
            }

            if (EntityManager.HasComponent<ActorAttribute3Offset<_HP>>(actorEntity))
            {
                var hpOffset = EntityManager.GetComponentData<ActorAttribute3Offset<_HP>>(actorEntity);
                hpOffset.scale = fireCreateData.attributeOffsetScale;
                EntityManager.SetComponentData(actorEntity, hpOffset);
            }

            else if (EntityManager.HasComponent<RigidbodyVelocity>(actorEntity))
            {
                var weaponVelocity = EntityManager.GetComponentData<RigidbodyVelocity>(fireCreateData.weaponEntity);

                var rigidbodyVelocity = EntityManager.GetComponentData<RigidbodyVelocity>(actorEntity);
                rigidbodyVelocity.linear += weaponVelocity.linear;
                EntityManager.SetComponentData(actorEntity, rigidbodyVelocity);
            }
        }

        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<FireCreateData>()
                .ForEach((Entity entity, ref FireCreateData fireCreateData) =>
                {
                    var weaponFirePoint = EntityManager.GetComponentObject<WeaponFirePoint>(fireCreateData.weaponEntity);

                    //weaponFirePoint.rotation = EntityManager.GetComponentData<Rotation>(fireCreateData.weaponEntity).Value;

                    foreach (Transform t in weaponFirePoint.firePoints)
                    {
                        fireCreate(t, fireCreateData);
                    }

                    PostUpdateCommands.DestroyEntity(entity);
                });
        }
    }
}
