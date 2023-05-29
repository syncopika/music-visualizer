using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    public float delay = 1f;

    void Awake()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.PlayDelayed(delay); // add 1 sec delay before playing audio
    }
}
