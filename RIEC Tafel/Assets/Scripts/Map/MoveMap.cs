using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using UnityEngine.XR;
using Mapbox.Utils;
using UnityEngine.XR.Interaction.Toolkit;

public class MoveMap : MonoBehaviour
{
    [SerializeField]
    private Transform table = null;

    private Vector3 offset = Vector3.zero, oldScale = Vector3.one;

    private AbstractMap abstractMap = null;

    [SerializeField]
    private InputDeviceCharacteristics characteristics = InputDeviceCharacteristics.None;

    private InputDevice inputDevice;

    private bool isTweening = false, clockWiseRotate = false, playerTargetsObject = false;

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

    private float originalMaxTileOffset = 0;

    [SerializeField]
    private XRRayInteractor rightRayInteractor = null;

    private LineRenderer lineVisual = null;

    [System.NonSerialized]
    public Transform playerConnectionTransform = null;

    private void Start()
    {
        lineVisual = rightRayInteractor.GetComponent<LineRenderer>();
        rightRayInteractor.selectEntered.AddListener(PlayerGrabsObject);
        rightRayInteractor.selectExited.AddListener(PlayerDropsObject);

        originalParent = transform.parent;

        originalMaxTileOffset = maxTileOffset;
        maxTileOffset -= table.localScale.z / 2;
        offset.y = table.transform.localScale.y / 2 + 0.01f;

        abstractMap = GetComponent<AbstractMap>();
        abstractMap.SetPlacementType(MapPlacementType.AtTileCenter);
        inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);

        transform.position = table.position + offset;
    }

    private void FixedUpdate()
    {
        ComputerControls();

        if (!inputDevice.isValid)
        {
            inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
            return;
        }

        if (isTweening || playerTargetsObject || lineVisual.colorGradient.alphaKeys[0].alpha != 0)
        {
            return;
        }

        if (inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerButton) && triggerButton > 0.1f)
        {
            ChangeMapScale(-scalePower);
        }

        if (inputDevice.TryGetFeatureValue(CommonUsages.grip, out float gripButton) && gripButton > 0.1f)
        {
            ChangeMapScale(scalePower);
        }

        if (inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool AButton) && AButton)
        {
            RotateMap(90, true);
        }

        if (inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool BButton) && BButton)
        {
            RotateMap(-90, false);
        }

        if (inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 steerStickInput) && steerStickInput != Vector2.zero)
        {
            if (playerConnectionTransform)
            {
                float xInput = steerStickInput.x;
                switch (playerConnectionTransform.eulerAngles.y)
                {
                    case 90:
                        steerStickInput.x = steerStickInput.y;
                        steerStickInput.y = -xInput;
                        break;
                    case 180:
                        steerStickInput.x = -steerStickInput.x;
                        steerStickInput.y = -steerStickInput.y;
                        break;
                    case 270:
                        steerStickInput.x = -steerStickInput.y;
                        steerStickInput.y = xInput;
                        break;
                }
            }

            MoveTheMap(steerStickInput, true);
        }
    }

    public void RotateMap(float rotation, bool clockWise)
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

    private void MoveTheMap(Vector2 steerStickDirection, bool movePOIs)
    {
        if (steerStickDirection.x < 0.1f && steerStickDirection.y < 0.1f && steerStickDirection.x > -0.1f && steerStickDirection.y > -0.1f) { return; }

        Vector2 movement = new Vector2(steerStickDirection.x * moveMapPower, steerStickDirection.y * moveMapPower);
        offset.x += movement.x;
        offset.z += movement.y;
        player.SetExtraOffset(offset);

        if (movePOIs)
        {
            player.MovePOIs(new Vector3(movement.x, 0, movement.y));
        }

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
        nextScale.x += changedScale * oldScale.x;
        nextScale.z += changedScale * oldScale.x;

        if (nextScale.x < minimumScale)
        {
            nextScale.x = minimumScale;
            nextScale.z = minimumScale;
        } else if (nextScale.x > maximumScale)
        {
            nextScale.x = maximumScale;
            nextScale.z = maximumScale;
        }

        SetScaleOfMap(nextScale);
    }

    private float CalculatePosDiff(float oldMaxTileOffset, float offsetParameter)
    {
        float xPercentageOnMap = offsetParameter / oldMaxTileOffset;
        float newXPos = xPercentageOnMap * maxTileOffset;
        float possDiff = newXPos - offsetParameter;
        return possDiff / moveMapPower;
    }

    public void ChangeMapScaleToOne()
    {
        SetScaleOfMap(Vector3.one);
    }

    private void SetScaleOfMap(Vector3 nextScale)
    {
        transform.localScale = nextScale;
        oldScale = nextScale;
        float oldMaxTileOffset = maxTileOffset;
        maxTileOffset = originalMaxTileOffset * nextScale.x - table.localScale.z / 2;
        nextScale.y = nextScale.z;
        player.SetPOIsScale(nextScale);

        MoveTheMap(new Vector2(CalculatePosDiff(oldMaxTileOffset, offset.x), CalculatePosDiff(oldMaxTileOffset, offset.z)), false);
    }

    private void CheckIfMapIsStillOnTable()
    {
        if (offset.x < -maxTileOffset || offset.x > maxTileOffset || offset.z < -maxTileOffset || offset.z > maxTileOffset)
        {
            SetNewMapCenter(abstractMap.WorldToGeoPosition(table.position));
        }
    }

    public void SetNewMapCenter(Vector2d newCenter)
    {
        transform.localScale = Vector3.one;
        float regularMaxTileOffset = originalMaxTileOffset - table.localScale.z / 2;
        float percentageGrowth = maxTileOffset / regularMaxTileOffset;
        offset.x /= percentageGrowth;
        offset.z /= percentageGrowth;
        transform.position = table.position + offset;
        abstractMap.SetCenterLatitudeLongitude(newCenter);
        abstractMap.UpdateMap();
        offset = Vector3.zero;
        offset.y = table.transform.localScale.y / 2 + 0.01f;
        player.SetExtraOffset(offset);
        player.SetPOIMapPosition();
        transform.position = table.position + offset;
        transform.localScale = oldScale;
    }

    private void PlayerGrabsObject(SelectEnterEventArgs args)
    {
        playerTargetsObject = true;
    }

    private void PlayerDropsObject(SelectExitEventArgs args)
    {
        playerTargetsObject = false;
    }

    private void ComputerControls()
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
            MoveTheMap(new Vector2(-1, 0), true);
        }

        if (Input.GetKey(KeyCode.LeftArrow) && !isTweening)
        {
            MoveTheMap(new Vector2(1, 0), true);
        }

        if (Input.GetKey(KeyCode.UpArrow) && !isTweening)
        {
            MoveTheMap(new Vector2(0, -1), true);
        }

        if (Input.GetKey(KeyCode.DownArrow) && !isTweening)
        {
            MoveTheMap(new Vector2(0, 1), true);
        }

        if (Input.GetKey(KeyCode.C) && !isTweening)
        {
            ChangeMapScale(-scalePower);
        }

        if (Input.GetKey(KeyCode.V) && !isTweening)
        {
            ChangeMapScale(scalePower);
        }
    }
}
