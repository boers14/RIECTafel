using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartConnectButton : ConnectButton
{
    [SerializeField]
    private TMP_InputField citySelectionInputfield = null, roomNameInputField = null;

    public override void SwitchScene()
    {
        if (citySelectionInputfield.text == "" || roomNameInputField.text == "") { return; }

        base.SwitchScene();
    }

    public override void SetConnectionFunction()
    {
        base.SetConnectionFunction();
        ConnectionManager.cityName = citySelectionInputfield.text;
        ConnectionManager.roomName = roomNameInputField.text;
    }
}
