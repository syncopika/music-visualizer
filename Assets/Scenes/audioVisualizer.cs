using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioVisualizer : MonoBehaviour
{
    // https://medium.com/giant-scam/algorithmic-beat-mapping-in-unity-preprocessed-audio-analysis-d41c339c135a
    // https://forum.unity.com/threads/what-is-spectrum-data-audio-getspectrumdata.204060/
    // https://answers.unity.com/questions/157940/getoutputdata-and-getspectrumdata-they-represent-t.html
    // https://answers.unity.com/questions/472188/what-does-getoutputdata-sample-float-represent.html

    public GameObject particle;
    public AudioSource audioSrc;

    private int sampleDataSize = 256;

    private const float xCoord = -25.0f;
    private const float pointSpacing = 0.2f; //Screen.width / spectrumDataSize;

    private float[] outputData;
    private float[] spectrumData;

    private List<GameObject> pointObjectsOutput;
    private List<GameObject> pointObjectsSpectrum;

    private float calculateRMS(float[] samples)
    {
		float sum = 0f;
		for(int i = 0; i < samples.Length; i++){
            Debug.Log(samples[i]);
			sum += (samples[i] * samples[i]);
		}
        //Debug.Log(sum);
        return Mathf.Sqrt(sum / samples.Length);
	}

    // when using samples from getOutputData
    private void displayVolumeinDb(float[] samples, List<GameObject> points, float xStart, float spacing)
    {
        float rootMeanSquare = calculateRMS(samples);
        //Debug.Log(rootMeanSquare);

        float log = rootMeanSquare / 0.1f == 0 ? 0 : Mathf.Log10(rootMeanSquare / 0.1f);

        float newVol = 20 * log;
        foreach(GameObject pt in points)
        {
            pt.transform.position = new Vector3(xStart, (float)newVol, 2);
            xStart += spacing;
        }
    }

    private void displayWaveform(float[] samples, List<GameObject> points, float xStart, float spacing, string style)
    {
        for(int i = 0; i < points.Count; i++)
        {
            if (style == "move")
            {
                points[i].transform.position = new Vector3(xStart, samples[i] * 100, 2);
                xStart += spacing;
            }
            else if(style == "stretch")
            {
                points[i].transform.localScale = new Vector3(1, samples[i] * 20, 1);
            }
        }
    }

    // when using samples from getSpectrumData
    private void displaySpectrum(float[] samples, List<GameObject> points, string style)
    {
        float currPos = xCoord;
        for (int i = 1; i < samples.Length - 1; i++)
        {
            Vector3 v1 = new Vector3(currPos, Mathf.Log(samples[i - 1]) + 10, 2);
            Vector3 v2 = new Vector3(currPos + pointSpacing, Mathf.Log(samples[i]) + 10, 2);

            points[i - 1].transform.position = v1;
            points[i].transform.position = v2;

            Debug.Log(Mathf.Log(samples[i]));

            currPos += pointSpacing;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pointObjectsSpectrum = new List<GameObject>();
        pointObjectsOutput = new List<GameObject>();

        outputData = new float[sampleDataSize];
        spectrumData = new float[sampleDataSize];

        float currPos = xCoord;
        for(int i = 0; i < sampleDataSize - 1; i++)
        {
            GameObject newPoint = Instantiate(particle, new Vector3(currPos, 0, 2), Quaternion.Euler(0, 0, 0));
            pointObjectsOutput.Add(newPoint);

            //GameObject newPoint2 = Instantiate(particle, new Vector3(currPos, 0, 2), Quaternion.Euler(0, 0, 0));
            //pointObjectsSpectrum.Add(newPoint2);

            currPos += pointSpacing;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //AudioListener.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        //displaySpectrum(spectrumData, pointObjectsSpectrum, "");

        AudioListener.GetOutputData(outputData, 0);
        //displayWaveform(outputData, pointObjectsOutput, xCoord, pointSpacing, "stretch");
        displayWaveform(outputData, pointObjectsOutput, xCoord, pointSpacing, "move");
        //displayVolumeinDb(outputData, pointObjectsOutput, xCoord, pointSpacing);
    }
}
