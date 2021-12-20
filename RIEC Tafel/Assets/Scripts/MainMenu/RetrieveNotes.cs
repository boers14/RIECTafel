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

    private void Start()
    {
        notesOptionDropdown.ClearOptions();
        button = GetComponent<Button>();
        StartCoroutine(RetrieveAllNotes());
    }

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

            notesOptionDropdown.AddOptions(allOptions);
            notesOptionDropdown.onValueChanged.AddListener(ShowNotesSet);
            button.onClick.AddListener(OpenSavedNotesSet);
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

    public void ShowNotesSet(int value)
    {
        notesText.text = allNotes[value];
    }

    public void SetTextToEmpty()
    {
        notesText.text = "";
    }

    private void OpenSavedNotesSet()
    {
        savedNotesSet.SetActive(true);
    }
}
