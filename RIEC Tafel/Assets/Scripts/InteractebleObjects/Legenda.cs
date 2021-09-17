using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class Legenda : GrabbebleObjects
{
    [SerializeField]
    private Image legendaImage = null;

    public override void Start()
    {
        legendaImage.enabled = false;
        base.Start();
    }

    private void Update()
    {
        if (!legendaImage.enabled) { return; }
    }

    public override void OnGrabEnter(SelectEnterEventArgs selectEnterEventArgs)
    {
        base.OnGrabEnter(selectEnterEventArgs);
        legendaImage.enabled = true;
    }

    public override void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
    {
        base.OnSelectExit(selectExitEventArgs);
        legendaImage.enabled = false;
    }
}
