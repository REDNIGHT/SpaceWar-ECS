using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Mathematics;

namespace RN.Network.SpaceWar
{
    public class WeaponAttributePanel : MonoBehaviour, IActor3DUIPanel
    {
        public RectTransform attributes;
        CanvasGroup canvasGroup;
        private void Awake()
        {
            attributes.gameObject.SetActive(false);
            canvasGroup = attributes.GetComponent<CanvasGroup>();
        }

        bool init = false;
        public void setAttributes(int hp, int itemCount)
        {
            init = true;
            setCount(attributes.GetChild(0), hp);
            setCount(attributes.GetChild(1), itemCount);
        }
        void setCount(Transform root, int count)
        {
            Debug.Assert(count <= root.childCount, "count <= root.childCount", root);

            var i = 0;
            for (; i < count && i < root.childCount; ++i)
            {
                root.GetChild(i).gameObject.SetActive(true);
            }
            for (; i < root.childCount; ++i)
            {
                root.GetChild(i).gameObject.SetActive(false);
            }
        }


        //
        float time = 0f;
        float fadeoutTime = 0.5f;
        public void show(float showTime, in CameraDataSingleton cameraData)
        {
            if (init == false)
                return;

            if (time <= 0f)
            {
                attributes.gameObject.SetActive(true);
            }

            //
            time = showTime;
            fadeoutTime = showTime * 0.75f;
            canvasGroup.alpha = 1f;

            //
            setSide(cameraData);
        }

        private void Update()
        {
            if (time > 0f)
            {
                time -= Time.deltaTime;

                if (time < fadeoutTime)
                {
                    canvasGroup.alpha = time / fadeoutTime;
                }

                if (time <= 0)
                {
                    attributes.gameObject.SetActive(false);
                }
            }
        }


        void setSide(in CameraDataSingleton cameraData)
        {
            transform.localScale = Vector3.one;

            var dampedTransform = GetComponentInParent<_DampedTransform>();
            if (dampedTransform == null)
            {
                //Debug.LogWarning("dampedTransform == null", this);
                return;
            }
            if (dampedTransform.shipT == null)
            {
                //Debug.LogWarning("dampedTransform.shipT == null", this);
                return;
            }


            var weaponDir = dampedTransform.shipT.position - transform.parent.position;

            var cameraForward = math.forward(cameraData.targetRotation);

            var angle = Vector3.SignedAngle(cameraForward, weaponDir, Vector3.up);
            if (angle > 0f)
                transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }
}