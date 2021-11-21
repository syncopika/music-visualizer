using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualizer : Visualizer
{
    private const int sampleOutputDataSize = 512;    // power of 2
    private const float xCoordOutputData = -55.0f;   // x coord of particles
    private const float pointSpacing = 0.22f;        // Screen.width / spectrumDataSize; TODO: have it scale with screen size/camera?
    private const float zCoord = 3.0f;               // z coord of particles
    private float[] outputData;

    private float calculateRMS(float[] samples)
    {
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += (samples[i] * samples[i]);
        }
        return Mathf.Sqrt(sum / samples.Length);
    }

    private void setupOutputDataPoints(List<GameObject> points, List<bool> pointFlags)
    {
        float currPos = xCoordOutputData;
        for (int i = 0; i < sampleOutputDataSize; i++)
        {
            GameObject newPoint = Instantiate(particle, new Vector3(currPos, 0, zCoord), Quaternion.Euler(0, 0, 0));
            newPoint.name = ("outputPoint_" + i);
            newPoint.transform.localScale = new Vector3(0.2f, 1, 1);
            points.Add(newPoint);
            pointFlags.Add(false);

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
    private void displayWaveform(float[] samples, List<GameObject> points, List<bool> pointFlags, float xStart, float spacing, string style)
    {
        Color baseColor = new Color(142f / 255f, 248f / 255f, 50f / 255f);

        for (int i = 0; i < sampleOutputDataSize; i++)
        {
            float sampleVal = samples[i] * 20f;

            // set up colors to lerp
            // TODO: make the base color of the material a public var? or the material itself?
            Color currColor = points[i].GetComponent<Renderer>().material.color;
            float factor = sampleVal * 0.5f;
            Color maxColor = new Color((factor * 117f) / 255f, (250f * factor) / 255f, (2f) / 255f); // colors need to be between 0-1 for each channel! :/

            if (style == "move")
            {
                if (pointObjectsFlag[i] == false)
                {
                    pointObjectsFlag[i] = true;
                    StartCoroutine(
                        moveToTarget(points[i], new Vector3(xStart, sampleVal, zCoord), i, baseColor, maxColor)
                    );
                }
                points[i].transform.position = Vector3.Lerp(points[i].transform.position, new Vector3(xStart, 0, zCoord), 50 * Time.deltaTime);
                points[i].GetComponent<Renderer>().material.color = Color.Lerp(currColor, baseColor, 10 * Time.deltaTime); // lerp color back to baseline
                xStart += spacing;
            }
            else if(style == "stretch")
            {
                if (pointObjectsFlag[i] == false)
                {
                    pointObjectsFlag[i] = true;
                    StartCoroutine(
                        scaleToTarget(points[i], new Vector3(1, sampleVal, zCoord), i, baseColor, maxColor)
                    );
                }
                points[i].transform.localScale = Vector3.Lerp(points[i].transform.localScale, new Vector3(1, 0, zCoord), 50 * Time.deltaTime);
                points[i].GetComponent<Renderer>().material.color = Color.Lerp(currColor, baseColor, 10 * Time.deltaTime); // lerp color back to baseline
            }
        }
    }

    // Start is called before the first frame update
    // also, kinda random but pretty cool: https://stackoverflow.com/questions/42325033/how-do-the-unity-private-awake-update-and-start-methods-work
    public override void Start()
    {
        base.Start();
        outputData = audioData;
        setupOutputDataPoints(pointObjects, pointObjectsFlag);
    }
	
    // Update is called once per frame
    void Update()
    {
        audioSrc.GetOutputData(outputData, 0);
        //displayWaveform(outputData, pointObjects, pointObjectsFlag, xCoordOutputData, pointSpacing, "stretch");
        displayWaveform(outputData, pointObjects, pointObjectsFlag, xCoordOutputData, pointSpacing, "move");
    }
}
