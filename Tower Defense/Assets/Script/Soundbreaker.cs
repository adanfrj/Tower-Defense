using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soundbreaker : MonoBehaviour
{
    public static bool soundBreaker = false;
    public AudioSource audio;

    public void StopPlaying()
    {
        audio.Stop(); // or audio.Pause();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
