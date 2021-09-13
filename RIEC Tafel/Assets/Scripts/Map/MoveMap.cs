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

    [SerializeField]
    private VRPlayer player = null;

    [SerializeField]
    private float moveMapPower = 1;

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

        if (Input.GetKeyDown(KeyCode.Z) && !isTweening)
        {
            RotateMap(90);
        }

        if (Input.GetKeyDown(KeyCode.X) && !isTweening)
        {
            RotateMap(-90);
        }

        if (Input.GetKey(KeyCode.RightArrow) && !isTweening)
        {
            MoveTheMap(new Vector2(1, 0));
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

    private void MoveTheMap(Vector2 steerStickDirection)
    {
        if (steerStickDirection.x < 0.1f && steerStickDirection.y < 0.1f && steerStickDirection.x > -0.1f && steerStickDirection.y > -0.1f) { return; }

        print(abstractMap.CenterLatitudeLongitude);
        Vector2d direction = new Vector2d(steerStickDirection.x, steerStickDirection.y);
        Vector2d newCenter = abstractMap.CenterLatitudeLongitude;
        newCenter += direction * moveMapPower;
        abstractMap.SetCenterLatitudeLongitude(newCenter);
        abstractMap.UpdateMap();
    }

    private void UpdateOffset()
    {
        offset.y = table.transform.localScale.y / 2 + 0.01f;
    }
}
