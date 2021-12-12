using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// visualizer based on frequency
// this visualization consists of multiple objects in various configurations such as a circle or a line
public class SpectrumVisualizer : VisualizerMultiple
{
    private float[] spectrumData;
    private float[] prevSpectrumData;   // keep track of previous spectrum data
    private string visualizationStyle;
    private GameObject parent;

    private void setupSpectrumDataPoints()
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2; // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.

        //Debug.Log("sample rate: " + sampleRate);
        //Debug.Log("max supported frequency in data: " + freqMax);
        //Debug.Log("freq per bin: " + freqPerIndex);

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
                float xCurr = xCoord + radius * Mathf.Cos(currAngle * (float)(Math.PI / 180f)); // radians
                float yCurr = yCoord + radius * Mathf.Sin(currAngle * (float)(Math.PI / 180f));

                GameObject newPoint = Instantiate(particle, new Vector3(xCurr, yCurr, zCoord), Quaternion.Euler(0, 0, currAngle));
                newPoint.name = ("spectrumPoint_" + i);

                // change color for each freq band
                float fraction = (float)i / numFreqBands;
                Renderer r = newPoint.GetComponent<Renderer>();
                Vector4 color = r.material.color;
                r.material.color = new Vector4(fraction * color.x, fraction * color.y, color.z, color.w);

                newPoint.transform.parent = parent.transform;
                objectsArray.Add(newPoint);
                isAnimatingArray.Add(false); // for knowing if the object is currently scaling up to some value
                currAngle += angleIncrement;
            }
        }
        else
        {
            // linear arrangement
            //float zCoord = 1.0f;   // z coord of particles
            //float xCoord = -25.0f; // x coord of particles

            for (int i = 0; i < numFreqBands; i++)
            {
                GameObject newPoint = Instantiate(particle, new Vector3(xCoord, 0, zCoord), Quaternion.Euler(0, 0, 0));
                newPoint.name = ("spectrumPoint_" + i);

                // change color for each freq band
                float fraction = (float)i / numFreqBands;
                Renderer r = newPoint.GetComponent<Renderer>();
                Vector4 color = r.material.color;
                r.material.color = new Vector4(fraction * color.x, fraction * color.y, color.z, color.w);

                objectsArray.Add(newPoint);
                isAnimatingArray.Add(false);
                xCoord += 1.1f;
            }
        }
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
            GameObject currObj = objectsArray[particleIndex];
            Transform currTransform = currObj.transform;
            float binValDelta = 90*(spectrumData[i] - prevSpectrumData[i]);
            float baseline = 0.1f;

            // save the current rotation
            Quaternion prevRot = currTransform.rotation;

            // set rotation to normal so we can scale along one axis properly
            currTransform.rotation = Quaternion.identity;

            // scale it based on spectrum bin value
            Color currColor = currObj.GetComponent<Renderer>().material.color;

            if (visualizationStyle == "circle")
            {
                // circular pattern
                if (binValDelta > 0 && isAnimatingArray[particleIndex] == false)
                {
                    isAnimatingArray[particleIndex] = true;
                    StartCoroutine(
                        scaleToTarget(currObj, new Vector3(binValDelta, 1, 1), particleIndex, currColor, currColor)
                    );
                }

                // the target object will always scale down to 0.1,1,1 (baseline scale) by default
                currTransform.localScale = Vector3.Lerp(currTransform.localScale, new Vector3(baseline, 1, 1), 10 * Time.deltaTime);
            }
            else
            {
                // linear pattern
                if (binValDelta > 0 && isAnimatingArray[particleIndex] == false)
                {
                    isAnimatingArray[particleIndex] = true;
                    StartCoroutine(
                        scaleToTarget(currObj, new Vector3(binValDelta*0.5f, binValDelta, 1), particleIndex, currColor, currColor)
                    );
                }

                // the target object will always scale down to 1,0.1,1 (baseline scale) by default
                currTransform.localScale = Vector3.Lerp(currTransform.localScale, new Vector3(1, baseline, 1), 10 * Time.deltaTime);
            }

            // put back the rotation
            currTransform.rotation = prevRot;

            particleIndex++;
        }

        spectrumData.CopyTo(prevSpectrumData, 0);
    }
    public override void Start()
    {
        base.Start();
        parent = new GameObject();
        spectrumData = audioData;
        prevSpectrumData = new float[sampleDataSize];
        visualizationStyle = "circle";
        prefill(prevSpectrumData, 0.0f);
        setupSpectrumDataPoints();
    }

    void Update()
    {
        audioSrc.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        displaySpectrum(spectrumData);
        parent.transform.Rotate(new Vector3(0, 0, 1), Time.deltaTime * 20f);
        //parent.transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime * 20f);
    }
}
