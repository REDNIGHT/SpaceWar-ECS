using UnityEngine;
using UnityEngine.UI;

namespace RN.Network.SpaceWar
{
    public class ActorAttributePanel : MonoBehaviour
    {
        static readonly Quaternion offset = Quaternion.Euler(90f, 0f, 0f);

        public RectTransform attributes;
        public void setAttributes(in ActorAttribute3<_HP> hp, in ActorAttribute3<_Power> power, in CameraDataSingleton cameraData)
        {
            //Debug.Log($"hp={hp.value}  max={hp.max}  hp.present={hp.present}");
            attributes.GetChild(0).GetComponent<Image>().fillAmount = hp.present;
            attributes.GetChild(1).GetComponent<Image>().fillAmount = power.present;
            attributes.GetChild(2).GetComponent<Text>().text = $"{hp.value.ToString("f0")} / {hp.max.ToString("f0")}";
            attributes.GetChild(3).GetComponent<Text>().text = $"{power.value.ToString("f0")} / {power.max.ToString("f0")}";

            transform.rotation = cameraData.targetRotation * offset;
        }
    }
}