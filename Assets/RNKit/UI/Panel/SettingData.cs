using UnityEngine;

#if POST_PROCESSING
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Experimental.Rendering.LightweightPipeline;
#endif

namespace RN
{
    public class SettingData : Singleton<SettingData>
    {
        protected void Start()
        {
            //
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

        /*public bool FXAA
        {
            get
            {
                return Camera.main.GetComponent<PostProcessLayer>().antialiasingMode != PostProcessLayer.Antialiasing.None;
            }
            set
            {
                Camera.main.GetComponent<PostProcessLayer>().antialiasingMode = value ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing : PostProcessLayer.Antialiasing.None;
            }
        }*/

#if POST_PROCESSING
        public PostProcessProfile postProcessProfile;
            
        //public bool defaultFXAA = false;
        public bool defaultBloom = false;
        public bool defaultDepthOfField = false;

        public bool bloom
        {
            get
            {
                return postProcessProfile.settings.Find(x => x is Bloom).active;
            }
            set
            {
                postProcessProfile.settings.Where(x => (x is DepthOfField || x is AmbientOcclusion) == false).forEach(x => x.active = value);
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

        /*public bool ambientOcclusion
        {
            get
            {
                return postProcessProfile.settings.Find(x => x is AmbientOcclusion).active;
            }
            set
            {
                postProcessProfile.settings.Find(x => x is AmbientOcclusion).active = value;
            }
        }*/

        public LightweightPipelineAsset[] lightweightPipelineAssets;
#endif
        public int qualityLevel
        {
            get
            {
                return QualitySettings.GetQualityLevel();
            }
            set
            {
                QualitySettings.SetQualityLevel(value);

#if POST_PROCESSING
                if (value >= lightweightPipelineAssets.Length)
                    value = 0;
                GraphicsSettings.renderPipelineAsset = lightweightPipelineAssets[value];
#endif
            }
        }

        public string qualityLevelName
        {
            get { return QualitySettings.names[QualitySettings.GetQualityLevel()]; }
        }

        //
        public int defaultQualityLevel = 2;
        void read()
        {
            volume = PlayerPrefs.GetFloat(this + ".volume", volume);
            //BGMVolume = PlayerPrefs.GetFloat(this + ".BGMVolume", BGMVolume);

#if POST_PROCESSING
            //FXAA = PlayerPrefsX.GetBool(this + ".FXAA", defaultFXAA);
            bloom = PlayerPrefsX.GetBool(this + ".bloom", defaultBloom);
            depthOfField = PlayerPrefsX.GetBool(this + ".depthOfField", defaultDepthOfField);
            //ambientOcclusion = PlayerPrefsX.GetBool(this + ".ambientOcclusion", ambientOcclusion);
#endif

            qualityLevel = PlayerPrefs.GetInt(this + ".qualityLevel", defaultQualityLevel);

            //fps = PlayerPrefs.GetInt(this + ".fps", fps);
        }
        public void save()
        {
            PlayerPrefs.SetFloat(this + ".volume", volume);
            //PlayerPrefs.SetFloat(this + ".BGMVolume", BGMVolume);

#if POST_PROCESSING
            //PlayerPrefsX.SetBool(this + ".FXAA", FXAA);
            PlayerPrefsX.SetBool(this + ".bloom", bloom);
            PlayerPrefsX.SetBool(this + ".depthOfField", depthOfField);
            //PlayerPrefsX.SetBool(this + ".ambientOcclusion", ambientOcclusion);

            PlayerPrefs.SetInt(this + ".qualityLevel", qualityLevel);
#endif
            //PlayerPrefs.GetInt(this + ".fps", fps);

            PlayerPrefs.Save();
        }
    }
}