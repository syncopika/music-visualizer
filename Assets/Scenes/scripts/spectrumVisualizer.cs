using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectrumVisualizer : MonoBehaviour
{
    public GameObject particle; // the gameobject to use to represent a spectrum data point
    public float lerpInterval = 0.05f; // time interval in sec for a data point object to scale towards a value
    public GameObject audioSrcParent;

    private AudioSource audioSrc;

    private const int sampleDataSize = 512;  // power of 2
    private const int desiredFreqMin = 50;
    private const int desiredFreqMax = 2000;

    private float[] spectrumData;
    private float[] prevSpectrumData;   // keep track of previous spectrum data

    private List<GameObject> pointObjects;
    private List<bool> pointObjectsFlag; // keep track of which objects are scaling up based on spectrum data
    private string visualizationStyle;

    private void prefill(float[] arr, float val)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = val;
        }
    }

    private void setupSpectrumDataPoints()
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2; // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.

        Debug.Log("sample rate: " + sampleRate);
        Debug.Log("max supported frequency in data: " + freqMax);
        Debug.Log("freq per bin: " + freqPerIndex);

        int targetIndexMin = (int)(desiredFreqMin / freqPerIndex);
        int targetIndexMax = (int)Math.Min(sampleDataSize - 1, desiredFreqMax / freqPerIndex);
        int numFreqBands = targetIndexMax - targetIndexMin;

        if (visualizationStyle == "circle")
        {
            float radius = 15f;
            float currAngle = 0f;
            float angleIncrement = 360f / numFreqBands;

            for (int i = 0; i < numFreqBands; i++)
            {
                // arrange in a circle
                float xCurr = radius * Mathf.Cos(currAngle * (float)(Math.PI / 180f)); // radians
                float yCurr = radius * Mathf.Sin(currAngle * (float)(Math.PI / 180f));
                float zCoord = 3.0f;   // z coord of particles

                GameObject newPoint = Instantiate(particle, new Vector3(xCurr, yCurr, zCoord), Quaternion.Euler(0, 0, currAngle));
                newPoint.name = ("spectrumPoint_" + i);

                // change color for each freq band
                float fraction = (float)i / numFreqBands;
                Renderer r = newPoint.GetComponent<Renderer>();
                Vector4 color = r.material.color;
                r.material.color = new Vector4(fraction * color.x, fraction * color.y, color.z, color.w);

                pointObjects.Add(newPoint);
                pointObjectsFlag.Add(false); // for knowing if the object is currently scaling up to some value
                currAngle += angleIncrement;
            }
        }
        else
        {
            // linear arrangement
            float zCoord = 1.0f;   // z coord of particles
            float xCoord = -25.0f; // x coord of particles

            for (int i = 0; i < numFreqBands; i++)
            {
                GameObject newPoint = Instantiate(particle, new Vector3(xCoord, 0, zCoord), Quaternion.Euler(0, 0, 0));
                newPoint.name = ("spectrumPoint_" + i);

                // change color for each freq band
                float fraction = (float)i / numFreqBands;
                Renderer r = newPoint.GetComponent<Renderer>();
                Vector4 color = r.material.color;
                r.material.color = new Vector4(fraction * color.x, fraction * color.y, color.z, color.w);

                pointObjects.Add(newPoint);
                pointObjectsFlag.Add(false);
                xCoord += 1.1f;
            }
        }
    }

    // super helpful: https://www.youtube.com/watch?v=PzVbaaxgPco => Unity3D How To: Audio Visualizer With Spectrum Data
    private IEnumerator scaleToTarget(GameObject obj, Vector3 target, int objIndex)
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

            yield return null;
        }
        pointObjectsFlag[objIndex] = false;
    }

    public void displaySpectrum(float[] spectrumData)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2;                          // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.

        int targetIndexMin = (int)(desiredFreqMin / freqPerIndex);
        int targetIndexMax = (int)Math.Min(sampleDataSize - 1, desiredFreqMax / freqPerIndex);

        int particleIndex = 0;
        for (int i = targetIndexMin; i < targetIndexMax; i++)
        {
            Transform currTransform = pointObjects[particleIndex].transform;
            float binValDelta = 90*(spectrumData[i] - prevSpectrumData[i]);
            float baseline = 0.1f;

            // save the current rotation
            Quaternion prevRot = currTransform.rotation;

            // set rotation to normal so we can scale along one axis properly
            currTransform.rotation = Quaternion.identity;

            // scale it
            if (visualizationStyle == "circle")
            {
                // circular pattern
                if (binValDelta > 0 && pointObjectsFlag[particleIndex] == false)
                {
                    pointObjectsFlag[particleIndex] = true;
                    StartCoroutine(
                        scaleToTarget(pointObjects[particleIndex], new Vector3(binValDelta, 1, 1), particleIndex)
                    );
                }

                // the target object will always scale down to 0.1,1,1 (baseline scale) by default
                currTransform.localScale = Vector3.Lerp(currTransform.localScale, new Vector3(baseline, 1, 1), 10 * Time.deltaTime);
            }
            else
            {
                // linear pattern
                if (binValDelta > 0 && pointObjectsFlag[particleIndex] == false)
                {
                    pointObjectsFlag[particleIndex] = true;
                    StartCoroutine(
                        scaleToTarget(pointObjects[particleIndex], new Vector3(binValDelta*0.5f, binValDelta, 1), particleIndex)
                    );
                }

                // the target object will always scale down to 0.1,1,1 (baseline scale) by default
                currTransform.localScale = Vector3.Lerp(currTransform.localScale, new Vector3(1, baseline, 1), 10 * Time.deltaTime);
            }

            // put back the rotation
            currTransform.rotation = prevRot;

            particleIndex++;
            //Debug.Log("bin: " + i + ", value: " + Mathf.Log10(spectrumData[i]));
        }

        spectrumData.CopyTo(prevSpectrumData, 0);
    }
    void Start()
    {
        audioSrc = audioSrcParent.GetComponent<AudioSource>();
        spectrumData = new float[sampleDataSize];
        prevSpectrumData = new float[sampleDataSize];

        visualizationStyle = "circle";
        
        pointObjects = new List<GameObject>();
        pointObjectsFlag = new List<bool>();

        prefill(prevSpectrumData, 1.0f); // fill with 1 so we take Log10(1.0) initially and won't have issues (otherwise we'd do Log10(0) which would be a problem)

        setupSpectrumDataPoints();
    }

    void Update()
    {
        audioSrc.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        displaySpectrum(spectrumData);
    }
}
