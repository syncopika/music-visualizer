using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RipplesVisualization : VisualizerMultiple
{
    public bool placeInFrontOfCamera;
    //Camera camera; // TODO: remove? we always assume there's only one camera and it's the main cam
    public float distFromCamera;
    public Material shaderMaterial;

    private float[] spectrumData;
    private float[] prevSpectrumData;   // keep track of previous spectrum data
    private GameObject parent;

    private float timeInterval = 0.1f; // wait n sec before updating visualization
    private float timeElapsed = 0f;
    private bool started = false;
    private void setupSpectrumDataPoints()
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2; // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.

        int targetIndexMin = (int)(desiredFreqMin / freqPerIndex);
        int targetIndexMax = (int)Math.Min(sampleDataSize - 1, desiredFreqMax / freqPerIndex);
        int numFreqBands = targetIndexMax - targetIndexMin;

        // assign random locations to each sphere representing a freq bin
        //float xInterval = 1f/numFreqBands + 0.01f;
        //float xStart = 0f;
        for(int i = 0; i < numFreqBands; i++)
        {
            float yPos = UnityEngine.Random.Range(0f, 1f);
            float xPos = UnityEngine.Random.Range(0f, 1f);
            float zPos = 60f;

            //xStart += xInterval;

            Vector3 newPos = Camera.main.ViewportToWorldPoint(new Vector3(xPos, yPos, zPos));

            GameObject newRipple = GameObject.CreatePrimitive(PrimitiveType.Plane);
            newRipple.transform.position = newPos;
            newRipple.transform.Rotate(new Vector3(-90, 0, 0));
            //newRipple.transform.rotation = UnityEngine.Random.rotation;
            newRipple.transform.parent = parent.transform;

            newRipple.GetComponent<Renderer>().material = shaderMaterial;
            newRipple.GetComponent<Renderer>().material.SetFloat("_Speed", 1.1f);
            newRipple.GetComponent<Renderer>().material.SetFloat("_Density", 60f);
            newRipple.GetComponent<Renderer>().material.SetFloat("_Strength", 2f);
            newRipple.GetComponent<Renderer>().material.SetFloat("_Brightness", 1f);
            newRipple.GetComponent<Renderer>().material.SetVector("color", new Vector4(0.3f, 0.6f, 1, 0.8f));
            newRipple.GetComponent<Renderer>().material.SetVector("center", new Vector2(0.5f, 0.5f));

            objectsArray.Add(newRipple);
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

            currTransform.GetComponent<Renderer>().material.SetFloat("freqBinDelta", binValDelta);

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
        if (timeElapsed >= timeInterval || !started)
        {
            audioSrc.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
            displaySpectrum(spectrumData);
            timeElapsed = 0f;
            started = true;
        }

        timeElapsed += Time.deltaTime;
    }
}
