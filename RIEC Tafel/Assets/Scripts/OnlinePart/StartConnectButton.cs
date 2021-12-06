using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartConnectButton : ConnectButton
{
    [SerializeField]
    private TMP_InputField citySelectionInputfield = null;

    public override void SetConnectionFunction()
    {
        base.SetConnectionFunction();
        ConnectionManager.cityName = citySelectionInputfield.text;
    }
}
