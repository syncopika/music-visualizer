using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.PostProcessing;

public class bloomcontrol : MonoBehaviour
{
    public GameObject audioSrcParent;

    AudioSource audioSrc;
    UnityEngine.Rendering.PostProcessing.Bloom bloomEffect;

    float currTime; // in sec
    public float startTime; // in sec
    public float endTime;  // in sec

    // Start is called before the first frame update
    void Start()
    {
        // https://forum.unity.com/threads/how-do-you-edit-post-processing-through-script-in-the-latest-version-of-unity.936785/
        // https://answers.unity.com/questions/1675373/change-postprocessing-bloom-effect-in-script.html
        PostProcessVolume vol = GetComponent<PostProcessVolume>();
        if(vol.profile.TryGetSettings<UnityEngine.Rendering.PostProcessing.Bloom>(out var bloom))
        {
            Debug.Log(bloom.intensity.value);
            bloomEffect = bloom;
        }

        audioSrc = audioSrcParent.GetComponent<AudioSource>();
        currTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (bloomEffect)
        {
            bloomEffect.intensity.value = 30f;
        }*/

        // cheat a bit and set bloom intensity based on start and end times of the audio
        // TODO: base intensity on spectrum data? like if there's no audio, intensity should be 0
        currTime += Time.deltaTime;

        if (currTime <= startTime)
        {
            // lerp bloom intensity to 2
            bloomEffect.intensity.value = Mathf.Lerp(0, 2, currTime / startTime);
        }
        else if(endTime - currTime <= 5)
        {
            // lerp to 0 - not currently working I think but ¯\_(ツ)_/¯. at least the lerp to 2 seems fine.
            bloomEffect.intensity.value = Mathf.Lerp(2, 0, (endTime - currTime) / 5);
        }
    }
}
