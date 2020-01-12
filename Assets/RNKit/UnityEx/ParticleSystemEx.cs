using UnityEngine;
using System.Collections;

public static class ParticleSystemEx
{
    public static void autoDestroyRootParticleSystem(this Component psRoot, bool stop)
    {
        var ps = psRoot.GetComponentInChildren<ParticleSystem>();
        ps.startCoroutine(ps.autoDestroyRootParticleSystemE(stop, psRoot));
    }

    public static void autoDestroyRootParticleSystem(this ParticleSystem psRoot, bool stop)
    {
        psRoot.startCoroutine(psRoot.autoDestroyRootParticleSystemE(stop));
    }

    public static void autoDestroyMultiParticleSystem(this Component psRoot, bool stop)
    {
        psRoot.startCoroutine(psRoot.autoDestroyMultiParticleSystemE(stop));
    }

    public static IEnumerator autoDestroyRootParticleSystemE(this ParticleSystem ps, bool stop, Component root = null)
    {
        Debug.Assert(ps.main.stopAction != ParticleSystemStopAction.Destroy, ps);
        if (stop)
        {
            ps.Stop();
        }
        else
        {
            var main = ps.main;
            main.loop = false;
        }

        while (ps.IsAlive())
        {
            yield return ps;
            yield return ps;
        }

        if (root == null)
            root = ps;
        root.destroyGO();
    }

    public static IEnumerator autoDestroyMultiParticleSystemE(this Component psRoot, bool stop)
    {
        var pss = psRoot.GetComponentsInChildren<ParticleSystem>();

#if UNITY_EDITOR
        foreach (var ps in pss)
        {
            Debug.Assert(ps.main.stopAction != ParticleSystemStopAction.Destroy, ps);
        }
#endif
        if (stop)
        {
            foreach (var ps in pss)
            {
                ps.Stop();
            }
        }
        else
        {
            foreach (var ps in pss)
            {
                var main = ps.main;
                main.loop = false;
            }
        }



        foreach (var ps in pss)
        {
            while (ps.IsAlive(false))
            {
                yield return psRoot;
                yield return psRoot;
            }
        }

        psRoot.destroyGO();
    }

    /*
    public static void psPlay(this Component psRoot)
    {
        var ps = psRoot.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }
    }
    public static void psStop(this Component psRoot)
    {
        var ps = psRoot.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop();
        }
    }

    public static bool psAlive(this Component psRoot)
    {
        var pss = psRoot.GetComponentsInChildren<ParticleSystem>();

        foreach (var ps in pss)
        {
            if (ps.IsAlive())
                return true;
        }

        return false;
    }
    */
}