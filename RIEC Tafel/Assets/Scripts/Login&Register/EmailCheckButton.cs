using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

        WWW www = new WWW("http://localhost/riectafel/resetlockouttries.php", form);
        yield return www;

        isCheckingPincode = false;
        if (www.text[0] == '0')
        {
            base.SwitchScene();
        } else
        {
            Debug.LogError("Sent user to main menu failed. Error#" + www.text);
        }
    }

    private IEnumerator CountDownAmountOfTries()
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", LogInManager.userID);

        WWW www = new WWW("http://localhost/riectafel/countdownamountoflockouttries.php", form);
        yield return www;

        isCheckingPincode = false;
        if (www.text[0] == '0')
        {
            string[] webTexts = www.text.Split('\t');
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
            LogInManager.LogOut();
            Debug.LogError("Update lock count down failed. Error#" + www.text);
        }
    }

    private IEnumerator LockoutUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", LogInManager.userID);
        form.AddField("LockOutDate", System.DateTime.Now.AddHours(24).ToString());

        WWW www = new WWW("http://localhost/riectafel/lockoutuser.php", form);
        yield return www;

        if (www.text[0] != '0')
        {
            Debug.LogError("Update lock out time failed. Error#" + www.text);
        }

        SceneManager.LoadScene(failedPincodeScene);
        LogInManager.LogOut();
    }
}
