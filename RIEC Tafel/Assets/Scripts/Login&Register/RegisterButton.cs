using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class RegisterButton : SwitchSceneButton
{
    [SerializeField]
    private List<RegisterInputfield> inputFields = new List<RegisterInputfield>();

    [SerializeField]
    private RegisterInputfield passwordField = null, emailField = null;

    [SerializeField]
    private RegisterToggle userAccordToggle = null;

    [SerializeField]
    private DataTypeDropdown dataTypeDropdown = null;

    [SerializeField]
    private Toggle icovWorkerToggle = null;

    [SerializeField]
    private TMP_Text disclaimerText = null;

    [SerializeField]
    private List<AvatarChangebleBodypart> bodyparts = new List<AvatarChangebleBodypart>();

    private bool canRegister = false, isRegistering = false;

    /// <summary>
    /// Check whether all inputfields have the required input and if the privacy statement toggle is toggled
    /// This is checked whenever input is changed from the inputfields and when privacy statement toggle is toggled
    /// </summary>

    public void CheckIfCanRegister()
    {
        bool canRegister = true;

        // At least 2 characters in every field
        for (int i = 0; i < inputFields.Count; i++)
        {
            if (inputFields[i].text.Length < 2)
            {
                canRegister = false;
                inputFields[i].isFilledIn = false;
            } else
            {
                inputFields[i].isFilledIn = true;
            }
        }

        // Has to be a mail from the government
        if (!emailField.text.EndsWith("@om.nl"))
        {
            canRegister = false;
            emailField.isFilledIn = false;
        }
        else
        {
            emailField.isFilledIn = true;
        }

        // Checks whether all required characters are in there
        bool lowerCase = false, upperCase = false, number = false;
        foreach (char c in passwordField.text)
        {
            if (char.IsUpper(c))
            {
                upperCase = true;
                continue;
            }

            if (char.IsLower(c))
            {
                lowerCase = true;
                continue;
            }

            if (char.IsDigit(c))
            {
                number = true;
                continue;
            }
        }

        // With password minimum length of 8
        if (!lowerCase || !upperCase || !number || passwordField.text.Length < 8)
        {
            canRegister = false;
            passwordField.isFilledIn = false;
        }
        else
        {
            passwordField.isFilledIn = true;
        }

        if (!userAccordToggle.isOn)
        {
            canRegister = false;
        }

        this.canRegister = canRegister;
    }

    /// <summary>
    /// If cant register, turn on warning texts for all fields not having the required input
    /// If can register, start register
    /// </summary>

    public override void SwitchScene()
    {
        if (!canRegister)
        {
            userAccordToggle.CheckWarningStatus();
            for (int i = 0; i < inputFields.Count; i++)
            {
                inputFields[i].CheckWarningStatus();
            }
            return;
        }

        if (isRegistering) { return; }
        isRegistering = true;

        StartCoroutine(RegisterUser());
    }

    /// <summary>
    /// Add the user to the database with the required variables
    /// </summary>

    private IEnumerator RegisterUser()
    {
        WWWForm form = new WWWForm();
        for (int i = 0; i < inputFields.Count; i++)
        {
            form.AddField(inputFields[i].name, inputFields[i].text);
        }
        form.AddField("datatype", dataTypeDropdown.dataType);
        form.AddField("icovWorkerToggle", icovWorkerToggle.isOn.ToString());

        string avatarStats = "";
        for (int i = 0; i < bodyparts.Count; i++)
        {
            Vector3 scale = bodyparts[i].transform.localScale;
            string modelName = bodyparts[i].GetComponent<MeshFilter>().mesh.name.Split(' ')[0];
            avatarStats += bodyparts[i].bodyType.ToString() + "\n" + modelName + "\n" + scale.x + "." + scale.y + "." + scale.z + "\n" + 
                bodyparts[i].standardScale.x + "." + bodyparts[i].standardScale.y;
            if (i < bodyparts.Count - 1)
            {
                avatarStats += "/*nextbodypart*/";
            }
        }
        form.AddField("avatarStats", avatarStats);

        UnityWebRequest www = UnityWebRequest.Post("https://riectafel.000webhostapp.com/register.php", form);
        yield return www.SendWebRequest();

        isRegistering = false;

        // Switch scene if the user was succesfully placed in the database else print the error
        if (www.downloadHandler.text == "0")
        {
            SceneManager.LoadScene(sceneToSwitchTo);
        }
        else
        {
            Debug.LogError("Registration failed. Error# " + www.downloadHandler.text);
            disclaimerText.enabled = true;
        }
    }
}
