using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// visualize a single spectrum bin with a shape
public class SpectrumVizShape : Visualizer
{
    private float[] spectrumData;
    private float[] prevSpectrumData;   // keep track of previous spectrum data
    private string visualizationStyle;
    private GameObject shape;

    private void setupSpectrumDataPoint()
    {
        shape = Instantiate(particle, new Vector3(xCoord, yCoord, zCoord), Quaternion.Euler(0, 0, 0));
        pointObjectsFlag.Add(false);
    }

    public void displaySpectrumPoint(float[] spectrumData)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2;                          // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.
        int targetIndex = (int)(desiredFreqMin / freqPerIndex);

        Transform currTransform = shape.transform;
        float binVal = spectrumData[targetIndex] * 200;

        // scale it based on spectrum bin value
        Color currColor = shape.GetComponent<Renderer>().material.color;

        if (binVal > 10 && pointObjectsFlag[0] == false)
        {
            Vector3 lerpTo = Vector3.Lerp(currTransform.localScale, new Vector3(15, 15, 15), (binVal / 10));
            pointObjectsFlag[0] = true;
            StartCoroutine(
                scaleToTarget(shape, lerpTo, 0, currColor, currColor)
            );
        }

        // the target object will always scale down by default
        currTransform.localScale = Vector3.Lerp(currTransform.localScale, new Vector3(10, 10, 10), 2 * Time.deltaTime);

        //spectrumData.CopyTo(prevSpectrumData, 0);
    }
    public override void Start()
    {
        base.Start();
        spectrumData = audioData;
        //prevSpectrumData = new float[sampleDataSize];
        //prefill(prevSpectrumData, 0.0f);
        setupSpectrumDataPoint();
    }

    void Update()
    {
        audioSrc.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        displaySpectrumPoint(spectrumData);
        shape.transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime * 20f);
    }
}
