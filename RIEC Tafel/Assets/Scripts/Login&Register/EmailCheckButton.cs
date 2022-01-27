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

    /// <summary>
    /// If pincode is there and the right pincode is filled in let the user in the program, else countdown how many times the user
    /// can try and fill in the right password
    /// </summary>

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

    /// <summary>
    /// Send user to main menu and set his account lock out tries to 5
    /// </summary>

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

    /// <summary>
    /// Count in the database down how many tries the user still has to log in at 0 start to lock out the user
    /// </summary>

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
            // If the back-end crashed just send the user back to the login screen
            SceneManager.LoadScene(failedPincodeScene);
            LogInManager.LogOut(this);
            Debug.LogError("Update lock count down failed. Error#" + www.downloadHandler.text);
        }
    }

    /// <summary>
    /// Lock the user out of the program for 1 day and send the user to the login screen
    /// </summary>

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

    /// <summary>
    /// Logout the user if the user quits the application on this screen so the user is also logged out in the database
    /// </summary>

    private void OnApplicationQuit()
    {
        LogInManager.LogOut(this);
    }
}
