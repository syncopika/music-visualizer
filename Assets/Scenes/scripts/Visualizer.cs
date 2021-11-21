using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// parent class for visualizations
public class Visualizer : MonoBehaviour
{
    public GameObject particle; // the gameobject to use to represent a spectrum data point
    public GameObject audioSrcParent;
    public float lerpInterval = 0.05f; // time interval in sec for a data point object to scale towards a value
    public int sampleDataSize = 512;  // must be power of 2
    public int desiredFreqMin = 50;
    public int desiredFreqMax = 2000;

    protected AudioSource audioSrc;
    protected float[] audioData;
    // private float[] prevAudioData;   // keep track of previous spectrum data - really only important for dealing with spectrum data

    protected List<GameObject> pointObjects;
    protected List<bool> pointObjectsFlag; // keep track of which objects are scaling up based on audio data


    // provide utility functions

    // super helpful: https://www.youtube.com/watch?v=PzVbaaxgPco => Unity3D How To: Audio Visualizer With Spectrum Data
    protected IEnumerator scaleToTarget(GameObject obj, Vector3 target, int objIndex, Color minColor, Color maxColor)
    {
        Transform trans = obj.transform;
        Vector3 initialScale = trans.localScale;
        Vector3 currScale = trans.localScale;
        float timer = 0f;

        while (currScale != target)
        {
            currScale = Vector3.Lerp(initialScale, target, timer / lerpInterval);
            trans.localScale = currScale;
            timer += Time.deltaTime;

            obj.GetComponent<Renderer>().material.color = Color.Lerp(minColor, maxColor, timer / lerpInterval);

            yield return null;
        }
        pointObjectsFlag[objIndex] = false;
    }

    protected IEnumerator moveToTarget(GameObject obj, Vector3 target, int objIndex, Color minColor, Color maxColor)
    {
        Transform trans = obj.transform;
        Vector3 initialPos = trans.position;
        Vector3 currPos = trans.position;

        float timer = 0f;

        while (currPos != target)
        {
            currPos = Vector3.Lerp(initialPos, target, timer / lerpInterval);
            trans.position = currPos;
            timer += Time.deltaTime;

            obj.GetComponent<Renderer>().material.color = Color.Lerp(minColor, maxColor, timer / lerpInterval);

            yield return null;
        }
        pointObjectsFlag[objIndex] = false;
    }

    protected void prefill(float[] arr, float val)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = val;
        }
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        // intialize stuff
        audioSrc = audioSrcParent.GetComponent<AudioSource>();
        audioData = new float[sampleDataSize];
        //prevAudioData = new float[sampleDataSize];
        pointObjects = new List<GameObject>();
        pointObjectsFlag = new List<bool>();
    }
}
