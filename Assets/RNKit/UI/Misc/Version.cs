using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace RN.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Text))]
    public class Version : MonoBehaviour
    {
        protected void Awake()
        {
            GetComponent<Text>().text = "Version " + Application.version;
        }
    }
}