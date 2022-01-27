using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RegisterInputfield : BaseInputField
{
    private RegisterButton registerButton = null;

    [System.NonSerialized]
    public string text = "";

    [SerializeField]
    private TMP_Text warningText = null;

    [System.NonSerialized]
    public bool isFilledIn = false;

    /// <summary>
    /// For all the required inputfields for registering a new user 
    /// </summary>

    public override void Start()
    {
        base.Start();
        registerButton = FindObjectOfType<RegisterButton>();
        inputfield.onValueChanged.AddListener(CheckifCanRegister);
    }

    private void CheckifCanRegister(string text)
    {
        this.text = text;
        // Checks if everything is ready for registering the user
        registerButton.CheckIfCanRegister();
    }

    /// <summary>
    /// Enable warning text if required
    /// </summary>

    public void CheckWarningStatus()
    {
        warningText.enabled = !isFilledIn;
    }
}
