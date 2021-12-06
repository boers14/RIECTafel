using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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

    private bool canRegister = false;

    public void CheckIfCanRegister()
    {
        bool canRegister = true;

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

        if (!emailField.text.EndsWith("@om.nl"))
        {
            canRegister = false;
            emailField.isFilledIn = false;
        }
        else
        {
            emailField.isFilledIn = true;
        }

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

        StartCoroutine(RegisterUser());
    }

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

        WWW www = new WWW("http://localhost/riectafel/register.php", form);
        yield return www;

        if (www.text == "0")
        {
            SceneManager.LoadScene(sceneToSwitchTo);
        }
        else
        {
            Debug.LogError("Registration failed. Error# " + www.text);
            disclaimerText.enabled = true;
        }
    }
}
