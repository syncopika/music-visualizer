using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioVisualizer : MonoBehaviour
{
    public GameObject particle;
    public AudioSource audioSrc;

    private const int sampleOutputDataSize = 512;    // power of 2
    private const float xCoordOutputData = -55.0f;   // x coord of particles
    private const float pointSpacing = 0.22f;        //Screen.width / spectrumDataSize; TODO: have it scale with screen size/camera?
    private const float zCoord = 3.0f;               // z coord of particles

    private float[] outputData;
    private List<GameObject> pointObjectsOutput;

    private float calculateRMS(float[] samples)
    {
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += (samples[i] * samples[i]);
        }
        return Mathf.Sqrt(sum / samples.Length);
    }

    private void prefill(float[] arr, float val)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = val;
        }
    }

    private void setupOutputDataPoints(List<GameObject> points)
    {
        float currPos = xCoordOutputData;
        for (int i = 0; i < sampleOutputDataSize; i++)
        {
            GameObject newPoint = Instantiate(particle, new Vector3(currPos, 0, zCoord), Quaternion.Euler(0, 0, 0));
            newPoint.name = ("outputPoint_" + i);
            newPoint.transform.localScale = new Vector3(0.2f, 1, 1);
            points.Add(newPoint);

            currPos += pointSpacing;
        }
    }

    // display volume when using samples from getOutputData
    private void displayVolumeinDb(float[] samples, List<GameObject> points, float xStart, float spacing)
    {
        float rootMeanSquare = calculateRMS(samples);
        float log = rootMeanSquare / 0.1f == 0 ? 0 : Mathf.Log10(rootMeanSquare / 0.1f);
        float newVol = 6f * log;
        float yIncrement = newVol / (samples.Length / 2);
		
        for(int i = 0; i < sampleOutputDataSize; i++)
        {
            if (i <= points.Count / 2)
            {
                points[i].transform.position = new Vector3(xStart, (float)newVol * ((i+1) * yIncrement), zCoord);
            }
            else
            {
                points[i].transform.position = new Vector3(xStart,  ((float)newVol * ((points.Count - i) * yIncrement)), zCoord);
            }
			
            xStart += spacing;
        }
    }

    // taking the output directly from getOutputData (which I think is just amplitude data?) and scaling it a bit to show a waveform based on volume
    private void displayWaveform(float[] samples, List<GameObject> points, float xStart, float spacing, string style)
    {
        for(int i = 0; i < sampleOutputDataSize; i++)
        {
            float sampleVal = samples[i] * 20f;

            if (style == "move")
            {
                points[i].transform.position = new Vector3(xStart, sampleVal, zCoord);
                xStart += spacing;
            }
            else if(style == "stretch")
            {
                points[i].transform.localScale = new Vector3(1, sampleVal, 1);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pointObjectsOutput = new List<GameObject>();
        outputData = new float[sampleOutputDataSize];
        setupOutputDataPoints(pointObjectsOutput);
    }
	
    // Update is called once per frame
    void Update()
    {
        AudioListener.GetOutputData(outputData, 0);
        //displayWaveform(outputData, pointObjectsOutput, xCoord, pointSpacing, "stretch");
        displayWaveform(outputData, pointObjectsOutput, xCoordOutputData, pointSpacing, "move");
    }
}
