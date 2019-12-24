using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PrintInfo : MonoBehaviour
{
    void Awake()
    {
        printAppInfo();
        printSystemInfo();
        printCameraInfo();
    }


#if UNITY_EDITOR
    [MenuItem("RNTools/Print/all info")]
#endif
    public static void printAllInfo()
    {
        printAppInfo();
        printSystemInfo();
        printCameraInfo();

        Debug.Log("int.MaxValue=" + int.MaxValue + "  int.MaxValue=" + int.MinValue);
    }


#if UNITY_EDITOR
    [MenuItem("RNTools/Print/App info")]
#endif
    public static void printAppInfo()
    {
        print("platform=" + Application.platform
            + "\n" + "isEditor=" + Application.isEditor
            //+ "\n" + "isWebPlayer=" + Application.isWebPlayer
            //+ "\n" + "isLoadingLevel=" + Application.isLoadingLevel
            + "\n" + "runInBackground=" + Application.runInBackground

            + "\n" + "systemLanguage=" + Application.systemLanguage

            + "\n" + "productName=" + Application.productName
            //+ "\n" + "srcValue=" + Application.srcValue
            + "\n" + "unityVersion=" + Application.unityVersion
            + "\n" + "version=" + Application.version
            + "\n" + "companyName=" + Application.companyName
            + "\n" + "bundleIdentifier=" + Application.identifier
            + "\n" + "cloudProjectId=" + Application.cloudProjectId

            + "\n" + "absoluteURL=" + Application.absoluteURL
            + "\n" + "dataPath=" + Application.dataPath
            + "\n" + "streamingAssetsPath=" + Application.streamingAssetsPath
            + "\n" + "persistentDataPath=" + Application.persistentDataPath
            + "\n" + "temporaryCachePath=" + Application.temporaryCachePath

            + "\n" + "sceneCountInBuildSettings=" + SceneManager.sceneCountInBuildSettings
            + "\n" + "loadedLevel=" + SceneManager.GetActiveScene().buildIndex
            + "\n" + "loadedLevelName=" + SceneManager.GetActiveScene().name
            + "\n" + "targetFrameRate=" + Application.targetFrameRate
            //+ "\n" + "webSecurityEnabled=" + Application.webSecurityEnabled
            //+ "\n" + "webSecurityHostUrl=" + Application.webSecurityHostUrl
            );

    }


#if UNITY_EDITOR
    [MenuItem("RNTools/Print/System info")]
#endif
    public static void printSystemInfo()
    {
        print(
#if UNITY_IPHONE
            //+ "\n" + "iPhoneSettings.generation=" + iPhoneSettings.generation
            "iPhone.generation=" + UnityEngine.iOS.Device.generation
            + "\n" + 
#endif
                     "deviceModel=" + SystemInfo.deviceModel
            + "\n" + "deviceName=" + SystemInfo.deviceName
            + "\n" + "deviceUniqueIdentifier=" + SystemInfo.deviceUniqueIdentifier
            + "\n" + "graphicsDeviceID=" + SystemInfo.graphicsDeviceID
            + "\n" + "graphicsDeviceName=" + SystemInfo.graphicsDeviceName
            + "\n" + "graphicsDeviceVendor=" + SystemInfo.graphicsDeviceVendor
            + "\n" + "graphicsDeviceVendorID=" + SystemInfo.graphicsDeviceVendorID
            + "\n" + "graphicsDeviceVersion=" + SystemInfo.graphicsDeviceVersion
            + "\n" + "graphicsMemorySize=" + SystemInfo.graphicsMemorySize
            //+ "\n" + "graphicsPixelFillrate=" + SystemInfo.graphicsPixelFillrate
            + "\n" + "graphicsShaderLevel=" + SystemInfo.graphicsShaderLevel
            + "\n" + "operatingSystem=" + SystemInfo.operatingSystem
            + "\n" + "processorCount=" + SystemInfo.processorCount
            + "\n" + "processorType=" + SystemInfo.processorType
            //+ "\n" + "supportsImageEffects=" + SystemInfo.supportsImageEffects
            //+ "\n" + "supportsRenderTextures=" + SystemInfo.supportsRenderTextures
            + "\n" + "supportsShadows=" + SystemInfo.supportsShadows
            //+ "\n" + "supportsVertexPrograms=" + SystemInfo.supportsVertexPrograms
            + "\n" + "systemMemorySize=" + SystemInfo.systemMemorySize
            );

    }


#if UNITY_EDITOR
    [MenuItem("RNTools/Print/Camera info")]
#endif
    public static void printCameraInfo()
    {
        if (Camera.main != null)
            print("Camera.main.rect=" + Camera.main.rect
                + "\n" + "Camera.main.pixelRect=" + Camera.main.pixelRect
                + "\n" + "Camera.main.pixelWidth=" + Camera.main.pixelWidth
                + "\n" + "Camera.main.pixelHeight=" + Camera.main.pixelHeight);
    }
}
