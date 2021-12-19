using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// display some text
public class TextHandler : MonoBehaviour
{
    public string title;
    public Vector2 titlePos;

    public string composer;
    public Vector2 composerPos;

    public string arranger;
    public Vector2 arrangerPos;
    
    public string bpm;
    public Vector2 bpmPos;

    public string miscText;
    public Vector2 miscTextPos;

    public bool moveText;

    // font type, font size, color?
    //public int fontSize;

    private Dictionary<string, (GameObject, Vector2)> textUis;
    //private GameObject textUi;

    public Canvas canvas; // where the text should go

    // https://gamedev.stackexchange.com/questions/116177/how-to-dynamically-create-an-ui-text-object-in-unity-5
    // https://answers.unity.com/questions/1716863/dynamically-added-ui-text-is-not-displaying.html
    private GameObject createText(string text, Vector2 pos)
    {
        GameObject ui = new GameObject("textUI");
        ui.transform.SetParent(canvas.transform);

        RectTransform t = ui.AddComponent<RectTransform>();
        t.anchoredPosition = pos; //new Vector2(-500, 250);

        // allow text to be laid out horizontally as much as possible
        ContentSizeFitter csf = ui.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        Text tex = ui.AddComponent<Text>();
        tex.text = text;
        tex.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        tex.fontSize = 16;
        tex.color = Color.white;

        return ui;
    }

    // Start is called before the first frame update
    void Start()
    {
        textUis = new Dictionary<string, (GameObject, Vector2)>();
        
        if(title != "")
        {
            textUis["title"] = (createText(title, titlePos), titlePos); // maybe use 2nd part of tuple as the final position to animate to, if animation is desired?
        }

        if(composer != "")
        {
            textUis["composer"] = (createText(composer, composerPos), composerPos);
        }

        if (arranger != "")
        {
            textUis["arranger"] = (createText(arranger, arrangerPos), arrangerPos);
        }

        if (bpm != "")
        {
            textUis["bpm"] = (createText(bpm, bpmPos), bpmPos);
        }

        if (miscText != "")
        {
            textUis["miscText"] = (createText(miscText, miscTextPos), miscTextPos);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (KeyValuePair<string, (GameObject, Vector2)> kv in textUis)
        {
            // TODO: animate text
        }

        //Vector2 newPos = Vector2.Lerp(textUi.GetComponent<RectTransform>().anchoredPosition, new Vector2(550, 250), Time.deltaTime * 0.2f);
        //textUi.GetComponent<RectTransform>().anchoredPosition = newPos;
    }
}
