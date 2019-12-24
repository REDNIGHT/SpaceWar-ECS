using System.Collections.Generic;
using System;
using Unity.Entities;
using UnityEngine.Experimental.PlayerLoop;

namespace RN.Network.SpaceWar
{
    //
    public class ServerNetworkBootstrap : Network.ServerBootstrap
    {
        public override IEnumerable
            <(
                (Type type, Type subType) order,
                ComponentSystemGroup group,
                EntityCommandBufferSystem beginCommandSystem,
                IEnumerable<ComponentSystemBase> systems,
                EntityCommandBufferSystem endCommandSystem
            )> createSystemMaps()
        {
            yield return
            (
                (typeof(FixedUpdate), typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate)),

                new FixedUpdateSystemGroup(),
                null,

                new ComponentSystemBase[]
                {
                    //
                    new CallTriggerSystem(),

                    //
                    new NetworkStreamSystem(),
                    new NetworkStreamReceiveSystem(),
#if UNITY_EDITOR
                    new NetworkStreamReceiveStatisticalSystem(),
#endif
                    new NetworkStreamSerializeSystem(),

                    new NetworkPingServerSystem(),
                    //new NetworkHeartbeatServerSystem(),
                    new NetworkVersionSystem(),
                    new NetworkIdServerSystem(),

                    new PlayerEnterGameServerSystem(),
                    new PlayerServerSystem(),
                    new PlayerNameServerSystem(),
                    new PlayerTeamServerSystem(),
                    new PlayerActorSelectServerSystem(),
                    new PlayerActorArrayServerSystem(),
                    new PlayerGameReadyServerSystem(),
                    new PlayerCreateActorOnGameStartServerSystem(),


                    new EntityBuilderServerSystem<EntityBuilder>(),
                    new SceneObjectAutoResetServerSystem(),

                    
#if true//OnDestroyMessage
        //对这两信息处理的system必须在这后面
                    new ActorCreateOnDestroyServerSystem(),

                    new ShipOnDestroyServerSystem(),
                    new ShieldOnDestroyServerSystem(),

                    //
                    new GameObject_OnDestroyMessageSystem(),

                    //
                    new PlayerScoreServerSystem(),
                    new KillServerSystem(),
#endif

                    


#if true//input操作的system放在这下面
                    new PlayerInputServerSystem(),

                    new ShipControlServerSystem(),
                    new WeaponControlServerSystem(),
#endif

                    
                    //
                    new ControlCommandBufferServerSystem(),//CommandBufferSystem

                    new WeaponFireCreateServerSystem(),

                    new CatchWeaponServerSystem(),

                    new WeaponOnInstallServerSystem(),
                    new WeaponConstraintServerSystem(),
                    new WeaponOnShipDestroyServerSystem(),
                    new SlotOnWeaponInstallServerSystem(),

                    new PlayerActorArrayOnWeaponInstallServerSystem(),

                    new WeaponFirePrepareFxServerSystem(),
                    new ShieldOnCreateServerSystem(),
                    new ShieldControlServerSystem(),
                    new LaserUpdateServerSystem(),
                    

#if true//物理操作的system放在这下面
                    
                    //new ActorVelocitySystem(),


                    new BulletOnCreateServerSystem(),
                    new LaserRaycastServerSystem(),

                    new PhysicsTriggerServerSystem(),

                    new BatteryControlServerSystem(),
                    new MissileControlServerSystem(),


                    new FindTraceTargetSystem(),
                    new BatteryTargetInputServerSystem(),

                    new RaycastByLastPositionSystem(),
                    //new LastPositionSaveSystem(),
#endif


                    //
                    new PhysicsLinecastSystem(),
                    new PhysicsRaycastSystem(),
                    new PhysicsRaycastAllSystem(),
                    new PhysicsSphereCastSystem(),
                    new PhysicsSphereCastAllSystem(),
                    new PhysicsOverlapSphereSystem(),




                    //
                    new RigidbodyControlSystem(),
                    new RigidbodyDragSystem(),
                    new RigidbodyForceSystem(),
                    new RigidbodyInSystem(),

                    new TransformInSystem(),

                    //
                    new TriggerClearSystem(),
                },

                new MiddleCommandBufferSystem()
            );

            yield return
            (
                (typeof(FixedUpdate), typeof(FixedUpdate.ScriptRunDelayedFixedFrameRate)),

                new FixedLateUpdateSystemGroup(),
                null,

                new ComponentSystemBase[]
                {
                    new TransformOutSystem(),
                    new RigidbodyOutSystem(),

                    new WeaponConstraintUpdateServerSystem(),

                    new LastPositionByChildTransformSystem(),

                    new TriggerSystem(),
                    new PhysicsTriggerFxServerSystem(),



#if true//对HP Power操作的system放在这下面
                    new BulletAMServerSystem(),
                    new LaserAMServerSystem(),
                    new MissileExplosionServerSystem(),
                    new ExplosionAMServerSystem(),
                    new AttributeTriggerAMServerSystem(),
#endif


                    new ShipWeaponArrayOnWeaponInstallServerSystem(),
                    new ShipWeaponArray2AttributeLevelsServerSystem(),
                    new ShieldOnWeaponInstallServerSystem(),


                    new ShipPowerServerSystem(),
                    new ActorAttribute3ServerSystem<_HP> { zeroToActorDeath = true, modifyHandleType = ActorAttribute3ServerSystem<_HP>.ModifyHandleType.Sample, regainEnable = false },



                    //
                    new PlayerObserveCreateServerSystem(),
                    new PlayerObserveServerSystem(),
                    new PlayerObserveDestroyServerSystem(),


                    //
                    new ActorSyncCreateServerSystem(),
                    new ActorSyncDestroyServerSystem(),
                    new ActorSyncAttributeServerSystem(),
                    new ActorSyncDatasServerSystem(),
                    //new ActorDestroyServerSystem(),

                    new WeaponSyncInstalledStateServerSystem(),

#if UNITY_EDITOR
                    new NetworkStreamStateSystem(),
                    new NetworkStreamSendStatisticalSystem(),
#endif
                    new NetworkStreamSendSystem(),



                    //
                    new ActorLifetimeSystem(),

                    new GameObject_OnDestroyWithoutMessageSystem(),
                    new DestroyMessageSystem(),

                    new AutoClearSystem<ServerAutoClearAttribute>(),
                },

                new EndCommandBufferSystem()
            );
        }
    }


    //
    public class ClientNetworkBootstrap : Network.ClientBootstrap
    {
        public override IEnumerable
            <(
                (Type type, Type subType) order,
                ComponentSystemGroup group,
                EntityCommandBufferSystem beginCommandSystem,
                IEnumerable<ComponentSystemBase> systems,
                EntityCommandBufferSystem endCommandSystem
            )> createSystemMaps()
        {
            yield return
            (
                (typeof(FixedUpdate), typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate)),

                new FixedUpdateSystemGroup(),
                null,

                new ComponentSystemBase[]
                {
                    new NetworkStreamSystem(),
                    new NetworkStreamReceiveSystem(),
                    new NetworkStreamConnectSuccessSystem(),
                    new NetworkStreamSerializeSystem(),

                    new NetworkPingClientSystem(),
                    new NetworkVersionSystem(),
                    new NetworkIdClientSystem(),

                    new PlayerEnterGameClientSystem(),
                    new PlayerClientSystem(),
                    new PlayerNameClientSystem(),
                    new PlayerTeamClientSystem(),
                    new PlayerActorSelectClientSystem(),
                    new PlayerActorArrayClientSystem(),
                    new PlayerScoreClientSystem(),
                    new PlayerGameReadyClientSystem(),
                    new PlayerDestroyClientSystem(),



                    //
                    new PlayerInput2UISystem(),
                    new PlayerInputClientSystem(),
                    new ShipControlClientSystem(),
                    new SlotAngleLimitLineSystem(),


                    //
                    new ActorSyncCreateClientSystem { actorCountInHashMap = 1024 },
                    new ActorSyncDatasClientSystem(),
                    new ActorSyncAttributeClientSystem(),
                    new WeaponItemCountFxClientSystem(),

                    new ActorSyncDestroyClientSystem(),
                    new ActorDestroyClientSystem(),


                    new WeaponSyncInstalledStateClientSystem(),
                    new WeaponConstraintClientSystem(),
                    new ShipWeaponArrayOnWeaponInstallClientSystem(),
                    new ShipWeaponArray2AttributeLevelsClientSystem(),
                    new ShipPowerClientSystem(),



                    new WeaponFirePrepareFxClientSystem(),
                    new WeaponFireFxClientSystem(),
                    new AccelerateFxOnCreateClientSystem(),
                    new ShieldOnCreateClientSystem(),
                    new ShieldOnDestroyClientSystem(),
                    new ShieldOnUpdateClientSystem(),
                    new LaserUpdateClientSystem(),

                    new KillClientSystem(),

                    new ActorVelocitySystem(),
                    new AttributeModifyFxClientSystem { fadeOutTime = 0.5f },


                    new RigidbodyControlSystem(),
                    new RigidbodyForceSystem(),

                    new TransformInSystem(),

                    new RigidbodyInSystem(),
                },

                new MiddleCommandBufferSystem()
            );

            yield return
            (
                (typeof(FixedUpdate), typeof(FixedUpdate.ScriptRunDelayedFixedFrameRate)),

                new FixedLateUpdateSystemGroup(),
                null,

                new ComponentSystemBase[]
                {
                    //new TransformOutSystem(),
                    //new RigidbodyOutSystem(),

                    new WeaponConstraintUpdateClientSystem(),

                    new CameraControllerSystem(),
                    new ActorLocatorSystem<Ship>
                    {
                        Name_TransformIndex = ShipSpawner.Name_TransformIndex,
                        MyLocator_TransformIndex = ShipSpawner.MyLocator_TransformIndex,
                        TeamLocators_TransformIndex = ShipSpawner.TeamLocators_TransformIndex
                    },
                    new ActorAttributePanelSystem(),

                    new NetworkStreamStateSystem(),
                    new NetworkStreamSendSystem(),
                    new NetworkStreamDisconnectClientSystem(),



                    new ActorLifetimeSystem(),
                    new DestroyMessageSystem(),

                    new AutoClearSystem<ClientAutoClearAttribute>(),
                },

                new EndCommandBufferSystem()
            );
        }
    }
}