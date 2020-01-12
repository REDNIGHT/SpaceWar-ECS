using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public interface IWeaponItemCount
    {
        void OnPlay(short itemCount);
    }

    [DisableAutoCreation]
    public class WeaponItemCountClientSystem : ComponentSystem
    {
        ActorSyncCreateClientSystem actorClientSystem;

        protected override void OnCreate()
        {
            actorClientSystem = World.GetExistingSystem<ActorSyncCreateClientSystem>();
        }

        protected override void OnUpdate()
        {
            using (var actorDatas1List = Entities
                .WithAllReadOnly<ActorDatas1Buffer>()
                .WithNone<NetworkDisconnectedMessage>()
                .AllBufferElementToList<ActorDatas1Buffer>(Allocator.Temp))
            {
                for (int i = 0; i < actorDatas1List.Length; ++i)
                {
                    var actorDatas1 = actorDatas1List[i];
                    if (actorDatas1.shortValueB >= 0 && actorDatas1.synDataType == (sbyte)ActorSynDataTypes.WeaponAttribute)
                    {
                        if (actorClientSystem.actorEntityFromActorId.TryGetValue(actorDatas1.actorId, out Entity actorEntity))
                        {
                            var transform = EntityManager.GetComponentObject<Transform>(actorEntity);
                            var itemCountT = transform.GetChild(WeaponSpawner.WeaponItemCount_TransformIndex);
                            var fx = itemCountT.GetComponent<IWeaponItemCount>();
                            if (fx != null)
                            {
                                fx.OnPlay(actorDatas1.shortValueB);
                            }
                            else
                            {
                                var textMeshT = GameObject.Instantiate(itemCountT, transform);
                                textMeshT.gameObject.SetActive(true);
                                foreach (var textMesh in textMeshT.GetComponentsInChildren<TextMesh>(true))
                                    textMesh.text = actorDatas1.shortValueB.ToString();

                                //textMeshT.forward = Camera.main.transform.forward;

                                var e = EntityManager.CreateEntity();
#if UNITY_EDITOR
                                EntityManager.SetName(e, $"WeaponItemCount:{e.Index}");
#endif
                                EntityManager.AddComponent<AttributeModifyFx>(e);
                                EntityManager.AddComponentData(e, new Actor { actorType = (short)ActorTypes.AttributeModifyFx });
                                EntityManager.AddComponentData(e, new ActorLifetime { value = 1.5f });
                                EntityManager.AddComponentObject(e, textMeshT);
                                EntityManager.AddComponentObject(e, textMeshT.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }
}
