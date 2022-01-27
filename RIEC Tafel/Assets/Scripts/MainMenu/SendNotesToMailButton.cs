using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class SendNotesToMailButton : MonoBehaviour
{
    private bool isCurrentlySendingMail = false;

    [SerializeField]
    private TMP_Text notesText = null;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(StartSentNotesToMail);
    }

    /// <summary>
    /// Start sending notes to email
    /// </summary>

    private void StartSentNotesToMail()
    {
        if (isCurrentlySendingMail || notesText.text == "") { return; }
        isCurrentlySendingMail = true;
        StartCoroutine(SentNotesToMail());
    }

    /// <summary>
    /// Send notes to the user email without rich text functions
    /// </summary>

    private IEnumerator SentNotesToMail()
    {
        WWWForm form = new WWWForm();

        string titleText = notesText.text.Split('>')[1];
        titleText = titleText.Split('<')[0];
        string mainText = notesText.text.Split(new string[] { "</b>" }, System.StringSplitOptions.None)[1];

        form.AddField("userID", LogInManager.userID);
        form.AddField("emailedNotes", titleText + mainText);
        UnityWebRequest www = UnityWebRequest.Post("https://riectafel.000webhostapp.com/sendnotestoemail.php", form);
        yield return www.SendWebRequest();

        if (www.downloadHandler.text[0] == '0')
        {
            print("Notes sent to mail!");
        }
        else
        {
            Debug.LogError("Error#" + www.downloadHandler.text);
        }

        isCurrentlySendingMail = false;
    }
}
