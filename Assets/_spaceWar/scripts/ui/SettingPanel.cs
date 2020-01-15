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

        void onPanelVisible(bool v)
        {
            if (v == false)
            {
                SettingData.singleton.save();
            }
        }

        protected void on_FXAA_updateUI(Button b)
        {
            b.GetComponentInChildren<Text>().text = "FXAA " + (SettingData.singleton.FXAA ? "On" : "Off");
        }
        protected void on_FXAA(Button b)
        {
            SettingData.singleton.FXAA = !SettingData.singleton.FXAA;
            on_FXAA_updateUI(b);
        }

        protected void on_bloom_updateUI(Button b)
        {
            b.GetComponentInChildren<Text>().text = "Bloom " + (SettingData.singleton.bloom ? "On" : "Off");
        }
        protected void on_bloom(Button b)
        {
            SettingData.singleton.bloom = !SettingData.singleton.bloom;
            on_bloom_updateUI(b);
        }

        protected void on_ambientOcclusion_updateUI(Button b)
        {
            b.GetComponentInChildren<Text>().text = "Ambient Occlusion " + (SettingData.singleton.ambientOcclusion ? "On" : "Off");
        }
        protected void on_ambientOcclusion(Button b)
        {
            SettingData.singleton.ambientOcclusion = !SettingData.singleton.ambientOcclusion;
            on_ambientOcclusion_updateUI(b);
        }

        protected void on_vignette_updateUI(Button b)
        {
            b.GetComponentInChildren<Text>().text = "Vignette " + (SettingData.singleton.vignette ? "On" : "Off");
        }
        protected void on_vignette(Button b)
        {
            SettingData.singleton.vignette = !SettingData.singleton.vignette;
            on_vignette_updateUI(b);
        }

        protected void on_depthOfField_updateUI(Button b)
        {
            b.GetComponentInChildren<Text>().text = "Depth Of Field " + (SettingData.singleton.depthOfField ? "On" : "Off");
        }
        protected void on_depthOfField(Button b)
        {
            SettingData.singleton.depthOfField = !SettingData.singleton.depthOfField;
            on_depthOfField_updateUI(b);
        }

        protected void on_autoExposure_updateUI(Button b)
        {
            b.GetComponentInChildren<Text>().text = "Auto Exposure " + (SettingData.singleton.autoExposure ? "On" : "Off");
        }
        protected void on_autoExposure(Button b)
        {
            SettingData.singleton.autoExposure = !SettingData.singleton.autoExposure;
            on_autoExposure_updateUI(b);
        }

        protected void on_qualityLevel_updateUI(Button b)
        {
            b.GetComponentInChildren<Text>().text = SettingData.singleton.qualityLevelName;
        }
        protected void on_qualityLevel(Button b)
        {
            SettingData.singleton.qualityLevel = ++SettingData.singleton.qualityLevel;
            on_qualityLevel_updateUI(b);
        }


        protected void on_volume_updateUI(Button b)
        {
            b.GetComponentInChildren<Text>().text = "Volume " + (SettingData.singleton.volume > 0f ? "On" : "Off");
        }
        protected void on_volume(Button b)
        {
            SettingData.singleton.volume = SettingData.singleton.volume > 0f ? 0f : 1f;
            on_volume_updateUI(b);
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