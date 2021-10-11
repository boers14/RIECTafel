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

    public override void Start()
    {
        originalImageScale = legendaImage.rectTransform.localScale;
        originalPosition = legendaImage.rectTransform.localPosition;
        legendaImage.gameObject.SetActive(false);
        base.Start();
    }

    public override void Update()
    {
        if (!legendaImage.gameObject.activeSelf) { return; }
        base.Update();
    }

    public override void ChangeImageScale(float scalePower, GameObject image, Vector3 vector3, float extraYMovement)
    {
        base.ChangeImageScale(scalePower, legendaImage.gameObject, originalPosition, 11);
    }

    public override void MoveImage(Vector2 steerStickInput, GameObject image, Vector3 vector3, float extraYMovement, bool nullifyMovement)
    {
        base.MoveImage(steerStickInput, legendaImage.gameObject, originalPosition, 11, nullifyMovement);
    }

    public override void OnGrabEnter(SelectEnterEventArgs selectEnterEventArgs, bool setOriginalVectors)
    {
        base.OnGrabEnter(selectEnterEventArgs, setOriginalVectors);
        legendaImage.rectTransform.localPosition = originalPosition;
        legendaImage.rectTransform.localScale = originalImageScale;
        legendaImage.transform.SetAsLastSibling();
        legendaImage.gameObject.SetActive(true);
    }

    public override void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
    {
        base.OnSelectExit(selectExitEventArgs);
        legendaImage.gameObject.SetActive(false);
    }
}
