// script for creating a bunch of objects of a single kind that are placed randomly and move in a certain direction.
// the objects' scale can also be altered based on audio spectrum data if desired via a boolean flag.
// how to use: create an empty game object in the scene, attach this script to it and assign an object to use in the inspector.

using System;
using System.Collections.Generic;
using UnityEngine;

public enum MoveDirection
{
    Down,
    Up,
    Left,
    Right,
    Forward,
    Backward
}

public class MovingParticle : VisualizerMultiple
{
    public int numObjects;
    public int xRange;
    public int yRange;
    public int zRange;
    public bool applySpectrumData; // whether to adjust particle scale based on spectrum data
    public float objScaleFactor = 1f;
    public MoveDirection moveDir = MoveDirection.Down;

    private float[] spectrumData;
    private float[] prevSpectrumData;   // keep track of previous spectrum data

    private Vector3 initialObjectScale; // get the initial scale of the object that we're multiplying
    private Dictionary<string, Vector3> initialPositions = new Dictionary<string, Vector3>();

    private void setupSpectrumDataPoints()
    {
        System.Random rnd = new System.Random(Guid.NewGuid().GetHashCode());

        // instantiate objects and place them randomly with random rotations
        for (int i = 0; i < numObjects; i++)
        {
            int randX = rnd.Next((int)-xRange / 2, (int)xRange / 2);
            int randY = rnd.Next((int)-yRange / 2 + 10, (int)yRange / 2 + 10);
            int randZ = rnd.Next(-5, (int)zRange - 5);

            GameObject newPoint = Instantiate(particle, new Vector3(randX, randY, randZ), UnityEngine.Random.rotation);
            newPoint.name = ("spectrumPoint_" + i);
            initialPositions.Add(newPoint.name, newPoint.transform.position);

            Vector3 currScale = particle.transform.localScale;
            newPoint.transform.localScale = new Vector3(currScale.x * objScaleFactor, currScale.y * objScaleFactor, currScale.z * objScaleFactor);
            initialObjectScale = new Vector3(currScale.x * objScaleFactor, currScale.y * objScaleFactor, currScale.z * objScaleFactor);

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
                            Mathf.Min(initialObjectScale.x * 2, initialObjectScale.x * binValDelta), // ensure max scale is twice the original scale
                            Mathf.Min(initialObjectScale.x * 2, initialObjectScale.y * binValDelta),
                            Mathf.Min(initialObjectScale.x * 2, initialObjectScale.z * binValDelta)
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

    void rotateAll()
    {
        foreach (GameObject obj in objectsArray)
        {
            obj.transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime * 20f);
        }
    }

    void moveAll()
    {
        switch (moveDir)
        {
            case MoveDirection.Down:
                {
                    foreach (GameObject obj in objectsArray)
                    {
                        // TODO: be able to modify speed of movement
                        obj.transform.Translate(new Vector3(0, -0.01f, 0), Space.World); // Space.World is important here. otherwise it'll be based on the object's local axis (although that may be desirable sometimes?)

                        if (obj.transform.position.y < -20)
                        {
                            // fyi: https://answers.unity.com/questions/225729/gameobject-positionset-not-working.html
                            // transform.position returns a copy/value not a ref
                            Vector3 initial = initialPositions[obj.name];
                            obj.transform.position = new Vector3(initial.x, initial.y + 20, initial.z);
                        }
                    }
                }
                break;
            case MoveDirection.Up:
                {
                    foreach (GameObject obj in objectsArray)
                    {
                        obj.transform.Translate(new Vector3(0, 0.05f, 0), Space.World);

                        if (obj.transform.position.y > 15)
                        {
                            Vector3 initial = initialPositions[obj.name];
                            obj.transform.position = initial;
                        }
                    }
                }
                break;
            case MoveDirection.Backward:
                {
                    foreach (GameObject obj in objectsArray)
                    {
                        obj.transform.Translate(new Vector3(0, 0, -0.05f), Space.World);

                        if (obj.transform.position.z < -5)
                        {
                            Vector3 initial = initialPositions[obj.name];
                            obj.transform.position = new Vector3(initial.x, initial.y, initial.z + 10);
                        }
                    }
                }
                break;
            case MoveDirection.Forward:
                {
                    foreach (GameObject obj in objectsArray)
                    {
                        obj.transform.Translate(new Vector3(0, 0, 0.05f), Space.World);

                        if (obj.transform.position.z > 100)
                        {
                            Vector3 initial = initialPositions[obj.name];
                            obj.transform.position = new Vector3(initial.x, initial.y, initial.z - 10);
                        }
                    }
                }
                break;
            case MoveDirection.Left:
                {
                    // TODO
                }
                break;
            case MoveDirection.Right:
                {
                    // TODO
                }
                break;
            default:
                {
                    // nothing to do
                }
                break;
        }
    }

    void Update()
    {
        if (applySpectrumData)
        {
            audioSrc.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
            displaySpectrum(spectrumData);
        }
        rotateAll();
        moveAll();
    }
}
