using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used for delaying audio playback from an Audio Source (to help with recordings that need audio when using the Unity Recorder)
// https://forum.unity.com/threads/recorder-missing-the-first-few-frames-of-audio-when-driven-by-timeline.1116967/

// place on object that has an audio source
// make sure audio source does not have "Play On Awake" checked
public class AudioScript : MonoBehaviour
{
    public float delay = 1.5f;
    void Awake()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.PlayDelayed(delay);
    }
}
