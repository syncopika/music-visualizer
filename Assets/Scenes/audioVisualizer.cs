using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioVisualizer : MonoBehaviour
{
    public GameObject particle;
    public GameObject particle2;
    public AudioSource audioSrc;

    private int sampleDataSize = 512;

    private const float xCoord = -55.0f; // x coord of particles
    private const float pointSpacing = 0.22f; //Screen.width / spectrumDataSize; TODO: have it scale with screen size/camera?
    private const float zCoord = 3.0f; // z coord of particles

    private float[] outputData;
    private float[] spectrumData;

    private List<GameObject> pointObjectsOutput;
    private List<GameObject> pointObjectsSpectrum;
    private List<GameObject> pointObjectsVolume;

    private float calculateRMS(float[] samples)
    {
		float sum = 0f;
		for(int i = 0; i < samples.Length; i++){
			sum += (samples[i] * samples[i]);
		}
        return Mathf.Sqrt(sum / samples.Length);
	}

    // display volume when using samples from getOutputData
    private void displayVolumeinDb(float[] samples, List<GameObject> points, float xStart, float spacing)
    {
        float rootMeanSquare = calculateRMS(samples);

        float log = rootMeanSquare / 0.1f == 0 ? 0 : Mathf.Log10(rootMeanSquare / 0.1f);

        float newVol = 6f * log;

        float yIncrement = newVol / (samples.Length / 2);

        for(int i = 0; i < sampleDataSize; i++)
        {
            if (i <= points.Count / 2)
            {
                points[i].transform.position = new Vector3(xStart, (float)newVol*((i+1)*yIncrement), zCoord);
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
        for(int i = 0; i < sampleDataSize; i++)
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

    // when using samples from getSpectrumData
    private void displaySpectrum(float[] samples, List<GameObject> points)
    {
        float currPos = xCoord;
        for (int i = 0; i < sampleDataSize; i++)
        {
            //Debug.Log(samples[i]);
            float log = samples[i] == 0 ? 0 : Mathf.Log10(samples[i]);
            points[i].transform.position = new Vector3(currPos, log, zCoord);
            currPos += pointSpacing;
        }
    }

    // select a range of frequencies to keep track of and how loud those frequencies are
    private void displaySpectrum2(float[] spectrumData, List<GameObject> points, float freqRangeMin, float freqRangeMax)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2; // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.

        int targetIndexMin = (int)(freqRangeMin / freqPerIndex);
        int targetIndexMax = (int)Math.Min(spectrumData.Length - 1, freqRangeMax / freqPerIndex);
        //Debug.Log("min: " + targetIndexMin + ", max: " + targetIndexMax);

        for(int i = targetIndexMin; i < targetIndexMax; i++)
        {
            //Debug.Log(Mathf.Log10(spectrumData[i]));
            points[i].transform.position = new Vector3(
                points[i].transform.position.x,
                -1*Mathf.Log10(spectrumData[i]),
                points[i].transform.position.z
            );
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        pointObjectsSpectrum = new List<GameObject>();
        pointObjectsOutput = new List<GameObject>();
        pointObjectsVolume = new List<GameObject>();

        outputData = new float[sampleDataSize];
        spectrumData = new float[sampleDataSize];

        float currPos = xCoord;
        for(int i = 0; i < sampleDataSize; i++)
        {
            GameObject newPoint = Instantiate(particle, new Vector3(currPos, 0, zCoord), Quaternion.Euler(0, 0, 0));
            newPoint.transform.localScale = new Vector3(0.2f, 1, 1);
            pointObjectsOutput.Add(newPoint);

            GameObject newPoint2 = Instantiate(particle2, new Vector3(currPos, 0, zCoord), Quaternion.Euler(0, 0, 0));
            pointObjectsSpectrum.Add(newPoint2);

            //GameObject newPoint3 = Instantiate(particle, new Vector3(currPos, 0, zCoord), Quaternion.Euler(0, 0, 0));
            //pointObjectsVolume.Add(newPoint3);

            currPos += pointSpacing;
        }
    }

    // Update is called once per frame
    void Update()
    {
        AudioListener.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        //displaySpectrum(spectrumData, pointObjectsSpectrum);
        displaySpectrum2(spectrumData, pointObjectsSpectrum, 50f, 24000f);

        AudioListener.GetOutputData(outputData, 0);
        //displayWaveform(outputData, pointObjectsOutput, xCoord, pointSpacing, "stretch");
        displayWaveform(outputData, pointObjectsOutput, xCoord, pointSpacing, "move");

        //displayVolumeinDb(outputData, pointObjectsVolume, xCoord, pointSpacing);
    }
}
