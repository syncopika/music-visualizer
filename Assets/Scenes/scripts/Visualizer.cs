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

    private AudioSource audioSrc;
    private float[] audioData;
   // private float[] prevAudioData;   // keep track of previous spectrum data - really only important for dealing with spectrum data

    private List<GameObject> pointObjects;
    private List<bool> pointObjectsFlag; // keep track of which objects are scaling up based on spectrum data


    // provide utility functions


    // Start is called before the first frame update
    void Start()
    {
        // intialize stuff
        audioSrc = audioSrcParent.GetComponent<AudioSource>();
        audioData = new float[sampleDataSize];
        //prevAudioData = new float[sampleDataSize];
        pointObjects = new List<GameObject>();
        pointObjectsFlag = new List<bool>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
