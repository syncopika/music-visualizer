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
    public float objScaleFactor = 1f;

    private float[] spectrumData;
    private float[] prevSpectrumData;   // keep track of previous spectrum data
    
    private Vector3 initialObjectScale; // get the initial scale of the object that we're multiplying

    private void setupSpectrumDataPoints()
    {
        System.Random rnd = new System.Random();

        // instantiate objects and place them randomly with random rotations
        for(int i = 0; i < numObjects; i++)
        {
            int randX = rnd.Next((int)-xRange / 2, (int)xRange / 2);
            int randY = rnd.Next((int)-yRange / 2, (int)yRange / 2);
            int randZ = rnd.Next(-10, (int)zRange - 10);

            GameObject newPoint = Instantiate(particle, new Vector3(randX, randY, randZ), UnityEngine.Random.rotation);
            newPoint.name = ("spectrumPoint_" + i);
            
            Vector3 currScale = particle.transform.localScale;
            newPoint.transform.localScale = new Vector3(currScale.x * objScaleFactor, currScale.y * objScaleFactor, currScale.z * objScaleFactor);
            initialObjectScale = new Vector3(currScale.x * objScaleFactor, currScale.y * objScaleFactor, currScale.z * objScaleFactor);

            //float fraction = (float)i / numFreqBands;
            //Renderer r = newPoint.GetComponent<Renderer>();
            //Vector4 color = r.material.color;
            //r.material.color = new Vector4(fraction * color.x, fraction * color.y, color.z, color.w);

            objectsArray.Add(newPoint);
            isAnimatingArray.Add(false);
        }

    }

    public void displaySpectrum(float[] spectrumData)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2;                  // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.

        int targetIndexMin = (int)(desiredFreqMin / freqPerIndex);
        int targetIndexMax = (int)Math.Min(sampleDataSize - 1, desiredFreqMax / freqPerIndex);

        for (int i = 0; i < objectsArray.Count; i++)
        {
            GameObject currObj = objectsArray[i];
            Transform currTransform = currObj.transform;

            // map each object to a bin freq
            int binIndex = i % (targetIndexMax - targetIndexMin);
            float binValDelta = 160 * (spectrumData[binIndex] - prevSpectrumData[binIndex]);

            // can scale color also
            Color currColor = currObj.GetComponent<Renderer>().material.color;

            if (binValDelta > 5 && isAnimatingArray[i] == false)
            {
                isAnimatingArray[i] = true;
                
                StartCoroutine(
                    scaleToTarget(
                        currObj,
                        new Vector3(
                            Mathf.Min(initialObjectScale.x * 3, initialObjectScale.x * binValDelta), 
                            Mathf.Min(initialObjectScale.x * 3, initialObjectScale.y * binValDelta), 
                            Mathf.Min(initialObjectScale.x * 3, initialObjectScale.z * binValDelta)
                        ), 
                        i, 
                        currColor, 
                        currColor
                    )
                );
            }

            // the target object will always scale down to initial scale by default
            currTransform.localScale = Vector3.Lerp(currTransform.localScale, initialObjectScale, 10 * Time.deltaTime);
        }

        spectrumData.CopyTo(prevSpectrumData, 0);
    }
    
    public override void Start()
    {
        base.Start();
        spectrumData = audioData;
        prevSpectrumData = new float[sampleDataSize];
        prefill(prevSpectrumData, 0.0f);
        setupSpectrumDataPoints();
    }
    
    void rotateAll(){
        foreach(GameObject obj in objectsArray){
            obj.transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime * 20f);
        }
    }

    void Update()
    {
        audioSrc.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        displaySpectrum(spectrumData);
        rotateAll();
        camera.transform.Rotate(new Vector3(0, 0, -1), Time.deltaTime * 1f);
        camera.transform.position += transform.forward * 0.01f;
    }
}
