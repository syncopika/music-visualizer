using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftSpheresVisualization : VisualizerMultiple
{
    public bool placeInFrontOfCamera;
    //Camera camera; // TODO: remove? we always assume there's only one camera and it's the main cam
    public float distFromCamera;
    public Material shaderMaterial;

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

        // assign random locations to each sphere representing a freq bin
        for(int i = 0; i < numFreqBands; i++)
        {
            float yPos = UnityEngine.Random.Range(0f, 1f);
            float xPos = UnityEngine.Random.Range(0f, 1f);
            float zPos = 30f;

            Vector3 newPos = Camera.main.ViewportToWorldPoint(new Vector3(xPos, yPos, zPos));

            GameObject newSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newSphere.transform.position = newPos;
            newSphere.transform.rotation = UnityEngine.Random.rotation;
            newSphere.transform.parent = parent.transform;

            // TODO: add shader to newSphere + script
            newSphere.GetComponent<Renderer>().material = shaderMaterial;
            /* specify:
             *  float _Speed;
             *  float _Density;
             *  float _Strength;
             *  float _Brightness;
            */
            newSphere.GetComponent<Renderer>().material.SetFloat("_Speed", 1.8f);
            newSphere.GetComponent<Renderer>().material.SetFloat("_Density", 120f);
            newSphere.GetComponent<Renderer>().material.SetFloat("_Strength", 2f);
            newSphere.GetComponent<Renderer>().material.SetFloat("_Brightness", 1f);
            newSphere.GetComponent<Renderer>().material.SetVector("color", new Vector4(0.3f, 0.6f, 1, 0.8f));
            newSphere.GetComponent<Renderer>().material.SetVector("center", new Vector2(0.5f, 0.5f));

            objectsArray.Add(newSphere);
            isAnimatingArray.Add(false);
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
            float binValDelta = 90 * (spectrumData[i] - prevSpectrumData[i]);

            // TODO: using binValDelta, pass to currObj's shader
            // https://github.com/twostraws/ShaderKit/blob/main/Shaders/SHKCircleWaveBlended.fsh
            // https://answers.unity.com/questions/1409060/is-there-any-way-to-create-a-dynamic-material-when.html
            // https://answers.unity.com/questions/55402/how-to-pass-custom-variables-to-shaders.html

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
    }
}
