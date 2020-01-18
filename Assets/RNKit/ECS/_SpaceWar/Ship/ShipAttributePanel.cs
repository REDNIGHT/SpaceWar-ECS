using UnityEngine;
using UnityEngine.UI;

namespace RN.Network.SpaceWar
{
    public class ShipAttributePanel : MonoBehaviour
    {
        public RectTransform attributes;
        public void setAttributes(in ActorAttribute3<_HP> hp, in ActorAttribute3<_Power> power)
        {
            //Debug.Log($"hp={hp.value}  max={hp.max}  hp.present={hp.present}");
            var t = attributes.GetChild(0);
            t.GetComponent<Image>().fillAmount = hp.present;


            t = attributes.GetChild(1);
            t.GetComponent<Image>().fillAmount = power.present;


            t = attributes.GetChild(2);
            t.GetComponent<Text>().text = $"HP {hp.value.ToString("f0")} / {hp.max.ToString("f0")}    POW {power.value.ToString("f0")} / {power.max.ToString("f0")}";
        }
    }
}