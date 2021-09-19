using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    private static AudioPlayer _instance = null;

    public static AudioPlayer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioPlayer> ();
            }

            return _instance;
        }
    }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> audioClips;

    public void PlaySFX (string name)
    {
        AudioClip sfx = audioClips.Find (s => s.name == name);
        if (sfx == null)
        {
            return;
        }

        audioSource.PlayOneShot (sfx);
    }
}
