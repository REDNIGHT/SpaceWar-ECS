using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Mathematics;

namespace RN.Network.SpaceWar
{
    public class WeaponAttributePanel : MonoBehaviour
    {
        public RectTransform attributes;
        CanvasGroup canvasGroup;
        private void Awake()
        {
            attributes.gameObject.SetActive(false);
            canvasGroup = attributes.GetComponent<CanvasGroup>();
        }
        public void setAttributes(int hp, int itemCount)
        {
            //
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

        void setSide(in CameraDataSingleton cameraData)
        {
            var dampedTransform = GetComponentInParent<_DampedTransform>();
            if (dampedTransform == null)
            {
                Debug.LogWarning("dampedTransform == null", this);
                return;
            }
            if (dampedTransform.shipT == null)
            {
                Debug.LogWarning("dampedTransform.shipT == null", this);
                return;
            }


            var weaponDir = dampedTransform.shipT.position - transform.parent.position;

            var cameraForward = math.forward(cameraData.targetRotation);

            var angle = Vector3.SignedAngle(cameraForward, weaponDir, Vector3.up);

            transform.localScale = angle < 0f ? new Vector3(1f, 1f, 1f) : new Vector3(-1f, 1f, 1f);
        }


        //
        public void autoPlay(in CameraDataSingleton cameraData)
        {
            if (visible)
                return;

            setSide(cameraData);
            StopAllCoroutines();


            attributes.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            StartCoroutine(fadeout(showTime));
        }


        //
        const float showTime = 1.5f;
        const float hideTime = 0.75f;
        IEnumerator fadeout(float _showTime)
        {
            if (_showTime > 0f)
                yield return new WaitForSeconds(_showTime);

            foreach (var t in new TimeEquation().linear.play(hideTime))
            {
                canvasGroup.alpha = 1f - t;
                yield return this;
            }

            attributes.gameObject.SetActive(false);
        }




        //
        public bool visible { get; protected set; } = false;

        public void begin()
        {
            attributes.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;

            StopAllCoroutines();

            //
            visible = true;
        }

        public void end()
        {
            visible = false;

            StartCoroutine(fadeout(0f));
        }

        public void update(in CameraDataSingleton cameraData)
        {
            setSide(cameraData);
        }
    }
}