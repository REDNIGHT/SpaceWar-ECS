using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RN.UI;
using Unity.Networking.Transport;
using UnityEngine.Networking;

namespace RN.Network.SpaceWar
{
    public class MainPanel : SubPanel<MainPanel>, IOnLoadUI
    {
        public class GameServerInfo
        {
            public string name;
            public string ip;
            public ushort port;

            public int maxPlayerCount;
            public int curPlayerCount;
            //...

            //[System.NonSerialized]
            //public float lifeTime;
        }

        public class GameServerInfos
        {
            public string v;
            public GameServerInfo[] gsInfos;
        }

        GameServerInfos gameServerInfos;

        public string masterServerUrl;
        public string updateUrl;
        public bool showPing = false;

        public IEnumerator onLoadUI()
        {
            if (string.IsNullOrEmpty(masterServerUrl))
                yield break;

            using (var webRequest = UnityWebRequest.Get(masterServerUrl))
            {
                yield return webRequest.SendWebRequest();


                Debug.Log($"webRequest.responseCode={webRequest.responseCode}");
                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Message.singleton.show(webRequest.error, 5f);
                }
                else
                {
                    Debug.Log($"webRequest.text={webRequest.downloadHandler.text}");
                    gameServerInfos = JsonUtility.FromJson<GameServerInfos>(webRequest.downloadHandler.text);

                    if (versionFail(gameServerInfos.v, Application.version))
                    {
                        MessageBox.singleton.Show((m) =>
                        {
                            m.Message($"version error: cur version:{gameServerInfos.v}\n Please update client!");
                            m.No(null);
                            m.Yes(null);
                        },
                        (m) =>
                        {
                            if (string.IsNullOrEmpty(updateUrl))
                                return null;

                            Application.OpenURL(updateUrl);
                            return null;
                        });
                    }
                    else
                    {
                        if (gameServerInfos.gsInfos == null)
                        {
                            Debug.LogError("gameServerInfos.gsInfos == null");

                            Message.singleton.show("Can't find any game servers!", 5f);
                            yield break;
                        }

                        var i = 0;
                        foreach (var info in gameServerInfos.gsInfos)
                        {
                            initItem(i, info);
                            ++i;
                        }
                    }
                }
            }
        }

        protected bool versionFail(string versionA, string versionB)
        {
            var vas = versionA.Split('.');
            var vbs = versionB.Split('.');
            if (vas[0] == vas[0] && vbs[1] == vbs[1])
                return false;
            return true;
        }

        public Transform content;
        protected void initItem(int index, GameServerInfo sinfo)
        {
            if (index >= content.childCount)
            {
                var prefab = content.GetChild(0);
                GameObject.Instantiate(prefab, content);
            }

            var infoT = content.GetChild(index);
            infoT.GetChild(0).GetComponent<Text>().text = sinfo.name;
            infoT.GetChild(1).GetComponent<Text>().text = $"{sinfo.curPlayerCount} / {sinfo.maxPlayerCount}";

            if (showPing)
            {
                StartCoroutine(pingE(sinfo.ip, infoT.GetChild(1).GetComponent<Text>()));
            }
        }

#if !UNITY_WEBGL
        protected IEnumerator pingE(string ip, Text text)
        {
            var _ping = new Ping(ip);
            while (_ping.isDone == false)
            {
                yield return this;

                if (visible == false)
                    yield break;
            }

            text.text += $"  {_ping.time}ms";
        }
#else
        protected IEnumerator pingE(string ip, Text text)
        {
            yield break;
        }
#endif

        protected IEnumerator on_sInfoItem(Button b)
        {
            var sInfo = gameServerInfos.gsInfos[b.transform.GetSiblingIndex()];
            return next(sInfo.ip, sInfo.port, 1);
        }



        //
        const string IpKey = "IP";
        [RN._Editor.ButtonInEndArea]
        void clearIp()
        {
            PlayerPrefs.DeleteKey(IpKey);
        }
        InputField ipInputField;
        protected void on_ip_updateUI(InputField inputField)
        {
            ipInputField = inputField;
            ipInputField.text = PlayerPrefs.GetString(IpKey, "127.0.0.1:80");
        }
        protected void on_ipEnd(InputField inputField) { }


        protected IEnumerator on_connect(Button b)
        {
            string ip = "";
            ushort port = 80;

            var ip_port = ipInputField.text.Split(':');
            if (ip_port.Length == 2)
            {
                ip = ip_port[0];
                port = ushort.Parse(ip_port[1]);
            }
            else
            {
                ip = ipInputField.text;
            }


            if (NetworkEndPoint.TryParse(ip, port, out NetworkEndPoint _) == false)
            {
                Message.singleton.show("ip error");
                return null;
            }

            /*var ping = new Ping(ip + ":" + port);
            while (ping.isDone == false)
            {
                Debug.Log($"ping.isDone={ping.isDone}");
                yield return this;
            }
            Debug.Log($"ping.time={ping.time}");*/


            PlayerPrefs.SetString(IpKey, ipInputField.text);

            return next(ip, port, 1);
        }

        IEnumerator next(string ip, ushort port, int sceneBuildIndex)
        {
            __ClientWorld.__ip = ip;
            __ClientWorld.__port = port;


            //
            yield return Out();
            yield return LoadingPanel.singleton.In();
            LoadingPanel.singleton.setProgress(SceneManager.LoadSceneAsync(sceneBuildIndex));
        }

        IEnumerator on_host(Button b)
        {
            return next("127.0.0.1", 80, 2);
        }
    }
}