using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartConnectButton : ConnectButton
{
    [SerializeField]
    private TMP_InputField citySelectionInputfield = null;

    /// <summary>
    /// Set the connection manager cityname to the filled in cityname
    /// </summary>

    public override void SetConnectionFunction()
    {
        base.SetConnectionFunction();
        ConnectionManager.cityName = citySelectionInputfield.text;
    }
}
