using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [ClientAutoClear]
    public struct WeaponInstalledStateNetMessage : IBufferElementData
    {
        public int weaponId;
        public bool install;
        public bool uninstall => !install;

        public int shipId;
        public byte slotIndex;
    }

    public interface IWeaponInstalledFx
    {
        void OnPlayInstalledFx();
        void OnPlayUninstalledFx();
    }

    public struct WeaponInstalledStateSerialize : NetworkSerializeUnsafe.ISerializer//在ActorSpawner.OnActorSerialize里调用
    {
        public SC_CS sc_cs => SC_CS.server2client;
        public short Type => (short)NetworkSerializeType.WeaponInstalledState;

        public int weaponId;
        public bool install;

        public int shipId;
        public byte slotIndex;

        public void Execute(int index, Entity entity, EntityCommandBuffer.Concurrent commandBuffer)
        {
            NetworkSerializeUnsafe.AutoAddBufferMessage(index, entity
                , new WeaponInstalledStateNetMessage { weaponId = weaponId, install = install, shipId = shipId, slotIndex = slotIndex }, commandBuffer);
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class WeaponSyncInstalledStateServerSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponInstallMessage))]
        struct WeaponOnMessageJobA : IJobForEachWithEntity<WeaponInstalledState>
        {
            public NativeQueue<WeaponInstalledStateSerialize>.ParallelWriter outs;
            public void Execute(Entity entity, int index, [ReadOnly]ref WeaponInstalledState weaponInstalledState)
            {
                outs.Enqueue(new WeaponInstalledStateSerialize
                {
                    weaponId = entity.Index,
                    install = true,
                    shipId = weaponInstalledState.shipEntity.Index,
                    slotIndex = weaponInstalledState.slot.index
                });
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OnWeaponUninstallMessage))]
        struct WeaponOnMessageJobB : IJobForEachWithEntity<WeaponInstalledState>
        {
            public NativeQueue<WeaponInstalledStateSerialize>.ParallelWriter outs;
            public void Execute(Entity entity, int index, [ReadOnly]ref WeaponInstalledState weaponInstalledState)
            {
                outs.Enqueue(new WeaponInstalledStateSerialize { weaponId = entity.Index, install = false });
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct SyncWeaponInstalledStateJob : IJobForEach_B<NetworkReliableOutBuffer>
        {
            [ReadOnly] public NativeArray<WeaponInstalledStateSerialize> ins;
            public void Execute(DynamicBuffer<NetworkReliableOutBuffer> outBuffer)
            {
                for (var i = 0; i < ins.Length; ++i)
                {
                    var s = ins[i];
                    s._DoSerialize(outBuffer);
                }
            }
        }


        protected override void OnDestroy()
        {
            outAs.Dispose();
            outBs.Dispose();
            ins.Dispose();
        }

        NativeQueue<WeaponInstalledStateSerialize> outAs = new NativeQueue<WeaponInstalledStateSerialize>(Allocator.Persistent);
        NativeQueue<WeaponInstalledStateSerialize> outBs = new NativeQueue<WeaponInstalledStateSerialize>(Allocator.Persistent);
        NativeList<WeaponInstalledStateSerialize> ins = new NativeList<WeaponInstalledStateSerialize>(Allocator.Persistent);
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            outAs.Clear();
            outBs.Clear();
            ins.Clear();

            var inputDepsA = new WeaponOnMessageJobA
            {
                outs = outAs.AsParallelWriter(),
            }
            .Schedule(this, inputDeps);

            var inputDepsB = new WeaponOnMessageJobB
            {
                outs = outBs.AsParallelWriter(),
            }
            .Schedule(this, inputDeps);

            inputDeps = NativeQueueEx.ToListJob(outAs, outBs, ref ins, JobHandle.CombineDependencies(inputDepsA, inputDepsB));

            inputDeps = new SyncWeaponInstalledStateJob
            {
                ins = ins.AsDeferredJobArray(),
            }
            .Schedule(this, inputDeps);
            return inputDeps;
        }
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class WeaponSyncInstalledStateClientSystem : ComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        ActorSyncCreateClientSystem actorSyncCreateClientSystem;

        EntityQuery networkConnectedMessageQuery;

        protected override void OnCreate()
        {
            actorSyncCreateClientSystem = World.GetExistingSystem<ActorSyncCreateClientSystem>();
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();

            networkConnectedMessageQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<NetworkConnection>(), ComponentType.ReadOnly<NetworkConnectedMessage>() },
                None = new ComponentType[] { ComponentType.ReadOnly<NetworkDisconnectedMessage>() },
            });
        }

        void onNetworkConnectedMessage()
        {
            if (networkConnectedMessageQuery.IsEmptyIgnoreFilter == false)
            {
                EntityManager.AddComponent<WeaponInstalledStateNetMessage>(networkConnectedMessageQuery);
            }
        }

        protected override void OnUpdate()
        {
            //
            onNetworkConnectedMessage();


            //
            var endCommandBuffer = endBarrier.CreateCommandBuffer();

            using (var weaponInstalledStateNetMessages = Entities
                .WithAllReadOnly<NetworkConnection, WeaponInstalledStateNetMessage>()
                .WithNone<NetworkDisconnectedMessage>()
                .AllBufferElementToList<WeaponInstalledStateNetMessage>(Allocator.Temp))
            {
                for (int i = 0; i < weaponInstalledStateNetMessages.Length; ++i)
                {
                    var weaponInstalledStateNetMessage = weaponInstalledStateNetMessages[i];

                    if (actorSyncCreateClientSystem.actorEntityFromActorId.TryGetValue(weaponInstalledStateNetMessage.weaponId, out Entity weaponEntity) == false)
                    {
                        Debug.LogError($"{World.Name} => actorEntityFromActorId.TryGetValue(weaponInstalledStateNetMessage.weaponId={weaponInstalledStateNetMessage.weaponId}, out Entity weaponEntity) == false");
                        continue;
                    }

                    //
                    var weaponT = EntityManager.GetComponentObject<Transform>(weaponEntity);
                    var weapon = EntityManager.GetComponentData<Weapon>(weaponEntity);

                    if (weaponInstalledStateNetMessage.install)
                    {
                        if (actorSyncCreateClientSystem.actorEntityFromActorId.TryGetValue(weaponInstalledStateNetMessage.shipId, out Entity shipEntity) == false)
                        {
                            Debug.LogError($"{World.Name} => actorEntityFromActorId.TryGetValue(weaponInstalledStateNetMessage.shipId={weaponInstalledStateNetMessage.shipId}, out Entity shipEntity) == false");
                            continue;
                        }

                        EntityManager.AddComponentData(weaponEntity, new WeaponInstalledState { slot = new Slot { shipEntity = shipEntity, index = weaponInstalledStateNetMessage.slotIndex } });
                        EntityManager.AddComponent<OnWeaponInstallMessage>(weaponEntity);
                        endCommandBuffer.RemoveComponent<OnWeaponInstallMessage>(weaponEntity);


                        var fx = weaponT.GetComponent<IWeaponInstalledFx>();
                        if (fx != null)
                        {
                            fx.OnPlayInstalledFx();
                        }
                    }
                    else
                    {
                        endCommandBuffer.RemoveComponent<WeaponInstalledState>(weaponEntity);

                        EntityManager.AddComponent<OnWeaponUninstallMessage>(weaponEntity);
                        endCommandBuffer.RemoveComponent<OnWeaponUninstallMessage>(weaponEntity);


                        var fx = weaponT.GetComponent<IWeaponInstalledFx>();
                        if (fx != null)
                        {
                            fx.OnPlayUninstalledFx();
                        }
                    }


                    //Debug.Log($"weaponInstalledStateNetMessage  weaponId={weaponInstalledStateNetMessage.weaponId}  weapon={weapon.type}  slotIndex={weaponInstalledStateNetMessage.slotIndex}  shipId={weaponInstalledStateNetMessage.shipId}");
                }
            }
        }
    }
}
