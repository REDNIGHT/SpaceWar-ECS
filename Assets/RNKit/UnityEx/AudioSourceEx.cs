using UnityEngine;
using System.Collections;

public static class AudioSourceEx
{
    public static IEnumerator fadeIn(this AudioSource audio, float fadeOutTime = 0.5f)
    {
        audio.Play();

        var b = 0f;
        var e = audio.volume;
        foreach (var t in new TimeEquation().linear.playRealtime(fadeOutTime))
        {
            audio.volume = Mathf.Lerp(b, e, t);
            yield return null;
        }
    }
    public static IEnumerator fadeOut(this AudioSource audio, float fadeOutTime = 0.5f, float delay = 0f)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        var b = audio.volume;
        var e = 0f;
        foreach (var t in new TimeEquation().linear.playRealtime(fadeOutTime))
        {
            audio.volume = Mathf.Lerp(b, e, t);
            yield return null;
        }

        audio.Stop();
        audio.volume = b;
    }
}
