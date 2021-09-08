using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using UnityEngine.XR;
using Mapbox.Utils;

public class MoveMap : MonoBehaviour
{
    [SerializeField]
    private Transform table = null;

    private Vector3 offset = Vector3.zero;

    private AbstractMap abstractMap = null;

    [SerializeField]
    private InputDeviceCharacteristics characteristics = InputDeviceCharacteristics.None;

    private InputDevice inputDevice;

    private bool isTweening = false;

    private void Start()
    {
        abstractMap = GetComponent<AbstractMap>();
        abstractMap.SetPlacementType(MapPlacementType.AtTileCenter);
        inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
        UpdateOffset();
    }

    private void FixedUpdate()
    {
        transform.position = table.position + offset;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            UpdateOffset();
        }

        if (!inputDevice.isValid)
        {
            inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
            return;
        }

        if (inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool AButton) && AButton && !isTweening)
        {
            RotateMap(90);
        }

        if (inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool BButton) && BButton && !isTweening)
        {
            RotateMap(-90);
        }
    }

    private void RotateMap(float rotation)
    {
        Vector3 nextRotation = transform.eulerAngles;
        nextRotation.y += rotation;
        isTweening = true;
        iTween.RotateTo(gameObject, iTween.Hash("rotation", nextRotation, "time", 1f, "easetype", iTween.EaseType.linear,
            "oncomplete", "CanTweenAgain", "oncompletetarget", gameObject));
    }

    private void CanTweenAgain()
    {
        isTweening = false;
    }

    private void UpdateOffset()
    {
        offset.y = table.transform.localScale.y / 2 + 0.01f;
        abstractMap.UpdateMap();
    }
}
