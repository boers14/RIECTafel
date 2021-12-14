using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class EmailCheckButton : SwitchSceneButton
{
    [SerializeField]
    private TMP_InputField pincodeField = null;

    [SerializeField]
    private TMP_Text disclaimerText = null;

    [SerializeField]
    private string failedPincodeScene = "";

    private bool isCheckingPincode = false;

    public override void SwitchScene()
    {
        if (isCheckingPincode) { return; }
        isCheckingPincode = true;
        if (pincodeField.text == LogInManager.pincode && LogInManager.pincode != null)
        {
            StartCoroutine(SentUserToMainMenu());
        } else
        {
            StartCoroutine(CountDownAmountOfTries());
        }
    }

    private IEnumerator SentUserToMainMenu()
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", LogInManager.userID);

        UnityWebRequest www = UnityWebRequest.Post("https://riectafel.000webhostapp.com/resetlockouttries.php", form);
        yield return www.SendWebRequest();

        isCheckingPincode = false;
        if (www.downloadHandler.text[0] == '0')
        {
            base.SwitchScene();
        } else
        {
            Debug.LogError("Sent user to main menu failed. Error#" + www.downloadHandler.text);
        }
    }

    private IEnumerator CountDownAmountOfTries()
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", LogInManager.userID);

        UnityWebRequest www = UnityWebRequest.Post("https://riectafel.000webhostapp.com/countdownamountoflockouttries.php", form);
        yield return www.SendWebRequest();

        isCheckingPincode = false;
        if (www.downloadHandler.text[0] == '0')
        {
            string[] webTexts = www.downloadHandler.text.Split('\t');
            int amountOfTries = int.Parse(webTexts[1]);
            disclaimerText.enabled = true;
            disclaimerText.text = "Verkeerde pincode, u heeft nog " + amountOfTries + " pogingen. Na deze pogingen kunt u 1 dag niet meer inloggen " +
                "of moet u contact opnemen met de administratoren.";
            if (amountOfTries == 0)
            {
                StartCoroutine(LockoutUser());
            }
        } else
        {
            SceneManager.LoadScene(failedPincodeScene);
            LogInManager.LogOut(this);
            Debug.LogError("Update lock count down failed. Error#" + www.downloadHandler.text);
        }
    }

    private IEnumerator LockoutUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", LogInManager.userID);
        form.AddField("LockOutDate", System.DateTime.Now.AddHours(24).ToString());

        UnityWebRequest www = UnityWebRequest.Post("https://riectafel.000webhostapp.com/lockoutuser.php", form);
        yield return www.SendWebRequest();

        if (www.downloadHandler.text[0] != '0')
        {
            Debug.LogError("Update lock out time failed. Error#" + www.downloadHandler.text);
        }

        SceneManager.LoadScene(failedPincodeScene);
        LogInManager.LogOut(this);
    }

    private void OnApplicationQuit()
    {
        LogInManager.LogOut(this);
    }
}
