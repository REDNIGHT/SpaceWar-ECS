using System.Collections.Generic;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class WeaponFirePoint : MonoBehaviour
    {
        public enum FirePointType
        {
            parallel,
            sequence,
            random,
            random2Direction,
        }

        public FirePointType firePointType;
        public Transform firePointT;
        int sequenceIndex = 0;
        void Awake()
        {
            if (firePointT == null)
            {
                firePointT = transform.GetChild(WeaponSpawner.firePoint_TransformIndex);
                if (firePointT == null)
                    Debug.LogError("firePointT == null", this);
            }
        }
        public IEnumerable<Transform> firePoints
        {
            get
            {
                if (firePointType == FirePointType.parallel)
                {
                    if (firePointT.childCount > 0)
                    {
                        foreach (Transform t in firePointT)
                        {
                            yield return t;
                        }
                    }
                    else
                    {
                        yield return firePointT;
                    }
                }
                else if (firePointType == FirePointType.sequence)
                {
                    if (sequenceIndex >= firePointT.childCount)
                        sequenceIndex = 0;
                    yield return firePointT.GetChild(sequenceIndex++);
                }
                else if (firePointType == FirePointType.random)
                {
                    yield return firePointT.GetChild(Random.Range(0, firePointT.childCount));
                }
                else if (firePointType == FirePointType.random2Direction)
                {
                    firePointT.GetChild(2).forward = Vector3.Lerp(firePointT.GetChild(0).forward, firePointT.GetChild(1).forward, Random.value);
                    yield return firePointT.GetChild(2);
                }
            }
        }

        public Quaternion rotation { set => transform.rotation = value; }
    }
}
