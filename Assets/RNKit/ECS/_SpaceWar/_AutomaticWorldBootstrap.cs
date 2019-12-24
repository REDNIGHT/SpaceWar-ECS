using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RN.Network.SpaceWar
{
    public class _AutomaticWorldBootstrap : MonoBehaviour
    {
        public Transform serverRoot;
        public Transform[] clientRoots;

        public Transform cameraController;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]//这个只执行一次
        static void Initialize()
        {
            SceneManager.sceneLoaded += sceneLoaded;
            SceneManager.sceneUnloaded += sceneUnloaded;
        }

        static int curSceneIndex = -1;
        static void sceneLoaded(Scene s, LoadSceneMode m)
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Debug.LogWarning($"sceneLoaded:{activeScene.name}  buildIndex={activeScene.buildIndex}");

            var automaticWorldBootstrap = FindObjectOfType<_AutomaticWorldBootstrap>();
            if (automaticWorldBootstrap == null)
                return;

            curSceneIndex = activeScene.buildIndex;

            //
            //WorldInitialization.Initialize_DefaultWorld();
            WorldInitialization.RegisterDomainUnload();

#if !UNITY_CLIENT
            //
            if (automaticWorldBootstrap.serverRoot != null && automaticWorldBootstrap.serverRoot.gameObject.activeSelf)
            {
                var root = automaticWorldBootstrap.serverRoot;
                WorldInitialization.Initialize(root.name, new ServerNetworkBootstrap { root = root });
            }
#endif

#if !UNITY_SERVER
            //
            ClientNetworkBootstrap.SetClientWorldCount(automaticWorldBootstrap.clientRoots.Length);
            for (var i = 0; i < automaticWorldBootstrap.clientRoots.Length; ++i)
            {
                var root = automaticWorldBootstrap.clientRoots[i];
                if (root.gameObject.activeSelf)
                    WorldInitialization.Initialize(root.name, new ClientNetworkBootstrap { worldIndex = i, root = root });
            }


#if UNITY_EDITOR
            initPlayerInputClientSystems();
#endif
#endif
        }

        static void sceneUnloaded(Scene s)
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Debug.LogWarning($"sceneUnloaded:{activeScene.name}  buildIndex={activeScene.buildIndex}");

            if (activeScene.buildIndex != curSceneIndex)
                return;

            curSceneIndex = -1;
            WorldInitialization.DomainUnloadShutdown();
        }




#if UNITY_EDITOR
        int playerInputIndex = 0;

        static void setInputSystems(int index, bool enabled)
        {
            if (ClientBootstrap.worlds[index] == null)
                return;

            ClientBootstrap.worlds[index].GetExistingSystem<PlayerInput2UISystem>().Enabled = enabled;
            ClientBootstrap.worlds[index].GetExistingSystem<PlayerInputClientSystem>().Enabled = enabled;
            ClientBootstrap.worlds[index].GetExistingSystem<CameraControllerSystem>().Enabled = enabled;
        }

        static void initPlayerInputClientSystems()
        {
            if (ClientBootstrap.worlds.Length <= 1)
                return;

            setInputSystems(0, true);

            for (var i = 1; i < ClientBootstrap.worlds.Length; ++i)
            {
                if (ClientBootstrap.worlds[i] == null)
                    continue;

                setInputSystems(i, false);
            }
        }

        void Update()
        {
            if (ClientBootstrap.worlds.Length <= 1)
                return;

            if (Input.GetKeyUp(KeyCode.Tab))
            {
                setInputSystems(playerInputIndex, false);

                do
                {
                    ++playerInputIndex;

                    if (playerInputIndex >= ClientBootstrap.worlds.Length)
                        playerInputIndex = 0;
                } while (ClientBootstrap.worlds[playerInputIndex] == null);

                setInputSystems(playerInputIndex, true);
            }
        }
#endif
    }
}