using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAudioClipsFx : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>(true);

        Debug.Assert(audioSource != null, "audioSource != null", this);
    }

    void OnDisable()
    {
        audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
    }
}
