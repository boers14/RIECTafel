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

    private bool isTweening = false, clockWiseRotate = false;

    [SerializeField]
    private VRPlayer player = null;

    [SerializeField]
    private float moveMapPower = 1, scalePower = 0.1f, minimumScale = 0.5f, maximumScale = 5;

    [SerializeField, Tooltip("UnityTileSize * 2 / 10")]
    private float maxTileOffset = 12f;

    [SerializeField]
    private GameObject emptyTransform = null;

    private GameObject pivotObject = null;

    private Transform originalParent = null;

    private void Start()
    {
        originalParent = transform.parent;

        maxTileOffset -= table.localScale.z / 2;
        offset.y = table.transform.localScale.y / 2 + 0.01f;

        abstractMap = GetComponent<AbstractMap>();
        abstractMap.SetPlacementType(MapPlacementType.AtTileCenter);
        inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);

        transform.position = table.position + offset;
        ChangeMapScale(1);
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !isTweening)
        {
            RotateMap(90, true);
        }

        if (Input.GetKeyDown(KeyCode.X) && !isTweening)
        {
            RotateMap(-90, false);
        }

        if (Input.GetKey(KeyCode.RightArrow) && !isTweening)
        {
            MoveTheMap(new Vector2(-1, 0));
        }

        if (Input.GetKey(KeyCode.LeftArrow) && !isTweening)
        {
            MoveTheMap(new Vector2(1, 0));
        }

        if (Input.GetKey(KeyCode.UpArrow) && !isTweening)
        {
            MoveTheMap(new Vector2(0, -1));
        }

        if (Input.GetKey(KeyCode.DownArrow ) && !isTweening)
        {
            MoveTheMap(new Vector2(0, 1));
        }

        if (Input.GetKey(KeyCode.C) && !isTweening)
        {
            ChangeMapScale(-scalePower);
        }

        if (Input.GetKey(KeyCode.V) && !isTweening)
        {
            ChangeMapScale(scalePower);
        }

        if (!inputDevice.isValid)
        {
            inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
            return;
        }

        if (inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool AButton) && AButton && !isTweening)
        {
            RotateMap(90, true);
        }

        if (inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool BButton) && BButton && !isTweening)
        {
            RotateMap(-90, false);
        }

        if (inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 steerStickInput) && steerStickInput != Vector2.zero && !isTweening)
        {
            MoveTheMap(steerStickInput);
        }
    }

    private void RotateMap(float rotation, bool clockWise)
    {
        clockWiseRotate = clockWise;

        Vector3 nextRotation = transform.eulerAngles;
        nextRotation.y += rotation;
        isTweening = true;

        pivotObject = Instantiate(emptyTransform, table.position, transform.rotation);
        transform.SetParent(pivotObject.transform);
        player.ParentPOIs(pivotObject.transform);

        iTween.RotateTo(pivotObject, iTween.Hash("rotation", nextRotation, "time", 1f, "easetype", iTween.EaseType.linear,
            "oncomplete", "CanTweenAgain", "oncompletetarget", gameObject));
    }

    private void CanTweenAgain()
    {
        isTweening = false;
        player.ParentPOIs(null);
        transform.SetParent(originalParent);
        Destroy(pivotObject);

        float xOffset = offset.x;
        if (clockWiseRotate)
        {
            offset.x = offset.z;
            offset.z = -xOffset;
        } else
        {
            offset.x = -offset.z;
            offset.z = xOffset;
        }
    }

    private void MoveTheMap(Vector2 steerStickDirection)
    {
        if (steerStickDirection.x < 0.1f && steerStickDirection.y < 0.1f && steerStickDirection.x > -0.1f && steerStickDirection.y > -0.1f) { return; }

        Vector2 movement = new Vector2(steerStickDirection.x * moveMapPower, steerStickDirection.y * moveMapPower);
        offset.x += movement.x;
        offset.z += movement.y;
        player.MovePOIs(new Vector3(movement.x, 0, movement.y));

        CheckIfMapIsStillOnTable();

        Vector3 newPos = transform.position;
        newPos.x += movement.x;
        newPos.z += movement.y;
        transform.position = newPos;
    }

    private void ChangeMapScale(float changedScale)
    {
        if (changedScale < 0 && transform.localScale.x == minimumScale || changedScale > 0 && transform.localScale.x == maximumScale) { return; }

        Vector3 nextScale = transform.localScale;
        nextScale.x += changedScale;
        nextScale.z += changedScale;

        if (nextScale.x < minimumScale)
        {
            nextScale.x = minimumScale;
            nextScale.z = minimumScale;
        } else if (nextScale.x > maximumScale)
        {
            nextScale.x = maximumScale;
            nextScale.z = maximumScale;
        }
        transform.localScale = nextScale;

        nextScale.y = nextScale.z;
        player.SetPOIsScale(nextScale);

        maxTileOffset = 12 * nextScale.x;
        CheckIfMapIsStillOnTable();
    }

    private void CheckIfMapIsStillOnTable()
    {
        if (offset.x < -maxTileOffset || offset.x > maxTileOffset || offset.z < -maxTileOffset || offset.z > maxTileOffset)
        {
            SetNewMapCenter(abstractMap.WorldToGeoPosition(table.position));
            offset = Vector3.zero;
            offset.y = table.transform.localScale.y / 2 + 0.01f;
            player.SetPOIMapPosition();
            transform.position = table.position + offset;
        }
    }

    private void SetNewMapCenter(Vector2d newCenter)
    {
        abstractMap.SetCenterLatitudeLongitude(newCenter);
        abstractMap.UpdateMap();
    }
}
