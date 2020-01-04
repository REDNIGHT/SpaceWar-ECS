using UnityEngine;
using System.Collections;

public class MissileDelayTrigger : MonoBehaviour
{
    public float delay = 1f;
    IEnumerator Start()
    {
        yield return this;

        var c = GetComponent<Collider>();
        if (c == null)
        {
            Destroy(this);
            yield break;
        }


        Debug.Assert(c.enabled == false, c);
        yield return new WaitForSeconds(delay);
        c.enabled = true;
    }
}
