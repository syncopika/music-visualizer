using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Starfield : VisualizerMultiple
{
    public Camera camera;
    public int numObjects;
    public int xRange;
    public int yRange;
    public int zRange;

    private float[] spectrumData;
    private float[] prevSpectrumData;   // keep track of previous spectrum data
    private GameObject parent;

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

        float angleIncrement = 360f / numFreqBands;

        System.Random rnd = new System.Random();

        for(int i = 0; i < numObjects; i++)
        {
            int binIndex = i % numFreqBands;
            int randX = rnd.Next((int)-xRange / 2, (int)xRange / 2);
            int randY = rnd.Next((int)-yRange / 2, (int)yRange / 2);
            int randZ = rnd.Next(-10, (int)zRange - 10);

            GameObject newPoint = Instantiate(particle, new Vector3(randX, randY, randZ), Quaternion.Euler(0, 0, 0));
            newPoint.name = ("spectrumPoint_" + i);

            float fraction = (float)i / numFreqBands;
            Renderer r = newPoint.GetComponent<Renderer>();
            Vector4 color = r.material.color;
            r.material.color = new Vector4(fraction * color.x, fraction * color.y, color.z, color.w);

            pointObjects.Add(newPoint);
            pointObjectsFlag.Add(false);
        }

    }

    public void displaySpectrum(float[] spectrumData)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2;                          // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.

        int targetIndexMin = (int)(desiredFreqMin / freqPerIndex);
        int targetIndexMax = (int)Math.Min(sampleDataSize - 1, desiredFreqMax / freqPerIndex);

        for (int i = 0; i < pointObjects.Count; i++)
        {
            Transform currTransform = pointObjects[i].transform;

            int binIndex = i % (targetIndexMax - targetIndexMin);
            float binValDelta = 90 * (spectrumData[binIndex] - prevSpectrumData[binIndex]);
            float baseline = 0.1f;

            // scale it based on spectrum bin value
            Color currColor = pointObjects[i].GetComponent<Renderer>().material.color;

            if (binValDelta > 0 && pointObjectsFlag[i] == false)
            {
                pointObjectsFlag[i] = true;
                StartCoroutine(
                    scaleToTarget(
                        pointObjects[i], 
                        new Vector3(binValDelta, pointObjects[i].transform.localScale.y, pointObjects[i].transform.localScale.z), 
                        i, 
                        currColor, 
                        currColor)
                );
            }

            // the target object will always scale down to 0.1,1,1 (baseline scale) by default
            currTransform.localScale = Vector3.Lerp(currTransform.localScale, new Vector3(baseline, 1, 1), 10 * Time.deltaTime);
        }

        spectrumData.CopyTo(prevSpectrumData, 0);
    }
    public override void Start()
    {
        base.Start();
        parent = new GameObject();
        spectrumData = audioData;
        prevSpectrumData = new float[sampleDataSize];
        prefill(prevSpectrumData, 0.0f);
        setupSpectrumDataPoints();
    }

    void Update()
    {
        audioSrc.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        displaySpectrum(spectrumData);
        //parent.transform.Rotate(new Vector3(0, 0, 1), Time.deltaTime * 20f);
        camera.transform.position += transform.forward * 0.01f;
    }
}
