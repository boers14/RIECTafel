using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NotesSaver : MonoBehaviour
{
    private string textToBeSaved = "";

    public static NotesSaver instance = null;

    private bool initialLoad = true;

    [System.NonSerialized]
    public List<StickyNote> allStickyNotes = new List<StickyNote>();

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetTextToBeSaved()
    {
        textToBeSaved = "";
        for (int i = 0; i < allStickyNotes.Count; i++)
        {
            string addedText = "";
            for (int j = 0; j < allStickyNotes[i].texts.Count; j++)
            {
                addedText += allStickyNotes[i].texts[j].text.text;
                if (j < allStickyNotes[i].texts.Count - 1)
                {
                    addedText += ":\n";
                } else if (i < allStickyNotes.Count - 1)
                {
                    addedText += "\n\n";
                }
            }
            textToBeSaved += addedText;
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        if (initialLoad) { initialLoad = false; return; }

        if (textToBeSaved == "" || textToBeSaved == "|") { Destroy(gameObject); return; }

        int blankCounter = 0;
        foreach (char c in textToBeSaved)
        {
            if (char.IsWhiteSpace(c))
            {
                blankCounter++;
            }
        }

        if (blankCounter == textToBeSaved.Length || blankCounter == textToBeSaved.Length - 1 && textToBeSaved.EndsWith("|"))
        {
            Destroy(gameObject);
            return;
        }

        if (textToBeSaved.EndsWith("|"))
        {
            textToBeSaved = textToBeSaved.Remove(textToBeSaved.Length - 1);
        }

        System.DateTime currentDate = System.DateTime.Now;
        StartCoroutine(SaveNotes(currentDate.ToString().Split(' ')[0]));
    }

    private IEnumerator SaveNotes(string dateTimeString)
    {
        WWWForm form = new WWWForm();
        form.AddField("textToBeSaved", textToBeSaved);
        form.AddField("dateTimeString", dateTimeString);
        form.AddField("userID", LogInManager.userID);
        UnityWebRequest www = UnityWebRequest.Post("https://riectafel.000webhostapp.com/savenotes.php", form);
        yield return www.SendWebRequest();

        if (www.downloadHandler.text[0] == '0')
        {
            print("Notes saved!");
        }
        else
        {
            Debug.LogError("Error#" + www.downloadHandler.text);
        }

        Destroy(gameObject);
    }
}
