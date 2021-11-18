using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    public GameObject particle;
    public float lerpInterval = 0.05f;
    public GameObject audioSrcParent;

    private AudioSource audioSrc;

    private const int sampleOutputDataSize = 512;    // power of 2
    private const float xCoordOutputData = -55.0f;   // x coord of particles
    private const float pointSpacing = 0.22f;        // Screen.width / spectrumDataSize; TODO: have it scale with screen size/camera?
    private const float zCoord = 3.0f;               // z coord of particles

    private float[] outputData;
    private List<GameObject> pointObjectsOutput;
    private List<bool> pointObjectsFlag;

    private float calculateRMS(float[] samples)
    {
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += (samples[i] * samples[i]);
        }
        return Mathf.Sqrt(sum / samples.Length);
    }

    private void setupOutputDataPoints(List<GameObject> points, List<bool> pointFlags)
    {
        float currPos = xCoordOutputData;
        for (int i = 0; i < sampleOutputDataSize; i++)
        {
            GameObject newPoint = Instantiate(particle, new Vector3(currPos, 0, zCoord), Quaternion.Euler(0, 0, 0));
            newPoint.name = ("outputPoint_" + i);
            newPoint.transform.localScale = new Vector3(0.2f, 1, 1);
            points.Add(newPoint);
            pointFlags.Add(false);

            currPos += pointSpacing;
        }
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
                points[i].transform.position = new Vector3(xStart, (float)newVol * ((i+1) * yIncrement), zCoord);
            }
            else
            {
                points[i].transform.position = new Vector3(xStart,  ((float)newVol * ((points.Count - i) * yIncrement)), zCoord);
            }
			
            xStart += spacing;
        }
    }

    private IEnumerator scaleToTarget(GameObject obj, Vector3 target, int objIndex)
    {
        Transform trans = obj.transform;
        Vector3 initialScale = trans.localScale;
        Vector3 currScale = trans.localScale;

        // TODO: make this a public variable?
        Vector3 baseColor = new Vector3(142, 248, 50);
        Vector3 maxColor = new Vector3(178, 252, 114);

        float timer = 0f;

        while (currScale != target)
        {
            currScale = Vector3.Lerp(initialScale, target, timer / lerpInterval);

            Vector3 newColor = Vector3.Lerp(baseColor, maxColor, timer / lerpInterval);
            obj.GetComponent<Renderer>().material.color = new Color(newColor.x, newColor.y, newColor.z);

            trans.localScale = currScale;
            timer += Time.deltaTime;

            yield return null;
        }
        pointObjectsFlag[objIndex] = false;
    }

    private IEnumerator moveToTarget(GameObject obj, Vector3 target, int objIndex)
    {
        Transform trans = obj.transform;
        Vector3 initialPos = trans.position;
        Vector3 currPos = trans.position;

        // TODO: make the base color of the material a public var? or the material itself?
        float factor = target.y * 0.6f;
        Color maxColor = new Color((factor*117f) / 255f, (250f*factor) / 255f, (2f) / 255f); // colors need to be between 0-1 for each channel! :/

        float timer = 0f;

        while (currPos != target)
        {
            currPos = Vector3.Lerp(initialPos, target, timer / lerpInterval);

            Color currColor = obj.GetComponent<Renderer>().material.color;
            obj.GetComponent<Renderer>().material.color = Color.Lerp(currColor, maxColor, timer / lerpInterval);

            trans.position = currPos;
            timer += Time.deltaTime;

            yield return null;
        }
        pointObjectsFlag[objIndex] = false;
    }

    // taking the output directly from getOutputData (which I think is just amplitude data?) and scaling it a bit to show a waveform based on volume
    private void displayWaveform(float[] samples, List<GameObject> points, List<bool> pointFlags, float xStart, float spacing, string style)
    {
        Color baseColor = new Color(142f / 255f, 248f / 255f, 50f / 255f);

        for (int i = 0; i < sampleOutputDataSize; i++)
        {
            float sampleVal = samples[i] * 20f;

            if (style == "move")
            {
                if (pointObjectsFlag[i] == false)
                {
                    pointObjectsFlag[i] = true;
                    StartCoroutine(
                        moveToTarget(points[i], new Vector3(xStart, sampleVal, zCoord), i)
                    );
                }

                points[i].transform.position = Vector3.Lerp(points[i].transform.position, new Vector3(xStart, 0, zCoord), 50 * Time.deltaTime);

                Color currColor = points[i].GetComponent<Renderer>().material.color;
                points[i].GetComponent<Renderer>().material.color = Color.Lerp(currColor, baseColor, 10 * Time.deltaTime);

                xStart += spacing;
            }
            else if(style == "stretch")
            {
                if (pointObjectsFlag[i] == false)
                {
                    pointObjectsFlag[i] = true;
                    StartCoroutine(
                        scaleToTarget(points[i], new Vector3(1, sampleVal, zCoord), i)
                    );
                }
                points[i].transform.localScale = Vector3.Lerp(points[i].transform.localScale, new Vector3(1, 0, zCoord), 50 * Time.deltaTime);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSrc = audioSrcParent.GetComponent<AudioSource>();
        pointObjectsOutput = new List<GameObject>();
        pointObjectsFlag = new List<bool>();
        outputData = new float[sampleOutputDataSize];
        setupOutputDataPoints(pointObjectsOutput, pointObjectsFlag);
    }
	
    // Update is called once per frame
    void Update()
    {
        audioSrc.GetOutputData(outputData, 0);
        //displayWaveform(outputData, pointObjectsOutput, pointObjectsFlag, xCoordOutputData, pointSpacing, "stretch");
        displayWaveform(outputData, pointObjectsOutput, pointObjectsFlag, xCoordOutputData, pointSpacing, "move");
    }
}
