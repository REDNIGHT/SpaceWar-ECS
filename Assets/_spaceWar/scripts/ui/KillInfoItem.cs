using UnityEngine;
using UnityEngine.UI;

namespace RN.Network.SpaceWar
{
    public class KillInfoItem : MonoBehaviour
    {
        public string mainKillerName;
        public string killerNames;
        public string targetName;
        public float time;


        public void updateItems()
        {
            transform.GetComponent<CanvasGroup>().alpha = 1f;
            transform.GetChild(0).GetComponent<Text>().text = mainKillerName;
            transform.GetChild(1).GetComponent<Text>().text = killerNames;
            transform.GetChild(2).GetComponent<Text>().text = targetName;
        }

        private void FixedUpdate()
        {
            time -= Time.fixedDeltaTime;

            if (time < 0f)
                Object.Destroy(gameObject);

            else if (time < 1f)
                GetComponent<CanvasGroup>().alpha = time;
        }
    }

}