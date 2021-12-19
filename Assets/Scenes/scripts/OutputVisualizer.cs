using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// visualizes the audio output data as a waveform (based on amplitude, or volume)
// this visualization consists of multiple objects that form the wave
public class OutputVisualizer : VisualizerMultiple
{
    private const int sampleOutputDataSize = 512;    // power of 2
    private const float pointSpacing = 0.22f;        // Screen.width / spectrumDataSize; TODO: have it scale with screen size/camera?
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

    private void setupOutputDataPoints(List<GameObject> objectsArray, List<bool> isAnimatingArray)
    {
        float currPos = xCoord;
        for (int i = 0; i < sampleOutputDataSize; i++)
        {
            GameObject newPoint = Instantiate(particle, new Vector3(currPos, yCoord, zCoord), Quaternion.Euler(0, 0, 0));
            newPoint.name = ("outputPoint_" + i);
            newPoint.transform.localScale = new Vector3(0.2f, 1, 1);
            objectsArray.Add(newPoint);
            isAnimatingArray.Add(false);

            currPos += pointSpacing;
        }
    }

    // display volume when using samples from getOutputData
    private void displayVolumeinDb(float[] samples, float xStart, float spacing)
    {
        float rootMeanSquare = calculateRMS(samples);
        float log = rootMeanSquare / 0.1f == 0 ? 0 : Mathf.Log10(rootMeanSquare / 0.1f);
        float newVol = 6f * log;
        float yIncrement = newVol / (samples.Length / 2);
		
        for(int i = 0; i < sampleOutputDataSize; i++)
        {
            if (i <= objectsArray.Count / 2)
            {
                objectsArray[i].transform.position = new Vector3(xStart, (float)newVol * ((i+1) * yIncrement), zCoord);
            }
            else
            {
                objectsArray[i].transform.position = new Vector3(xStart,  ((float)newVol * ((objectsArray.Count - i) * yIncrement)), zCoord);
            }
			
            xStart += spacing;
        }
    }

    // taking the output directly from getOutputData (which I think is just amplitude data?) and scaling it a bit to show a waveform based on volume
    private void displayWaveform(float[] samples, float spacing, string style)
    {
        float xStart = xCoord;

        Color baseColor = new Color(142f / 255f, 248f / 255f, 50f / 255f);

        for (int i = 0; i < sampleOutputDataSize; i++)
        {
            GameObject currObj = objectsArray[i];
            float sampleVal = samples[i] * 20f;

            // set up colors to lerp
            // TODO: make the base color of the material a public var? or the material itself?
            Color currColor = currObj.GetComponent<Renderer>().material.color;
            float factor = sampleVal * 0.5f;
            Color maxColor = new Color((factor * 117f) / 255f, (250f * factor) / 255f, (2f) / 255f); // colors need to be between 0-1 for each channel! :/

            if (style == "move")
            {
                if (isAnimatingArray[i] == false)
                {
                    isAnimatingArray[i] = true;
                    StartCoroutine(
                        moveToTarget(currObj, new Vector3(xStart, sampleVal + yCoord, zCoord), i, baseColor, maxColor)
                    );
                }
                currObj.transform.position = Vector3.Lerp(currObj.transform.position, new Vector3(xStart, yCoord, zCoord), 50 * Time.deltaTime);
                currObj.GetComponent<Renderer>().material.color = Color.Lerp(currColor, baseColor, 10 * Time.deltaTime); // lerp color back to baseline
                xStart += spacing;
            }
            else if(style == "stretch")
            {
                if (isAnimatingArray[i] == false)
                {
                    isAnimatingArray[i] = true;
                    StartCoroutine(
                        scaleToTarget(currObj, new Vector3(1, sampleVal, zCoord), i, baseColor, maxColor)
                    );
                }
                currObj.transform.localScale = Vector3.Lerp(currObj.transform.localScale, new Vector3(1, 0, zCoord), 50 * Time.deltaTime);
                currObj.GetComponent<Renderer>().material.color = Color.Lerp(currColor, baseColor, 10 * Time.deltaTime); // lerp color back to baseline
            }
        }
    }

    // Start is called before the first frame update
    // also, kinda random but pretty cool: https://stackoverflow.com/questions/42325033/how-do-the-unity-private-awake-update-and-start-methods-work
    public override void Start()
    {
        base.Start();
        outputData = audioData;
        setupOutputDataPoints(objectsArray, isAnimatingArray);
    }
	
    // Update is called once per frame
    void Update()
    {
        audioSrc.GetOutputData(outputData, 0);
        //displayWaveform(outputData, pointSpacing, "stretch");
        displayWaveform(outputData, pointSpacing, "move");
    }
}
