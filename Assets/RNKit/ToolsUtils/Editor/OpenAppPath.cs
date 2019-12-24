using UnityEngine;
using UnityEditor;

public static class OpenAppPath
{
    [MenuItem("Assets/OpenPath/dataPath")]
    public static void openDataPath()
    {
        Debug.Log("Application.dataPath=" + Application.dataPath);
        Application.OpenURL(Application.dataPath);
    }

    [MenuItem("Assets/OpenPath/persistentDataPath")]
    public static void openPersistentDataPath()
    {
        Debug.Log("Application.persistentDataPath="+ Application.persistentDataPath);

        //fuck 中文路径打不开???
        Application.OpenURL(Application.persistentDataPath);
        //System.Diagnostics.Process.Start("Explorer.exe", Application.persistentDataPath);
    }

    [MenuItem("Assets/OpenPath/temporaryCachePath")]
    public static void openTemporaryCachePath()
    {
        Debug.Log("Application.temporaryCachePath=" + Application.temporaryCachePath);

        //fuck 中文路径打不开???
        Application.OpenURL(Application.temporaryCachePath);
        //System.Diagnostics.Process.Start("Explorer.exe", Application.temporaryCachePath);
    }
}
