using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowPasswordToggle : MonoBehaviour
{
    [SerializeField]
    private Sprite hidePasswordImage = null;

    private Sprite showPasswordImage = null;

    private Image currentCheckboxVisual = null;

    [SerializeField]
    private TMP_InputField passwordField = null;

    /// <summary>
    /// Change the password visual state to letters or stars
    /// </summary>

    private void Start()
    {
        currentCheckboxVisual = GetComponentInChildren<Image>();
        showPasswordImage = currentCheckboxVisual.sprite;
        GetComponent<Toggle>().onValueChanged.AddListener(ShowPasswordFieldText);
    }

    private void ShowPasswordFieldText(bool isOn)
    {
        if (isOn)
        {
            currentCheckboxVisual.sprite = hidePasswordImage;
            passwordField.contentType = TMP_InputField.ContentType.Standard;
        } else
        {
            currentCheckboxVisual.sprite = showPasswordImage;
            passwordField.contentType = TMP_InputField.ContentType.Password;
        }

        passwordField.ForceLabelUpdate();
    }
}
