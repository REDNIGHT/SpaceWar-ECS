using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public interface IWeaponItemCountFx
    {
        void OnPlay(short itemCount);
    }

    [DisableAutoCreation]
    public class WeaponItemCountFxClientSystem : ComponentSystem
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
                            var itemCountFxT = transform.GetChild(WeaponSpawner.WeaponItemCountFx_TransformIndex);
                            var fx = itemCountFxT.GetComponent<IWeaponItemCountFx>();
                            if (fx != null)
                            {
                                fx.OnPlay(actorDatas1.shortValueB);
                            }
                            else
                            {
                                var textMesh = GameObject.Instantiate(itemCountFxT.GetComponent<TextMesh>(), transform);
                                textMesh.gameObject.SetActive(true);
                                textMesh.text = actorDatas1.shortValueB.ToString();

                                //itemCountFxT.forward = Camera.main.transform.forward;

                                var e = EntityManager.CreateEntity();
#if UNITY_EDITOR
                                EntityManager.SetName(e, $"WeaponItemCount:{e.Index}");
#endif
                                EntityManager.AddComponent<AttributeModifyFx>(e);
                                EntityManager.AddComponentData(e, new Actor { actorType = (short)ActorTypes.AttributeModifyCountFx });
                                EntityManager.AddComponentData(e, new ActorLifetime { value = 1.5f });
                                EntityManager.AddComponentObject(e, textMesh);
                                EntityManager.AddComponentObject(e, textMesh.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }
}
