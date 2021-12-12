using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// parent class for a visualization containing multiple elements/shapes
// e.g. a circle of balls, each representing a spectrum bin range
public class VisualizerMultiple : MonoBehaviour
{
    public GameObject particle; // the gameobject to use to represent a spectrum data point
    public GameObject audioSrcParent;

    public float lerpInterval = 0.05f; // time interval in sec for a data point object to scale towards a value
    public int sampleDataSize = 512;  // must be power of 2
    public int desiredFreqMin = 50;
    public int desiredFreqMax = 2000;

    // location of visualization
    public float xCoord = 0.0f;
    public float yCoord = 0.0f;
    public float zCoord = 0.0f;

    protected AudioSource audioSrc;
    protected float[] audioData;
    // private float[] prevAudioData;   // keep track of previous spectrum data - really only important for dealing with spectrum data

    protected List<GameObject> objectsArray;
    protected List<bool> isAnimatingArray; // keep track of which objects are scaling based on audio data
    
    // super helpful: https://www.youtube.com/watch?v=PzVbaaxgPco => Unity3D How To: Audio Visualizer With Spectrum Data
    protected IEnumerator scaleToTarget(GameObject obj, Vector3 target, int objIndex, Color minColor, Color maxColor)
    {
        Transform transform = obj.transform;
        Vector3 initialScale = transform.localScale;
        Vector3 currScale = transform.localScale;
        float timer = 0f;

        while (currScale != target)
        {
            currScale = Vector3.Lerp(initialScale, target, timer / lerpInterval);
            transform.localScale = currScale;
            timer += Time.deltaTime;

            obj.GetComponent<Renderer>().material.color = Color.Lerp(minColor, maxColor, timer / lerpInterval);

            yield return null;
        }
        isAnimatingArray[objIndex] = false;
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
        isAnimatingArray[objIndex] = false;
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
        objectsArray = new List<GameObject>();
        isAnimatingArray = new List<bool>();
    }
}
