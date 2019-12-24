using RN.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RN.Network.SpaceWar
{
    public class SettingPanel : SubPanel<SettingPanel>
    {
        public Text _ping;
        private new void Awake()
        {
            base.Awake();

            _ping = transform.parent.Find("ping").GetComponent<Text>();
            Debug.Assert(_ping != null);
            _ping.gameObject.SetActive(false);
        }
        protected void on_ping(Button b)
        {
            var p = PlayerPanel.singleton.clientWorld._ping();
            if (p != null)
                StartCoroutine(nameof(pingE), p);
            else
                StopCoroutine(nameof(pingE));

            _ping.gameObject.SetActive(p != null);
        }

        protected IEnumerator pingE(NetworkPingClientSystem.Ping ping)
        {
            while (true)
            {
                _ping.text = $"ping:{ping.Time}  lost:{ping.LostCount}";

                yield return this;
            }
        }
    }
}