using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

class FindMissingComponents
{
    static Transform[] transforms;
    static List<UnityEngine.Object> offenders = new List<UnityEngine.Object>();
    // Add menu item to the menu.
    [MenuItem("Component/Find Missing Components")]
    static void FindMissingComponentsFunction()
    {
        var gos = SceneManager.GetActiveScene().GetRootGameObjects();
        offenders.Clear();
        foreach (var go in gos)
        {
            checkObject(go);
        }
        Selection.objects = offenders.ToArray();
        Debug.Log("Found " + offenders.Count.ToString() + " objects with missing components");
    }

    static void checkObject(GameObject go)
    {
        var checkStr = "checking: " + go.name;
        if (go.hideFlags != HideFlags.None)
        {
            checkStr += "    hideFlags=" + go.hideFlags;
            //go.hideFlags = HideFlags.None;
        }

        Debug.Log(checkStr, go);

        Component[] comps = go.GetComponents<Component>();
        foreach (var item in comps)
        {
            if (item == null)
            {
                offenders.Add(go);
                Debug.LogWarning("Missing Component: " + go.name, go);
            }
        }


        //
        foreach (Transform c in go.transform)
        {
            checkObject(c.gameObject);
        }
    }

}