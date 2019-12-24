using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RN.Network
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerCreateActorOnGameStartServerSystem : ComponentSystem
    {
        IActorSpawnerMap actorSpawnerMap;
        EndCommandBufferSystem endBarrier;


        Transform[] startPoints;
        protected void OnInit(Transform root)
        {
            //
            endBarrier = World.GetExistingSystem<EndCommandBufferSystem>();

            //
            actorSpawnerMap = root.GetComponentInChildren<IActorSpawnerMap>();
            Debug.Assert(actorSpawnerMap != null, $"actorSpawnerMap != null  root={root}", root);


            //
            var startPointsT = root.Find(nameof(startPoints));
            if (startPointsT == null)
            {
                Debug.LogError("startPointsT == null");
                return;
            }

            startPoints = new Transform[startPointsT.childCount];
            var i = 0;
            foreach (Transform c in startPointsT)
            {
                startPoints[i] = c;
                ++i;
            }
        }

        public enum SelectType
        {
            Near,
            Random,
            Next,
        }
        public SelectType selectType = SelectType.Near;
        int nextIndex = 0;
        Transform getStartPointT(float3 position)
        {
            if (selectType == SelectType.Near)
            {
                System.Array.Sort(startPoints, (x, y) => (int)math.distancesq(x.position, position) - (int)math.distancesq(y.position, position));
                return startPoints[0];
            }

            if (selectType == SelectType.Next)
            {
                if (nextIndex >= startPoints.Length)
                    nextIndex = 0;
                return startPoints[nextIndex++];
            }


            return startPoints[Random.Range(0, startPoints.Length)];
        }

        float3 getStartPoint(Transform t)
        {
            var b = t.GetComponent<Bounding>();
            if (b != null)
                return b.randomPosition;

            return t.position;
        }


        protected override void OnUpdate()
        {
            if (startPoints.Length == 0)
                return;

#if false
            var createQuery = Entities
                .WithAllReadOnly<Player, Observer, PlayerGameStartMessage, PlayerActorType>()
                .WithNone<NetworkDisconnectedMessage>()
                .ToEntityQuery();
            if (createQuery.IsEmptyIgnoreFilter == false)
            {
                using (var playerEntitys = createQuery.ToEntityArray(Allocator.TempJob))
                using (var players = createQuery.ToComponentDataArray<Player>(Allocator.TempJob))
                using (var observers = createQuery.ToComponentDataArray<Observer>(Allocator.TempJob))
                using (var actorTypes = createQuery.ToComponentDataArray<PlayerActorType>(Allocator.TempJob))
                {
                    for (var i = 0; i < playerEntitys.Length; ++i)
                    {
                        Debug.LogWarning("createQuery");

                        Entity playerEntity = playerEntitys[i];
                        Player player = players[i];
                        Observer observer = observers[i];
                        PlayerActorType actorType = actorTypes[i];

                        try
                        {
                            var actorOwner = new ActorOwner { playerEntity = playerEntity, playerId = player.id };
                            var actorEntity = actorServerSystem.CreateInServer(actorOwner, actorType.value);

                            var t = findNearStartPointT(observer.position);

                            EntityManager.SetComponentData(actorEntity, new Translation { Value = getStartPoint(t) });
                            EntityManager.SetComponentData(actorEntity, new Rotation { Value = t.rotation });
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log("System.Exception e");
                            Debug.LogException(e);
                            PostUpdateCommands.AddComponent<NetworkDisconnectedMessage>(playerEntity);
                        }
                    }
                }
            }
#else
            Entities
                .WithAllReadOnly<Player, ObserverPosition, PlayerGameStartNetMessage, PlayerActorType>()
                .WithNone<NetworkDisconnectedMessage>()
                .ForEach((Entity playerEntity, ref Player player, ref ObserverPosition observer, ref PlayerActorType actorType) =>
                {
                    var t = getStartPointT(observer.value);
                    var pos = getStartPoint(t);
                    var rot = t.rotation;

                    var actorOwner = new ActorOwner { playerEntity = playerEntity, playerId = player.id };
                    var actorEntity = actorSpawnerMap.CreateInServer(actorType.value, actorOwner);

                    EntityManager.SetComponentData(actorEntity, new Translation { Value = pos });
                    EntityManager.SetComponentData(actorEntity, new Rotation { Value = rot });

                });
#endif
        }
    }
}