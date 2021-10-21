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

    public override void Start()
    {
        base.Start();
        registerButton = FindObjectOfType<RegisterButton>();
        inputfield.onValueChanged.AddListener(CheckifCanRegister);
    }

    private void CheckifCanRegister(string text)
    {
        this.text = text;
        registerButton.CheckIfCanRegister();
    }

    public void CheckWarningStatus()
    {
        if (isFilledIn)
        {
            warningText.enabled = false;
        } else
        {
            warningText.enabled = true;
        }
    }
}
