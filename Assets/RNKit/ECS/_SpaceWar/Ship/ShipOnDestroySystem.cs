using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipOnDestroyServerSystem : JobComponentSystem
    {
        EndCommandBufferSystem endBarrier;
        protected override void OnCreate()
        {
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Ship), typeof(OnDestroyMessage))]
        struct ShipDestroyMessageJob : IJobForEachWithEntity<ActorOwner, ShipWeaponArray, ShipSlotList>
        {
            public ComponentType OnDestroyMessage;

            public ComponentType Weapon_OnShipDestroyMessage;
            public ComponentType Player_OnShipDestroyMessage;

            public EntityCommandBuffer.Concurrent commandBuffer;
            public EntityCommandBuffer.Concurrent endCommandBuffer;

            [ReadOnly] public ComponentDataFromEntity<Player> playerFromEntity;

            public void Execute(Entity shipEntity, int index, [ReadOnly]ref ActorOwner actorOwner, [ReadOnly]ref ShipWeaponArray shipWeaponInstalledArray, [ReadOnly]ref ShipSlotList slotList)
            {
                //
                for (var i = 0; i < slotList.Length; ++i)
                {
                    if (slotList[i] != Entity.Null)
                    {
                        commandBuffer.AddComponent(index, slotList[i], OnDestroyMessage);//必须马上添加OnDestroyMessage 因为Ship节点和子节点都已经删除了
                    }
                }




                //
                if (shipWeaponInstalledArray.shieldEntity != Entity.Null)
                {
                    commandBuffer.AddComponent(index, shipWeaponInstalledArray.shieldEntity, OnDestroyMessage);//必须马上添加OnDestroyMessage 因为Ship节点和子节点都已经删除了
                }



                //
                for (var i = 0; i < ShipWeaponArray.Length; ++i)
                {
                    var weaponEntity = shipWeaponInstalledArray[i];
                    if (weaponEntity == Entity.Null)
                        continue;

                    //
                    commandBuffer.AddComponent(index, weaponEntity, Weapon_OnShipDestroyMessage);
                    endCommandBuffer.RemoveComponent(index, weaponEntity, Weapon_OnShipDestroyMessage);
                }



                //
                if (actorOwner.playerEntity != default && playerFromEntity.Exists(actorOwner.playerEntity))//可能是先断线后的角色删除
                {
                    commandBuffer.AddComponent(index, actorOwner.playerEntity, Player_OnShipDestroyMessage);
                    endCommandBuffer.RemoveComponent(index, actorOwner.playerEntity, Player_OnShipDestroyMessage);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            using (var commandBuffer = new EntityCommandBuffer(Allocator.TempJob))
            {
                inputDeps = new ShipDestroyMessageJob
                {
                    OnDestroyMessage = typeof(OnDestroyMessage),

                    Weapon_OnShipDestroyMessage = typeof(Weapon_OnShipDestroyMessage),
                    Player_OnShipDestroyMessage = typeof(Player_OnShipDestroyMessage),

                    commandBuffer = commandBuffer.ToConcurrent(),
                    endCommandBuffer = endBarrier.CreateCommandBuffer().ToConcurrent(),

                    playerFromEntity = GetComponentDataFromEntity<Player>(true),
                }
                .Schedule(this, inputDeps);

                inputDeps.Complete();
                commandBuffer.Playback(EntityManager);
            }

            return inputDeps;
        }
    }
}
