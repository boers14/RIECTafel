using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class Legenda : GrabbebleObjects
{
    [SerializeField]
    private Image legendaImage = null;

    private Vector3 originalPosition = Vector3.zero, originalImageScale = Vector3.one;

    /// <summary>
    /// Initialise required variables
    /// </summary>
    
    public override void Start()
    {
        originalImageScale = legendaImage.rectTransform.localScale;
        originalPosition = legendaImage.rectTransform.localPosition;
        legendaImage.gameObject.SetActive(false);
        base.Start();
    }

    /// <summary>
    /// Perform base update
    /// </summary>

    public override void Update()
    {
        if (!legendaImage.gameObject.activeSelf) { return; }
        base.Update();
    }

    /// <summary>
    /// Perform base change image scale
    /// </summary>

    public override void ChangeImageScale(float scalePower, GameObject image, Vector3 vector3, float extraYMovement)
    {
        base.ChangeImageScale(scalePower, legendaImage.gameObject, originalPosition, 11);
    }

    /// <summary>
    /// Perform base move image
    /// </summary>

    public override void MoveImage(Vector2 steerStickInput, GameObject image, Vector3 vector3, float extraYMovement, bool nullifyMovement)
    {
        base.MoveImage(steerStickInput, legendaImage.gameObject, originalPosition, 11, nullifyMovement);
    }

    /// <summary>
    /// Set required variables when grabbing object
    /// </summary>

    public override void OnGrabEnter(SelectEnterEventArgs selectEnterEventArgs, bool setOriginalVectors)
    {
        base.OnGrabEnter(selectEnterEventArgs, setOriginalVectors);
        legendaImage.rectTransform.localPosition = originalPosition;
        legendaImage.rectTransform.localScale = originalImageScale;
        legendaImage.transform.SetAsLastSibling();
        legendaImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// Turn off legenda when dropping object
    /// </summary>

    public override void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
    {
        base.OnSelectExit(selectExitEventArgs);
        legendaImage.gameObject.SetActive(false);
    }
}
