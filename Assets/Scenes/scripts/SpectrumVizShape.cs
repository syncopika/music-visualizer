using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// visualize a single spectrum bin with a shape
public class SpectrumVizShape : VisualizerSingle
{
    private float[] spectrumData;

    public Vector3 scaleFrom;
    public Vector3 scaleTo;

    private void setupSpectrumDataPoint()
    {
        particle = Instantiate(particle, new Vector3(xCoord, yCoord, zCoord), Quaternion.Euler(0, 0, 0));
    }

    public void displaySpectrumPoint(float[] spectrumData)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2;                          // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.
        int targetIndex = (int)(desiredFreq / freqPerIndex);

        Transform currTransform = particle.transform;
        float binVal = spectrumData[targetIndex] * 200;

        // scale it based on spectrum bin value
        Color currColor = particle.GetComponent<Renderer>().material.color;

        if (binVal > 10 && !isAnimating)
        {
            Vector3 lerpTo = Vector3.Lerp(currTransform.localScale, scaleTo, (binVal / 10));
            isAnimating = true;
            StartCoroutine(
                scaleToTarget(particle, lerpTo, 0, currColor, currColor)
            );
        }

        // the target object will always scale down by default
        currTransform.localScale = Vector3.Lerp(currTransform.localScale, scaleFrom, 2 * Time.deltaTime);
    }
    public override void Start()
    {
        base.Start();
        spectrumData = audioData;
        setupSpectrumDataPoint();
    }

    void Update()
    {
        audioSrc.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        displaySpectrumPoint(spectrumData);
        particle.transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime * 20f);
    }
}
