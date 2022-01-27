using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class RemoveNotesButton : MeetingButton
{
    private bool isCurrentlyRemovingNotes = false;

    [SerializeField]
    private TMP_Dropdown notesOptionDropdown = null;

    private int currentlyChosenOption = 0;

    private RetrieveNotes retrieveNotes = null;

    [SerializeField]
    private Button yesButton = null;

    /// <summary>
    /// Initialize variables
    /// </summary>

    public override void Start()
    {
        base.Start();
        retrieveNotes = FindObjectOfType<RetrieveNotes>();
        yesButton.onClick.AddListener(StartRemoveNotes);
        notesOptionDropdown.onValueChanged.AddListener(ChangeCurrentlyChosenOption);
    }

    /// <summary>
    /// Select the current option to remove
    /// </summary>

    private void ChangeCurrentlyChosenOption(int value)
    {
        currentlyChosenOption = value;
    }

    /// <summary>
    /// Start removing the currently selected notes
    /// </summary>

    private void StartRemoveNotes()
    {
        if (isCurrentlyRemovingNotes || retrieveNotes.allNotes.Count == 0) { return; }
        isCurrentlyRemovingNotes = true;
        StartCoroutine(RemoveNotes());
    }

    /// <summary>
    /// Remove the currently selected notes by removing this option from the notes lists, then create new all notes text
    /// and update the database to that text
    /// </summary>

    private IEnumerator RemoveNotes()
    {
        WWWForm form = new WWWForm();

        retrieveNotes.allNotes.RemoveAt(currentlyChosenOption);
        notesOptionDropdown.options.RemoveAt(currentlyChosenOption);

        if (retrieveNotes.allNotes.Count > 0)
        {
            currentlyChosenOption = 0;
            notesOptionDropdown.value = currentlyChosenOption;
        } else
        {
            // Set text to empty string if no more notes are left
            notesOptionDropdown.transform.GetChild(0).GetComponent<TMP_Text>().text = "";
            retrieveNotes.SetTextToEmpty();
        }

        string newAllNotes = "";
        for (int i = 0; i < retrieveNotes.allNotes.Count; i++)
        {
            newAllNotes += retrieveNotes.allNotes[i] + "|endOfNotesSection|";
        }

        form.AddField("userID", LogInManager.userID);
        form.AddField("textToBeSaved", newAllNotes);
        UnityWebRequest www = UnityWebRequest.Post("https://riectafel.000webhostapp.com/removenotes.php", form);
        yield return www.SendWebRequest();

        if (www.downloadHandler.text[0] == '0')
        {
            print("Notes Removed!");
        }
        else
        {
            Debug.LogError("Error#" + www.downloadHandler.text);
        }

        isCurrentlyRemovingNotes = false;
    }
}
