using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class __ClientWorld : MonoBehaviour
    {
        World world;

        public static string __ip;
        public static ushort __port;

        [Header("NetworkStreamSystem")]
        public string ip;
        public ushort port;

        /// <summary> 尝试连接次数 </summary>
        public int maxConnectAttempts = NetworkParameterConstants.MaxConnectAttempts;

        public int clientPacketDelay = 0;
        public int clientPacketDrop = 0;

        public float connectTime = 0f;
        public bool random = true;
        public float randomConnectTime = 0.1f;

        [Header("NetworkPingClientSystem")]
        public NetworkPingClientSystem.Ping ping;

        [Header("PlayerNameClientSystem")]
        public int minPlayerNameCount = 2;
        public int maxPlayerNameCount = 16;

        [Header("AttributeModifyFxClientSystem")]
        public float fadeOutTime = 0.25f;

        [Header("PlayerInputClientSystem")]
        public float maxAngle = 90f;
        public float mousePointMaxDistance = 50f;

        [Header("ActorLocatorSystem<Ship>")]
        public float distanceMin = 2f;
        public float distanceScale = 0.005f;

        [Header("Actor3DUIPanelBySystem")]
        public LayerMask layerMask;
        public float radius = 1f;
        public float showTime = 1f;

        void Awake()
        {
            if (string.IsNullOrEmpty(__ip))
                return;

            ip = __ip;
            port = __port;

            __ip = null;
            __port = 0;
        }

        public void OnWorldInitialized(World world)
        {
            this.world = world;

            var networkStreamSystem = world.GetExistingSystem<NetworkStreamSystem>();
            networkStreamSystem.maxConnectAttempts = maxConnectAttempts;
            networkStreamSystem.clientPacketDelay = clientPacketDelay;
            networkStreamSystem.clientPacketDrop = clientPacketDrop;

            world.GetExistingSystem<NetworkPingClientSystem>().ping = ping;

            world.GetExistingSystem<NetworkStreamDisconnectClientSystem>().onClosed += onClosed;

            world.GetExistingSystem<NetworkVersionSystem>().versionResult += versionResult;

            world.GetExistingSystem<PlayerEnterGameClientSystem>().playerEnterGameResult += playerEnterGameResult;

            world.GetExistingSystem<PlayerGameReadyClientSystem>().gameReady += gameReady;
            world.GetExistingSystem<PlayerGameReadyClientSystem>().gameStart += gameStart;

            if (enabled)
                StartCoroutine(autoStart());


            //
            var attributeModifyFxClientSystem = world.GetExistingSystem<AttributeModifyFxClientSystem>();
            attributeModifyFxClientSystem.fadeOutTime = fadeOutTime;

            //
            var playerInputClientSystem = world.GetExistingSystem<PlayerInputClientSystem>();
            playerInputClientSystem.maxAngle = maxAngle;
            playerInputClientSystem.mousePointMaxDistance = mousePointMaxDistance;

            //
            var shipLocatorSystem = world.GetExistingSystem<ActorLocatorSystem<Ship>>();
            shipLocatorSystem.distanceMin = distanceMin;
            shipLocatorSystem.distanceScale = distanceScale;

            //
            var playerNameClientSystem = world.GetExistingSystem<PlayerNameClientSystem>();
            playerNameClientSystem.minPlayerNameCount = minPlayerNameCount;
            playerNameClientSystem.maxPlayerNameCount = maxPlayerNameCount;

            //
            var actor3DUIByMouseSystem = world.GetExistingSystem<Actor3DUIPanelBySystem>();
            actor3DUIByMouseSystem.layerMask = layerMask;
            actor3DUIByMouseSystem.radius = radius;
            actor3DUIByMouseSystem.showTime = showTime;
        }

        private void Start()
        {
#if UNITY_CLIENT
            Time.fixedDeltaTime = 0.02f;
#endif
        }

        private IEnumerator autoStart()
        {
            if (connectTime < 0f)
                yield return this;

            if (random)
            {
                connectTime = 0.5f + Random.Range(-randomConnectTime, randomConnectTime);
                yield return new WaitForSeconds(connectTime);
                Connect();

                //enterGameTime = 0.5f + Random.Range(-randomEnterGameTime, randomEnterGameTime);
                //yield return new WaitForSeconds(enterGameTime);
                //EnterGame();
            }
            else
            {
                yield return new WaitForSeconds(connectTime);
                Connect();

                //yield return new WaitForSeconds(enterGameTime);
                //EnterGame();
            }
        }

        [RN._Editor.ButtonInEndArea]
        public NetworkPingClientSystem.Ping _ping()
        {
            var s = world.GetExistingSystem<NetworkPingClientSystem>();
            s.Enabled = !s.Enabled;
            if (s.Enabled)
            {
                s.ping = ping;
                return s.ping;
            }
            else
            {
                s.ping = null;
                return null;
            }
        }

        [RN._Editor.ButtonInEndArea]
        void Connect()
        {
            var networkStreamSystem = world.GetExistingSystem<NetworkStreamSystem>();

            if (NetworkEndPoint.TryParse(ip, port, out NetworkEndPoint endPoint) == false)
            {
                Debug.LogError($"{name} => NetworkEndPoint.TryParse({ip}, {port}) == false");
                return;
            }

            //
            networkStreamSystem.Connect(endPoint);
            Debug.Log($"{name} => Connect({ip}, {port})");

            //
            var networkStreamStateSystem = world.GetExistingSystem<NetworkStreamStateSystem>();
            networkStreamStateSystem.connectedHandle += EnterGame;
        }

        [RN._Editor.ButtonInEndArea]
        void Disconnect()
        {
            world.GetExistingSystem<NetworkStreamSystem>().Disconnected();
        }

        void onClosed()
        {
            Debug.Log($"{name} => onClosed");
        }

        void versionResult(bool success, NetworkVersionResultNetMessage versionResult)
        {
            if (success)
            {
                //todo...
            }
            else
            {
                //todo...
            }
        }


        [RN._Editor.ButtonInEndArea]
        void EnterGame()
        {
            var playerEnterGameClientSystem = world.GetExistingSystem<PlayerEnterGameClientSystem>();
            playerEnterGameClientSystem.EnterGame();
        }

        void playerEnterGameResult(PlayerEnterGameResultNetMessage playerEnterGameResultNetMessage)
        {
            if (playerEnterGameResultNetMessage.success)
                Debug.Log($"{name} => playerEnterGameResult success");
            else
                Debug.LogError($"{name} => playerEnterGameResult fail 超出最大人数!");
        }


        //
        [Header("Player")]
        public string myPlayerName;

        public int myPlayerTeam;
        public byte myActorType;

        [RN._Editor.ButtonInEndArea1]
        void clearName()
        {
            PlayerPrefs.DeleteKey(PlayerNameClientSystem.PlayerNameKey);
        }
        [RN._Editor.ButtonInEndArea1]
        public void GetMyPlayerName()
        {
            myPlayerName = world.GetExistingSystem<PlayerNameClientSystem>().myPlayerName;
        }

        [RN._Editor.ButtonInEndArea1]
        public void ChangeMyPlayerName()
        {
            world.GetExistingSystem<PlayerNameClientSystem>().ChangeMyPlayerName(myPlayerName);
        }
        [RN._Editor.ButtonInEndArea2]
        public void GetPlayerTeam()
        {
            myPlayerTeam = world.GetExistingSystem<PlayerTeamClientSystem>().myPlayerTeam;
        }

        [RN._Editor.ButtonInEndArea2]
        public void ChangeMyPlayerTeam()
        {
            world.GetExistingSystem<PlayerTeamClientSystem>().ChangeMyPlayerTeam(myPlayerTeam);
        }


        [RN._Editor.ButtonInEndArea3]
        public void ChangeMyActorType()
        {
            world.GetExistingSystem<PlayerActorSelectClientSystem>().ChangeMyActorType(myActorType);
        }
        [RN._Editor.ButtonInEndArea3]
        public void GetMyActorType()
        {
            myActorType = world.GetExistingSystem<PlayerActorSelectClientSystem>().myActorType;
        }
        [RN._Editor.ButtonInEndArea3]
        void clearMyActorType()
        {
            world.GetExistingSystem<PlayerActorSelectClientSystem>().clearMyActorType();
        }


        void gameReady(float t)
        {
            StartCoroutine(gameReadyE(t));
        }
        public float gameReadyTime;
        IEnumerator gameReadyE(float t)
        {
            gameReadyTime = t;

            while (gameReadyTime > 0)
            {
                yield return this;

                gameReadyTime -= Time.deltaTime;
            }

            gameReadyTime = 0f;
        }

        void gameStart()
        {
            Debug.Log($"{name} => gameStart");
        }
    }
}