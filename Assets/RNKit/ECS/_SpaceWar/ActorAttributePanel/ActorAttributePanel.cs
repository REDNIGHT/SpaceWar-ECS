using UnityEngine;
using UnityEngine.UI;

namespace RN.Network.SpaceWar
{
    public class ActorAttributePanel : MonoBehaviour
    {
        public void setAttributes(in ActorAttribute3<_HP> hp, in ActorAttribute3<_Power> power)
        {
            //Debug.Log($"hp={hp.value}  max={hp.max}  hp.present={hp.present}");
            transform.GetChild(0).GetChild(0).GetComponent<Image>().fillAmount = hp.present;
            transform.GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = power.present;
            transform.GetChild(0).GetChild(2).GetComponent<Text>().text = $"{hp.value.ToString("f0")} / {hp.max.ToString("f0")}";
            transform.GetChild(0).GetChild(3).GetComponent<Text>().text = $"{power.value.ToString("f0")} / {power.max.ToString("f0")}";
        }
    }
}