using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// visualizer based on frequency (full spectrum)
// this visualization consists of multiple objects in various configurations such as a circle or a line
public class SpectrumVisualizer : VisualizerMultiple
{

    public enum VizStyles
    {
        Circle,
        Line
    };

    public enum VizOrientations
    {
        Vertical,
        Horizontal
    };

    public Camera camera;
    public float distFromCamera;
    public VizStyles visualizationStyle;
    public VizOrientations orientation; 
    public float circleRadius;        // if visualization style is circle
    public bool rotateY;

    private float[] spectrumData;
    private float[] prevSpectrumData;   // keep track of previous spectrum data
    private GameObject parent;

    private void setupSpectrumDataPoints()
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2; // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.

        int targetIndexMin = (int)(desiredFreqMin / freqPerIndex);
        int targetIndexMax = (int)Math.Min(sampleDataSize - 1, desiredFreqMax / freqPerIndex);
        int numFreqBands = targetIndexMax - targetIndexMin;

        if (visualizationStyle == VizStyles.Circle)
        {
            float radius = circleRadius;
            float currAngle = 0f;
            float angleIncrement = 360f / numFreqBands;

            for (int i = 0; i < numFreqBands; i++)
            {
                GameObject newPoint;
                float xCurr = xCoord + radius * Mathf.Cos(currAngle * (float)(Math.PI / 180f)); // radians
                if (orientation == VizOrientations.Horizontal)
                {
                    float zCurr = zCoord + radius * Mathf.Sin(currAngle * (float)(Math.PI / 180f));

                    // TODO: rotate the particle such that they are facing towards the center of the circle?
                    newPoint = Instantiate(particle, new Vector3(xCurr, yCoord, zCurr), Quaternion.Euler(0, 0, 0));
                }
                else
                {
                    float yCurr = yCoord + radius * Mathf.Sin(currAngle * (float)(Math.PI / 180f));
                    newPoint = Instantiate(particle, new Vector3(xCurr, yCurr, zCoord), Quaternion.Euler(0, 0, currAngle));
                }
                
                newPoint.name = ("spectrumPoint_" + i);

                // change color for each freq band
                float fraction = (float)i / numFreqBands;
                Renderer renderer = newPoint.GetComponent<Renderer>();
                Vector4 color = renderer.material.color;
                renderer.material.color = new Vector4(fraction * color.x, fraction * color.y, color.z, color.w);

                newPoint.transform.parent = parent.transform; // put these point objects under a GameObject parent

                objectsArray.Add(newPoint);
                isAnimatingArray.Add(false); // for knowing if the object is currently scaling up to some value

                currAngle += angleIncrement;
            }
        }
        else
        {
            // linear arrangement
            for (int i = 0; i < numFreqBands; i++)
            {
                GameObject newPoint = Instantiate(particle, new Vector3(xCoord, yCoord, zCoord), Quaternion.Euler(0, 0, 0));
                newPoint.name = ("spectrumPoint_" + i);

                // change color for each freq band
                float fraction = (float)i / numFreqBands;
                Renderer renderer = newPoint.GetComponent<Renderer>();
                Vector4 color = renderer.material.color;
                renderer.material.color = new Vector4(fraction * color.x, fraction * color.y, color.z, color.w);

                newPoint.transform.parent = parent.transform;

                objectsArray.Add(newPoint);
                isAnimatingArray.Add(false);

                xCoord += 1.8f;
            }
        }
    }

    public void displaySpectrum(float[] spectrumData)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2;                       // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleDataSize;      // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.

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

            if (visualizationStyle == VizStyles.Circle)
            {
                // circular pattern
                if (binValDelta > 0 && isAnimatingArray[particleIndex] == false)
                {
                    isAnimatingArray[particleIndex] = true;
                    if (orientation == VizOrientations.Horizontal)
                    {
                        StartCoroutine(
                            scaleToTarget(
                                currObj, 
                                new Vector3(
                                    currTransform.localScale.z, 
                                    binValDelta, 
                                    currTransform.localScale.z
                                ), 
                                particleIndex, 
                                currColor, 
                                currColor
                           )
                        );

                        // scale back down
                        currTransform.localScale = Vector3.Lerp(
                            currTransform.localScale, 
                            new Vector3(currTransform.localScale.x, baseline, currTransform.localScale.z), 
                            10 * Time.deltaTime
                        );
                    }
                    else
                    {
                        StartCoroutine(
                            scaleToTarget(
                                currObj, 
                                new Vector3(
                                    binValDelta,
                                    currTransform.localScale.y,
                                    currTransform.localScale.z
                                ), 
                                particleIndex, 
                                currColor, 
                                currColor
                            )
                        );

                        currTransform.localScale = Vector3.Lerp(
                            currTransform.localScale, 
                            new Vector3(baseline, currTransform.localScale.y, currTransform.localScale.z), 
                            10 * Time.deltaTime
                        );
                    }
                }
            }
            else
            {
                // linear pattern
                if (binValDelta > 0 && isAnimatingArray[particleIndex] == false)
                {
                    isAnimatingArray[particleIndex] = true;
                    StartCoroutine(
                        scaleToTarget(currObj, new Vector3(binValDelta*0.2f, binValDelta*1.1f, 1), particleIndex, currColor, currColor)
                    );
                }

                currTransform.localScale = Vector3.Lerp(currTransform.localScale, new Vector3(baseline, baseline, baseline), 10 * Time.deltaTime);
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
        prefill(prevSpectrumData, 0.0f);
        setupSpectrumDataPoints();
    }

    void Update()
    {
        audioSrc.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        displaySpectrum(spectrumData);

        if (camera)
        {
            Vector3 cameraPosition = camera.transform.position;
            parent.transform.position = cameraPosition + (camera.transform.forward * distFromCamera);

            if (visualizationStyle == VizStyles.Line) parent.transform.rotation = camera.transform.rotation;
        }

        if (rotateY) parent.transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime * 20f);
    }
}
