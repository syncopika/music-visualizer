using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;

// this script is for recording the editor on play (but needs to be stopped manually to stop capturing frames)
// then you can use ffmpeg to combine the captured frames with audio to make a video.
// how to use: attach to the Main Camera of a scene

// potentially helpful
// https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html
// https://docs.unity3d.com/ScriptReference/Texture2D.ReadPixels.html
// https://www.reddit.com/r/Unity3D/comments/8oo6d6/waitforseconds_framerate_dependant/

public class Recorder : MonoBehaviour
{
    public float framesPerSec = 12f;
    public string folderName = "test";

    string dirPath = "";
    int snapshotCounter = 0;
    bool isCapturing = false;
    float intervalTime;

    public void Awake()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    private void LogPlayModeState(PlayModeStateChange state)
    {
        //Debug.Log(state);
        if (state.Equals(PlayModeStateChange.EnteredPlayMode)) //state.Equals(PlayModeStateChange.ExitingEditMode)
        {
            Debug.Log("i'm playing!");
            snapshotCounter = 0;
            isCapturing = true;
            intervalTime = 1 / framesPerSec; // e.g. if 12 fps, every ~0.08 sec we should take a new snapshot

            bool dirCreated = createNewDirectoryForSnapshots();
            if (dirCreated)
            {
                // start capturing
                StartCoroutine(takeSnapshots());
            }
        }
        else if(state.Equals(PlayModeStateChange.ExitingPlayMode))
        {
            Debug.Log("i'm done");

            // stop capturing
            isCapturing = false;
            StopAllCoroutines();
        }
    }

    bool createNewDirectoryForSnapshots()
    {
        try
        {
            Debug.Log(Application.dataPath);
            dirPath = Application.dataPath + "/../" + folderName;
            Directory.CreateDirectory(dirPath);
            return true;
        }
        catch(IOException exception)
        {
            Debug.Log(exception);
            return false;
        }
    }

    IEnumerator takeSnapshots()
    {
        while (isCapturing)
        {
            StartCoroutine(getSnapshot());
            yield return new WaitForSecondsRealtime(intervalTime);
        }
    }

    IEnumerator getSnapshot()
    {
        yield return new WaitForEndOfFrame();
        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0); // capture screen
        //tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(dirPath + "/" + snapshotCounter.ToString() + ".png", bytes);
        Destroy(tex);
        snapshotCounter++;
    }

}
