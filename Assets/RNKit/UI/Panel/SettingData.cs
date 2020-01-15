using UnityEngine;

#if UNITY_POST_PROCESSING_STACK_V2
using System.Linq;
using UnityEngine.Rendering.PostProcessing;
#endif

namespace RN
{
    public class SettingData : Singleton<SettingData>
    {
        protected void Start()
        {
            //
#if UNITY_POST_PROCESSING_STACK_V2
            postProcessProfile = Camera.main.GetComponentInChildren<PostProcessVolume>().profile;
            Debug.Assert(postProcessProfile != null);
#endif
            read();
        }


        //
        public float volume
        {
            get
            {
                return AudioListener.volume;
            }
            set
            {
                AudioListener.volume = value;
            }
        }

        public int fps
        {
            get
            {
                return Application.targetFrameRate;
            }
            set
            {
                Application.targetFrameRate = value;
            }
        }

#if UNITY_POST_PROCESSING_STACK_V2
        public bool FXAA
        {
            get
            {
                return Camera.main.GetComponent<PostProcessLayer>().antialiasingMode != PostProcessLayer.Antialiasing.None;
            }
            set
            {
                Camera.main.GetComponent<PostProcessLayer>().antialiasingMode = value ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing : PostProcessLayer.Antialiasing.None;
            }
        }

        PostProcessProfile postProcessProfile;

        public bool bloom
        {
            get
            {
                return postProcessProfile.settings.Find(x => x is Bloom).active;
            }
            set
            {
                postProcessProfile.settings.Find(x => x is Bloom).active = value;
            }
        }

        public bool depthOfField
        {
            get
            {
                return postProcessProfile.settings.Find(x => x is DepthOfField).active;
            }
            set
            {
                postProcessProfile.settings.Find(x => x is DepthOfField).active = value;
            }
        }

        public bool ambientOcclusion
        {
            get
            {
                return postProcessProfile.settings.Find(x => x is AmbientOcclusion).active;
            }
            set
            {
                postProcessProfile.settings.Find(x => x is AmbientOcclusion).active = value;
            }
        }

        public bool vignette
        {
            get
            {
                return postProcessProfile.settings.Find(x => x is Vignette).active;
            }
            set
            {
                postProcessProfile.settings.Find(x => x is Vignette).active = value;
            }
        }

        public bool autoExposure
        {
            get
            {
                return postProcessProfile.settings.Find(x => x is AutoExposure).active;
            }
            set
            {
                postProcessProfile.settings.Find(x => x is AutoExposure).active = value;
            }
        }
#endif

        public int qualityLevel
        {
            get
            {
                return QualitySettings.GetQualityLevel();
            }
            set
            {
                if (value >= qualityLevelCount)
                    value = 0;
                QualitySettings.SetQualityLevel(value);

#if false
                UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset = ???;
#endif
            }
        }

        public string qualityLevelName
        {
            get { return QualitySettings.names[QualitySettings.GetQualityLevel()]; }
        }

        public int qualityLevelCount
        {
            get { return QualitySettings.names.Length; }
        }

        //
        void read()
        {
            volume = PlayerPrefs.GetFloat(this + ".volume", volume);
            //BGMVolume = PlayerPrefs.GetFloat(this + ".BGMVolume", BGMVolume);

#if UNITY_POST_PROCESSING_STACK_V2
            FXAA = PlayerPrefsX.GetBool(this + ".FXAA", FXAA);
            bloom = PlayerPrefsX.GetBool(this + ".bloom", bloom);
            depthOfField = PlayerPrefsX.GetBool(this + ".depthOfField", depthOfField);
            ambientOcclusion = PlayerPrefsX.GetBool(this + ".ambientOcclusion", ambientOcclusion);
            vignette = PlayerPrefsX.GetBool(this + ".vignette", vignette);
            autoExposure = PlayerPrefsX.GetBool(this + ".autoExposure", autoExposure);
#endif

            qualityLevel = PlayerPrefs.GetInt(this + ".qualityLevel", QualitySettings.GetQualityLevel());
        }
        public void save()
        {
            PlayerPrefs.SetFloat(this + ".volume", volume);
            //PlayerPrefs.SetFloat(this + ".BGMVolume", BGMVolume);

#if UNITY_POST_PROCESSING_STACK_V2
            PlayerPrefsX.SetBool(this + ".FXAA", FXAA);
            PlayerPrefsX.SetBool(this + ".bloom", bloom);
            PlayerPrefsX.SetBool(this + ".depthOfField", depthOfField);
            PlayerPrefsX.SetBool(this + ".ambientOcclusion", ambientOcclusion);
            PlayerPrefsX.SetBool(this + ".vignette", vignette);
            PlayerPrefsX.SetBool(this + ".autoExposure", autoExposure);

#endif
            PlayerPrefs.SetInt(this + ".qualityLevel", qualityLevel);

            PlayerPrefs.Save();
        }

        [RN._Editor.ButtonInEndArea]
        public void clear()
        {
            PlayerPrefs.DeleteKey(this + ".volume");
            //PlayerPrefs.DeleteKey(this + ".BGMVolume");

#if UNITY_POST_PROCESSING_STACK_V2
            PlayerPrefs.DeleteKey(this + ".FXAA");
            PlayerPrefs.DeleteKey(this + ".bloom");
            PlayerPrefs.DeleteKey(this + ".depthOfField");
            PlayerPrefs.DeleteKey(this + ".ambientOcclusion");
            PlayerPrefs.DeleteKey(this + ".vignette");
            PlayerPrefs.DeleteKey(this + ".autoExposure");

#endif
            PlayerPrefs.DeleteKey(this + ".qualityLevel");
        }
    }
}