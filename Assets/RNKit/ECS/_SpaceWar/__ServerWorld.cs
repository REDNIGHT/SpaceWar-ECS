using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class __ServerWorld : MonoBehaviour
    {
        World world;

        [Header("NetworkStreamSystem")]
        public ushort listenPort = 80;

        //public int connectTimeoutMS = NetworkParameterConstants.ConnectTimeoutMS;//这个变量没用 UdpNetworkDriver里的逻辑有问题
        /// <summary> 在服务器 连接在没有收到数据时 超过这个时间会断开连接 </summary>
        public int disconnectTimeoutMS = NetworkParameterConstants.DisconnectTimeoutMS * 2;

        [Header("NetworkStreamReceiveStatisticalSystem")]
        public NetworkStreamReceiveStatisticalSystem.Statistical receiveStatistical;
        public NetworkStreamSendStatisticalSystem.Statistical sendStatistical;

        [Header("PlayerEnterGameServerSystem")]
        public int playerMaxCount = 64;


        [Header("PlayerActorSelectServerSystem")]
        public ActorTypes actorTypeBegin = ActorTypes.ShipA;
        public ActorTypes actorTypeEnd = ActorTypes.ShipE;

        [Header("PlayerGameReadyServerSystem")]
        public float gameReadyTime = 2.5f;

        [Header("PlayerCreateActorOnGameStartServerSystem")]
        public PlayerCreateActorOnGameStartServerSystem.SelectType findStartPointType = PlayerCreateActorOnGameStartServerSystem.SelectType.Next;

        [Header("WeaponOnInstallServerSystem")]
        public float uninstallForce = 8f;
        public float uninstallTorque = 1f;
        public float uninstallInputTimeScale = 2.5f;


        [Header("SceneObjectAutoResetServerSystem")]
        public float radius = 100f;
        public float interval = 5f;
        public LayerMask actorLayerMask;

        [Header("ShipCatchWeaponServerSystem")]
        public float catchRadius = 1f;
        public float catchVelocityOffset = 1f;

        [Header("PlayerScoreServerSystem")]
        public short[] scores = new short[] { 1, 2, 3, 4, 5 };

        [Header("PlayerObserveCreateServerSystem")]
        public float visibleDistance = 64f;

        [Header("PlayerObserveServerSystem")]
        public float forceSyncTime = 2.5f;

        [Header("ActorSyncAttributeServerSystem")]
        public int perFrame = 5;


        private void Start()
        {
#if UNITY_SERVER
            Application.targetFrameRate = (int)(1f / Time.fixedDeltaTime);
#endif
        }

        public void OnWorldInitialized(World world)
        {
            this.world = world;

            var networkStreamSystem = world.GetExistingSystem<NetworkStreamSystem>();
            //networkStreamSystem.connectTimeoutMS = connectTimeoutMS;
            networkStreamSystem.disconnectTimeoutMS = disconnectTimeoutMS;


            NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4;
            endPoint.Port = listenPort;
            if (networkStreamSystem.Listen(endPoint) == false)
            {
                Debug.LogError($"Listen(port:{listenPort}) == false");
            }
            else
            {
                Debug.Log($"Server Listen(port:{listenPort})");
            }

            //
            receiveStatistical = world.GetExistingSystem<NetworkStreamReceiveStatisticalSystem>().statistical;
            sendStatistical = world.GetExistingSystem<NetworkStreamSendStatisticalSystem>().statistical;

            //
            var playerEnterGameServerSystem = world.GetExistingSystem<PlayerEnterGameServerSystem>();
            playerEnterGameServerSystem.playerMaxCount = playerMaxCount;

            //
            var playerActorSelectServerSystem = world.GetExistingSystem<PlayerActorSelectServerSystem>();
            playerActorSelectServerSystem.actorTypeBegin = (byte)actorTypeBegin;
            playerActorSelectServerSystem.actorTypeEnd = (byte)actorTypeEnd;

            //
            var playerGameReadyServerSystem = world.GetExistingSystem<PlayerGameReadyServerSystem>();
            playerGameReadyServerSystem.gameReadyTime = gameReadyTime;

            //
            var playerCreateActorOnGameStartServerSystem = world.GetExistingSystem<PlayerCreateActorOnGameStartServerSystem>();
            playerCreateActorOnGameStartServerSystem.selectType = findStartPointType;

            //
            var weaponOnInstallServerSystem = world.GetExistingSystem<WeaponOnInstallServerSystem>();
            weaponOnInstallServerSystem.uninstallForce = uninstallForce;
            weaponOnInstallServerSystem.uninstallTorque = uninstallTorque;
            weaponOnInstallServerSystem.uninstallInputTimeScale = uninstallInputTimeScale;

            //
            var SceneObjectAutoResetServerSystem = world.GetExistingSystem<SceneObjectAutoResetServerSystem>();
            SceneObjectAutoResetServerSystem.radius = radius;
            SceneObjectAutoResetServerSystem.interval = interval;
            SceneObjectAutoResetServerSystem.actorLayerMask = actorLayerMask;

            //
            var shipCatchWeaponServerSystem = world.GetExistingSystem<CatchWeaponServerSystem>();
            shipCatchWeaponServerSystem.catchRadius = catchRadius;
            shipCatchWeaponServerSystem.catchVelocityOffset = catchVelocityOffset;

            //
            var playerScoreServerSystem = world.GetExistingSystem<PlayerScoreServerSystem>();
            playerScoreServerSystem.setScores(scores);

            //
            var playerObserveCreateServerSystem = world.GetExistingSystem<PlayerObserveCreateServerSystem>();
            playerObserveCreateServerSystem.visibleDistance = visibleDistance;

            //
            var playerObserveServerSystem = world.GetExistingSystem<PlayerObserveServerSystem>();
            playerObserveServerSystem.forceSyncTime = forceSyncTime;

            //
            var actorSyncAttributeServerSystem = world.GetExistingSystem<ShipSyncAttributeServerSystem>();
            actorSyncAttributeServerSystem.perFrame = perFrame;
        }


        [RN._Editor.ButtonInEndArea]
        void toggleNetworkStreamStatistical()
        {
            var r = world.GetExistingSystem<NetworkStreamReceiveStatisticalSystem>();
            var s = world.GetExistingSystem<NetworkStreamSendStatisticalSystem>();
            r.Enabled = !r.Enabled;
            s.Enabled = !s.Enabled;
        }
    }
}