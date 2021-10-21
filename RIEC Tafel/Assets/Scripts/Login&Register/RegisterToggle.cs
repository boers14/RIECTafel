using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RegisterToggle : MonoBehaviour
{
    private RegisterButton registerButton = null;

    [System.NonSerialized]
    public bool isOn = false;

    [SerializeField]
    private TMP_Text warningText = null;

    private void Start()
    {
        registerButton = FindObjectOfType<RegisterButton>();
        GetComponent<Toggle>().onValueChanged.AddListener(CheckifCanRegister);
    }

    private void CheckifCanRegister(bool isOn)
    {
        this.isOn = isOn;
        registerButton.CheckIfCanRegister();
    }

    public void CheckWarningStatus()
    {
        if (isOn)
        {
            warningText.enabled = false;
        }
        else
        {
            warningText.enabled = true;
        }
    }
}
