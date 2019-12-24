using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleEx : MonoBehaviour
    {
        public string[] texts;
        public Sprite[] icons;
        public GameObject[] gos;

        protected void Awake()
        {
            var toggle = GetComponent<Toggle>();


            var icon = transform.find<Image>("icon");
            if (icon != null)
            {
                if (icons.Length == 0)
                    Debug.LogError("icons.Length == 0", this);
                toggle.onValueChanged.AddListener((v) => icon.sprite = v ? icons[0] : icons[1]);
            }


            var text = transform.find<Text>("icon");
            if (text == null)
                text = transform.find<Text>("text");
            if (text == null)
                Debug.LogError("text == null", this);
            if (texts.Length == 0)
                Debug.LogError("texts.Length == 0", this);
            toggle.onValueChanged.AddListener((v) => text.text = v ? texts[0] : texts[1]);


            toggle.onValueChanged.AddListener((v) =>
            {
                if (gos.Length >= 1 && gos[0] != null)
                {
                    gos[0].SetActive(v);
                }
                if (gos.Length >= 2 && gos[1] != null)
                {
                    gos[1].SetActive(!v);
                }
            });
        }
    }
}