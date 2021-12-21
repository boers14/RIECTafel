using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TextSpeech;

public class SpeechTextSettings : MonoBehaviour
{
    [SerializeField]
    private string language = "nl-NL";

    private NotesText currentlyEditedText = null;

    private void Start()
    {
        SpeechToText.instance.Setting(language);
        SpeechToText.instance.onPartialResultsCallback = OnPartialListening;
        SpeechToText.instance.onResultCallback = OnEndListening;
    }

    private void OnEndListening(string result)
    {
        print(result);
        if (result == "Verwijder tekst")
        {
            FetchCurrentNotesText().text.text = "";
        } else if (result != "")
        {
            FetchCurrentNotesText().text.text += ". ";
        }
    }

    private void OnPartialListening(string result)
    {
        FetchCurrentNotesText().text.text += result;
    }

    private NotesText FetchCurrentNotesText()
    {
        if (!currentlyEditedText)
        {
            return FetchCurrentlyEditedNotesText();
        } else if (!currentlyEditedText.isCurrentlyEdited)
        {
            return FetchCurrentlyEditedNotesText();
        }

        return currentlyEditedText;
    }

    private NotesText FetchCurrentlyEditedNotesText()
    {
        List<NotesText> notesTexts = new List<NotesText>();
        notesTexts.AddRange(FindObjectsOfType<NotesText>());
        return notesTexts.Find(note => note.isCurrentlyEdited);
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
