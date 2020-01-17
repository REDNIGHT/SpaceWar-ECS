using UnityEngine;
using UnityEngine.UI;

namespace RN.Network.SpaceWar
{
    public class WeaponAttributePanel : MonoBehaviour
    {
        public RectTransform attributes;
        public void setAttributes(int itemCount)
        {
            attributes.GetChild(0).GetComponent<Text>().text = $"{itemCount}";
        }
    }
}