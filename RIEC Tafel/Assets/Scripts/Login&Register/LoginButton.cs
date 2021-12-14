using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class LoginButton : SwitchSceneButton
{
    [SerializeField]
    private List<TMP_InputField> inputFields = new List<TMP_InputField>();

    [SerializeField]
    private TMP_Text disclaimerText = null;

    [SerializeField]
    private string tutorialScene = "";

    public override void Start()
    {
        base.Start();
        if (!SaveSytem.CheckIfFileExist()) 
        {
            TutorialSceneManager.sceneToSwitchTo = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(tutorialScene);
        } else
        {
            PlayerData data = SaveSytem.LoadGame();
            SettingsManager.LoadData(data, FindObjectOfType<KeyBoard>());
        }
    }

    public override void SwitchScene()
    {
        StartCoroutine(Login());
    }

    private IEnumerator Login()
    {
        WWWForm form = new WWWForm();
        for (int  i= 0; i < inputFields.Count; i++)
        {
            form.AddField(inputFields[i].name, inputFields[i].text);
        }

        UnityWebRequest www = UnityWebRequest.Post("https://riectafel.000webhostapp.com/login.php", form);
        yield return www.SendWebRequest();

        if (www.downloadHandler.text[0] == '0')
        {
            string[] webTexts = www.downloadHandler.text.Split('\t');
            LogInManager.userID = int.Parse(webTexts[1]);
            LogInManager.username = webTexts[2];
            LogInManager.datatype = webTexts[3];
            LogInManager.iCOVWorker = bool.Parse(webTexts[4]);
            LogInManager.pincode = webTexts[5];
            LogInManager.avatarData = webTexts[6];
            SceneManager.LoadScene(sceneToSwitchTo);
        }
        else
        {
            disclaimerText.enabled = enabled;
            Debug.LogError("Log in failed. Error#" + www.downloadHandler.text);
        }
    }
}
