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

    /// <summary>
    /// Sets the agreed to privacy statement to true
    /// </summary>

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

    /// <summary>
    /// Changes the active state of the warning text
    /// </summary>

    public void CheckWarningStatus()
    {
        warningText.enabled = !isOn;
    }
}
