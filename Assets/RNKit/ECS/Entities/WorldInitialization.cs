using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;

namespace Unity.Entities
{
    public interface IWorldBootstrap
    {
        IEnumerable
            <(
                (Type type, Type subType) order,
                ComponentSystemGroup group,
                EntityCommandBufferSystem beginCommandSystem,
                IEnumerable<ComponentSystemBase> systems,
                EntityCommandBufferSystem endCommandSystem
            )> createSystemMaps();

        void InitializeWorld(World world);
        void InitializeInScene(World world);
    }

    //
    public abstract class WorldBootstrap : IWorldBootstrap
    {
        public abstract IEnumerable
            <(
                (Type type, Type subType) order,
                ComponentSystemGroup group,
                EntityCommandBufferSystem beginCommandSystem,
                IEnumerable<ComponentSystemBase> systems,
                EntityCommandBufferSystem endCommandSystem
            )> createSystemMaps();

        public abstract void InitializeWorld(World world);

        public Transform root;
        public virtual void InitializeInScene(World world)
        {
            root.BroadcastMessage("OnWorldInitialized", world);

            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
            foreach (var system in world.Systems)
            {
                var m = system.GetType().GetMethod("OnInit", bindingAttr);
                if (m != null)
                    m.Invoke(system, new object[] { root });
            }
        }
    }

    public static class WorldInitialization
    {
        public static void DomainUnloadShutdown()
        {
            World.DisposeAllWorlds();

            WordStorage.Instance.Dispose();
            WordStorage.Instance = null;
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(null);
        }

        public static void RegisterDomainUnload()
        {
            PlayerLoopManager.RegisterDomainUnload(DomainUnloadShutdown, 10000);
        }

        public static void Initialize_DefaultWorld()
        {
            var worldName = "Default World";
            DefaultWorldInitialization.Initialize(worldName, false);

            //Initialize(World.Active, worldBootstrap);
        }

        public static void Initialize(string worldName, IWorldBootstrap worldBootstrap)
        {
            Initialize(new World(worldName), worldBootstrap);
        }

        public static void Initialize(World world, IWorldBootstrap worldBootstrap)
        {
            worldBootstrap.InitializeWorld(world);


            var systemMaps = worldBootstrap.createSystemMaps().ToArray();
            foreach (var systemMap in systemMaps)
            {
                var group = world.AddSystem(systemMap.group);

                if (systemMap.beginCommandSystem != null)
                {
                    world.AddSystem(systemMap.beginCommandSystem);
                }
                if (systemMap.endCommandSystem != null)
                {
                    world.AddSystem(systemMap.endCommandSystem);
                }
            }

            foreach (var systemMap in systemMaps)
            {
                foreach (var system in systemMap.systems)
                {
                    world.AddSystem(system);
                }
            }

            foreach (var systemMap in systemMaps)
            {
                var group = systemMap.group;

                if (systemMap.beginCommandSystem != null)
                {
                    group.AddSystemToUpdateList(systemMap.beginCommandSystem);
                }

                foreach (var system in systemMap.systems)
                {
                    group.AddSystemToUpdateList(system);
                }

                if (systemMap.endCommandSystem != null)
                {
                    group.AddSystemToUpdateList(systemMap.endCommandSystem);
                }
            }


            //
            worldBootstrap.InitializeInScene(world);

            updatePlayerLoop(systemMaps.Select(x => (x.order, x.group)));
        }




        //
        static void updatePlayerLoop(IEnumerable<((Type type, Type subType) order, ComponentSystemGroup group)> groupMaps)
        {
            var playerLoop = ScriptBehaviourUpdateOrder.CurrentPlayerLoop;
            if (playerLoop.subSystemList == null)
            {
                playerLoop = PlayerLoop.GetDefaultPlayerLoop();
            }


            foreach (var groupMap in groupMaps)
            {
                for (var i = 0; i < playerLoop.subSystemList.Length; ++i)
                {
                    if (playerLoop.subSystemList[i].type == groupMap.order.type)
                    {
                        var fixedUpdateLoop = playerLoop.subSystemList[i];
                        {
                            var newSubsystemList = new List<PlayerLoopSystem>();
                            newSubsystemList.AddRange(fixedUpdateLoop.subSystemList);

                            insertSubsystemList(newSubsystemList,
                                groupMap.order.subType, groupMap.group);

                            fixedUpdateLoop.subSystemList = newSubsystemList.ToArray();
                        }
                        playerLoop.subSystemList[i] = fixedUpdateLoop;
                    }
                }
            }

            //printplayerLoop(playerLoop, 0);
            ScriptBehaviourUpdateOrder.SetPlayerLoop(playerLoop);
        }

        private static void printplayerLoop(in PlayerLoopSystem playerLoop, int offset)
        {
            if (playerLoop.subSystemList == null)
                return;

            foreach (var s in playerLoop.subSystemList)
            {
                Debug.Log("" + offset + s.type.Name);

                printplayerLoop(s, offset + 1);
            }
        }


        private static void insertSubsystemList(List<PlayerLoopSystem> subsystemList, Type insertType, ComponentSystemGroup group)
        {
            var insertIndex = subsystemList.FindIndex(x => x.type == insertType);
            if (insertIndex < 0)
            {
                Debug.LogError("insertIndex < 0  insertType=" + insertType);
                return;
            }

            var bindingAttr = BindingFlags.NonPublic;
            var DummyDelegateWrapperT = typeof(ScriptBehaviourUpdateOrder).FindMembers(MemberTypes.NestedType, bindingAttr, null, null)
                .Where(x => x.Name == "DummyDelegateWrapper")
                //.forEach(x => { Debug.Log(x.Name); });
                .First() as Type;

            var del = DummyDelegateWrapperT.GetConstructor(new Type[] { typeof(ComponentSystemBase) }).Invoke(new object[] { group });
            var updateDelegate = Delegate.CreateDelegate(typeof(PlayerLoopSystem.UpdateFunction), del, "TriggerUpdate") as PlayerLoopSystem.UpdateFunction;
            subsystemList.Insert(insertIndex + 1, new PlayerLoopSystem { type = group.GetType(), updateDelegate = updateDelegate });

            //var del = new ScriptBehaviourUpdateOrder.DummyDelegateWrapper(group);
            //subsystemList.Insert(insertIndex + 1, new PlayerLoopSystem { type = group.GetType(), updateDelegate = del.TriggerUpdate });
        }
    }
}
