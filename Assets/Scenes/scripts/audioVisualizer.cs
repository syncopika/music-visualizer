using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioVisualizer : MonoBehaviour
{
    public GameObject particle;
    public GameObject particle2;
    public AudioSource audioSrc;

    private const int sampleOutputDataSize = 512;    // power of 2
    private const int sampleSpectrumDataSize = 512;  // power of 2
    private const float xCoordOutputData = -55.0f;   // x coord of particles
    private const float xCoordSpectrumData = -25.0f; // x coord of particles
    private const float pointSpacing = 0.22f;        //Screen.width / spectrumDataSize; TODO: have it scale with screen size/camera?
    private const float zCoord = 3.0f;               // z coord of particles
    private const int desiredFreqMin = 50;
    private const int desiredFreqMax = 2000;

    private float[] outputData;
    private float[] spectrumData;
    private float[] prevSpectrumData;

    private List<GameObject> pointObjectsOutput;
    private List<GameObject> pointObjectsSpectrum;
	
	// create only particles for the frequency range we're interested in
	private void setupSpectrumDataPoints(float[] spectrumData, List<GameObject> points, string style){
		int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2; // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleSpectrumDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.
		
		Debug.Log("sample rate: " + sampleRate);
		Debug.Log("max supported frequency in data: " + freqMax);
		Debug.Log("freq per bin: " + freqPerIndex);
		
        int targetIndexMin = (int)(desiredFreqMin / freqPerIndex);
        int targetIndexMax = (int)Math.Min(spectrumData.Length - 1, desiredFreqMax / freqPerIndex);
		int numFreqBands = targetIndexMax - targetIndexMin;
		
		if(style == "circle"){
			float radius = 12f;
			float currAngle = 0f;
			float angleIncrement = 360f / numFreqBands;
			
			for(int i = 0; i < numFreqBands; i++){
				// arrange in a circle
				float xCurr = radius*Mathf.Cos(currAngle*(float)(Math.PI / 180f)); // radians
				float yCurr = radius*Mathf.Sin(currAngle*(float)(Math.PI / 180f)); // radians
				
				GameObject newPoint = Instantiate(particle2, new Vector3(xCurr, yCurr, zCoord), Quaternion.Euler(0, 0, currAngle));
				newPoint.name = ("spectrumPoint_" + i);

                // change color for each freq band
                float fraction = (float)i / numFreqBands;
                Renderer r = newPoint.GetComponent<Renderer>();
                Vector4 color = r.material.color;
                r.material.color = new Vector4(fraction * color.x, fraction * color.y, color.z, color.w);

                points.Add(newPoint);

				currAngle += angleIncrement;
			}
		}else{
			float currPos = xCoordSpectrumData;
			
			for(int i = 0; i < numFreqBands; i++){
				GameObject newPoint = Instantiate(particle2, new Vector3(currPos, 0, zCoord), Quaternion.Euler(0, 0, 0));
				newPoint.name = ("spectrumPoint_" + i);
                
                // change color for each freq band
                float fraction = (float)i / numFreqBands;
                Renderer r = newPoint.GetComponent<Renderer>();
                Vector4 color = r.material.color;
                r.material.color = new Vector4(fraction * color.x, fraction * color.y, color.z, color.w);

                points.Add(newPoint);
				currPos += 1.1f;
			}
		}
	}

    private float calculateRMS(float[] samples)
    {
		float sum = 0f;
		for(int i = 0; i < samples.Length; i++){
			sum += (samples[i] * samples[i]);
		}
        return Mathf.Sqrt(sum / samples.Length);
	}

    // display volume when using samples from getOutputData
    private void displayVolumeinDb(float[] samples, List<GameObject> points, float xStart, float spacing)
    {
        float rootMeanSquare = calculateRMS(samples);
        float log = rootMeanSquare / 0.1f == 0 ? 0 : Mathf.Log10(rootMeanSquare / 0.1f);
        float newVol = 6f * log;
        float yIncrement = newVol / (samples.Length / 2);
		
        for(int i = 0; i < sampleOutputDataSize; i++)
        {
            if (i <= points.Count / 2)
            {
                points[i].transform.position = new Vector3(xStart, (float)newVol*((i+1)*yIncrement), zCoord);
            }
            else
            {
                points[i].transform.position = new Vector3(xStart,  ((float)newVol * ((points.Count - i) * yIncrement)), zCoord);
            }
			
            xStart += spacing;
        }
    }

    // taking the output directly from getOutputData (which I think is just amplitude data?) and scaling it a bit to show a waveform based on volume
    private void displayWaveform(float[] samples, List<GameObject> points, float xStart, float spacing, string style)
    {
        for(int i = 0; i < sampleOutputDataSize; i++)
        {
            float sampleVal = samples[i] * 20f;

            if (style == "move")
            {
                points[i].transform.position = new Vector3(xStart, sampleVal, zCoord);
                xStart += spacing;
            }
            else if(style == "stretch")
            {
                points[i].transform.localScale = new Vector3(1, sampleVal, 1);
            }
        }
    }

    // when using samples from getSpectrumData
    private void displaySpectrum(float[] samples, List<GameObject> points)
    {
        float currPos = xCoordSpectrumData;
        for (int i = 0; i < sampleSpectrumDataSize; i++)
        {
            //Debug.Log(samples[i]);
            float log = samples[i] == 0 ? 0 : Mathf.Log10(samples[i]);
            points[i].transform.position = new Vector3(currPos, log, zCoord);
            currPos += pointSpacing;
        }
    }

    // select a range of frequencies to keep track of and how loud those frequencies are
    private void displaySpectrum2(float[] spectrumData, List<GameObject> points, string style)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int freqMax = sampleRate / 2;                          // this is the max supported frequency in our data
        float freqPerIndex = freqMax / sampleSpectrumDataSize; // this is the frequency increment between indices of the data. e.g. data[0] = n Hz, data[1] = 2*n Hz, etc.

        int targetIndexMin = (int)(desiredFreqMin / freqPerIndex);
        int targetIndexMax = (int)Math.Min(spectrumData.Length - 1, desiredFreqMax / freqPerIndex);
		
		int particleIndex = 0;
        for(int i = targetIndexMin; i < targetIndexMax; i++)
        {
			Transform currTransform = points[particleIndex].transform;
            float binValDelta = Math.Abs( Math.Abs(Mathf.Log10(prevSpectrumData[i])) - Math.Abs(Mathf.Log10(spectrumData[i])) );
			
			// asve the current rotation
			Quaternion prevRot = currTransform.rotation;
			
			// set rotation to normal so we can scale along one axis properly
			currTransform.rotation = Quaternion.identity;

            // scale it
            if (style == "circle")
            {
                currTransform.localScale = new Vector3(
                    -2 * binValDelta,
                    1,
                    1
                );
            }
            else
            {
                currTransform.localScale = new Vector3(
                    1,
                    -2 * binValDelta,
                    1
                );
            }
			
			// put back the rotation
			currTransform.rotation = prevRot;
			
			particleIndex++;
			//Debug.Log("bin: " + i + ", value: " + Mathf.Log10(spectrumData[i]));
        }
		
		spectrumData.CopyTo(prevSpectrumData, 0);
    }
	
	private void prefill(float[] arr, float val){
		for(int i = 0; i < arr.Length; i++){
			arr[i] = val;
		}
	}

    // Start is called before the first frame update
    void Start()
    {
        pointObjectsSpectrum = new List<GameObject>();
        pointObjectsOutput = new List<GameObject>();

        outputData = new float[sampleOutputDataSize];
        spectrumData = new float[sampleSpectrumDataSize];
		prevSpectrumData = new float[sampleSpectrumDataSize];
		prefill(prevSpectrumData, 1.0f); // fill with 1 so we take Log10(1.0) initially and won't have issues (otherwise we'd do Log10(0) which would be a problem)
		
		// for output data
        float currPos = xCoordOutputData;
        for(int i = 0; i < sampleOutputDataSize; i++)
        {
            GameObject newPoint = Instantiate(particle, new Vector3(currPos, 0, zCoord), Quaternion.Euler(0, 0, 0));
            newPoint.name = ("outputPoint_" + i);
            newPoint.transform.localScale = new Vector3(0.2f, 1, 1);
            pointObjectsOutput.Add(newPoint);
			
            currPos += pointSpacing;
        }
		
		setupSpectrumDataPoints(spectrumData, pointObjectsSpectrum, "circle");
    }
	
    // Update is called once per frame
    void Update()
    {
        AudioListener.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        //displaySpectrum(spectrumData, pointObjectsSpectrum);
        displaySpectrum2(spectrumData, pointObjectsSpectrum, "circle");

        AudioListener.GetOutputData(outputData, 0);
        //displayWaveform(outputData, pointObjectsOutput, xCoord, pointSpacing, "stretch");
        displayWaveform(outputData, pointObjectsOutput, xCoordOutputData, pointSpacing, "move");
    }
}
