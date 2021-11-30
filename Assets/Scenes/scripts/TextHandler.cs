using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// display some text
public class TextHandler : MonoBehaviour
{
    public string title;
    public string composer;
    public string arranger;
    public string bpm;

    // font type, font size, color?
    public int fontSize;

    private GameObject textUi;

    public Canvas canvas; // where the text should go

    // https://gamedev.stackexchange.com/questions/116177/how-to-dynamically-create-an-ui-text-object-in-unity-5
    // https://answers.unity.com/questions/1716863/dynamically-added-ui-text-is-not-displaying.html
    private GameObject createText(string text)
    {
        GameObject ui = new GameObject("textUI");
        ui.transform.SetParent(canvas.transform);

        RectTransform t = ui.AddComponent<RectTransform>();
        t.anchoredPosition = new Vector2(-500, 250);

        Text tex = ui.AddComponent<Text>();
        tex.text = text;
        tex.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        tex.fontSize = 18;
        tex.color = Color.white;

        return ui;
    }

    // Start is called before the first frame update
    void Start()
    {
        textUi = createText(title);

        Debug.Log("hello from text handler");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 newPos = Vector2.Lerp(textUi.GetComponent<RectTransform>().anchoredPosition, new Vector2(550, 250), Time.deltaTime * 0.2f);
        textUi.GetComponent<RectTransform>().anchoredPosition = newPos;
        //Debug.Log(newPos);
    }
}
