using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class RetrieveNotes : MonoBehaviour
{
    private Button button = null;

    [SerializeField]
    private GameObject savedNotesSet = null;

    [SerializeField]
    private TMP_Dropdown notesOptionDropdown = null;

    [SerializeField]
    private TMP_Text notesText = null;

    [System.NonSerialized]
    public List<string> allNotes = new List<string>();

    /// <summary>
    /// Start retrieving notes from database
    /// Only have button interacteble if the user succefully retrieved the notes
    /// </summary>

    private void Start()
    {
        notesOptionDropdown.ClearOptions();
        button = GetComponent<Button>();
        button.interactable = false;
        StartCoroutine(RetrieveAllNotes());
    }

    /// <summary>
    /// Retrieve notes from database, split them up based on each notes section and set them as options in the dropdown
    /// </summary>

    private IEnumerator RetrieveAllNotes()
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", LogInManager.userID);
        UnityWebRequest www = UnityWebRequest.Post("https://riectafel.000webhostapp.com/retrievenotes.php", form);
        yield return www.SendWebRequest();

        if (www.downloadHandler.text[0] == '0')
        {
            string allNotesTogether = www.downloadHandler.text.Remove(0, 1);
            string[] allNotes = allNotesTogether.Split(new string[] { "|endOfNotesSection|" }, System.StringSplitOptions.None);
            List<string> allOptions = new List<string>();

            for (int i = 0; i < allNotes.Length - 1; i++)
            {
                string option = allNotes[i].Split('>')[1];
                option = option.Split(':')[0];
                allOptions.Add(option);

                this.allNotes.Add(allNotes[i]);
            }

            // Activate notes selection UI
            notesOptionDropdown.AddOptions(allOptions);
            notesOptionDropdown.onValueChanged.AddListener(ShowNotesSet);
            button.onClick.AddListener(OpenSavedNotesSet);
            button.interactable = true;
            if (this.allNotes.Count > 0)
            {
                ShowNotesSet(0);
            }
        }
        else
        {
            Debug.LogError("Error#" + www.downloadHandler.text);
        }
    }

    /// <summary>
    /// Shows selected notes set
    /// </summary>

    public void ShowNotesSet(int value)
    {
        notesText.text = allNotes[value];
    }

    /// <summary>
    /// Sets notes text to empty in case there are no more notes to show
    /// </summary>

    public void SetTextToEmpty()
    {
        notesText.text = "";
    }

    /// <summary>
    /// Open the saved notes game object
    /// </summary>

    private void OpenSavedNotesSet()
    {
        savedNotesSet.SetActive(true);
    }
}
