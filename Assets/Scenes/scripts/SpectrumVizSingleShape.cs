using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// visualize a single spectrum bin (frequency) with a shape by scaling size (or color)
public class SpectrumVizSingleShape : VisualizerSingle
{
    private float[] spectrumData;
    private Color baseColor;

    public Vector3 scaleFrom; // baseline scale
    public Vector3 scaleTo; // maximum size to scale to
    public Color targetColor;

    public bool scaleToColorOnly;
    public bool rotate;

    private void setupSpectrumDataPoint()
    {
        //particle = Instantiate(particle, new Vector3(xCoord, yCoord, zCoord), Quaternion.Euler(0, 0, 0));
        Color currColor = transform.GetComponent<Renderer>().material.color;
        baseColor = new Color(currColor.r, currColor.g, currColor.b);
    }

    public void displaySpectrumPoint(float[] spectrumData)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2;                          // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.
        int targetIndex = (int)(desiredFreq / freqPerIndex);
        float binVal = spectrumData[targetIndex] * 200;

        // scale it based on spectrum bin value
        Color currColor = transform.GetComponent<Renderer>().material.color;

        if (binVal > 10 && !isAnimating)
        {
            if (!scaleToColorOnly)
            {
                Vector3 lerpTo = Vector3.Lerp(transform.localScale, scaleTo, (binVal / 10));
                isAnimating = true;
                StartCoroutine(
                    scaleToTarget(transform.gameObject, lerpTo, currColor, currColor)
                );
            }
            else
            {
                // scale to color only
                Color endColor = targetColor * (10 / binVal);
                StartCoroutine(
                    scaleToColor(transform.gameObject, endColor)
                );
            }
        }

        // the target object will always scale down by default
        transform.localScale = Vector3.Lerp(transform.localScale, scaleFrom, 2 * Time.deltaTime);

        if (scaleToColorOnly)
        {
            transform.GetComponent<Renderer>().material.color = Color.Lerp(currColor, baseColor, Time.deltaTime);
        }
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
        if(rotate) transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime * 20f);
    }
}
