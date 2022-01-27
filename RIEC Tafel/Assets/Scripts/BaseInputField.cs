using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BaseInputField : EditebleText
{
    [System.NonSerialized]
    public TMP_InputField inputfield = null;

    private EditebleText editebleText = null;

    /// <summary>
    /// Sets a inputfield as editeble when it is selected
    /// </summary>

    public override void Start()
    {
        base.Start();

        inputfield = GetComponent<TMP_InputField>();
        editebleText = GetComponentInChildren<EditebleText>();

        inputfield.onSelect.AddListener(OnInputfieldSelect);
    }

    private void OnInputfieldSelect(string text)
    {
        editebleText.SetAsEditeble();
    }
}
