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

    private bool isTweening = false;

    [SerializeField]
    private POIManager poiManager = null;

    [SerializeField]
    private float moveMapPower = 1, scalePower = 0.1f;

    public float minimumScale = 0.5f, maximumScale = 5;

    [SerializeField, Tooltip("UnityTileSize * 2 / 10")]
    private float maxTileOffset = 12f;

    [SerializeField]
    private GameObject emptyTransform = null;

    private GameObject pivotObject = null;

    private Transform originalParent = null, closestPOI = null;

    private float originalMaxTileOffset = 0, rotationPower = 2;

    [SerializeField]
    private XRRayInteractor rightRayInteractor = null;

    private LineRenderer lineVisual = null;

    [System.NonSerialized]
    public Transform playerConnectionTransform = null;

    private int rotationCounter = 0;

    private void Start()
    {
        lineVisual = rightRayInteractor.GetComponent<LineRenderer>();

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

        if (isTweening)
        {
            return;
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

        if (lineVisual.colorGradient.alphaKeys[0].alpha != 0)
        {
            return;
        }

        if (inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool AButton) && AButton)
        {
            ChangeMapScale(scalePower);
        }
        else if (inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool BButton) && BButton)
        {
            ChangeMapScale(-scalePower);
        }

        if (inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerButton) && triggerButton > 0.1f)
        {
            RotateMap(rotationPower);
        }
        else if (inputDevice.TryGetFeatureValue(CommonUsages.grip, out float gripButton) && gripButton > 0.1f)
        {
            RotateMap(-rotationPower);
        }
    }

    public void RotateMap(float rotation)
    {
        Vector3 nextRotation = transform.eulerAngles;
        nextRotation.y += rotation;
        isTweening = true;

        if (!closestPOI)
        {
            closestPOI = poiManager.ReturnClosestPOIToTransform(table.position);
            if (!closestPOI)
            {
                closestPOI = table;
            }
        }

        pivotObject = Instantiate(emptyTransform, closestPOI.position, transform.rotation);
        transform.SetParent(pivotObject.transform);
        poiManager.ParentPOIs(pivotObject.transform, false);

        iTween.RotateTo(pivotObject, iTween.Hash("rotation", nextRotation, "time", 0.01f, "easetype", iTween.EaseType.linear,
            "oncomplete", "CanTweenAgain", "oncompletetarget", gameObject));
    }

    private void CanTweenAgain()
    {
        isTweening = false;
        poiManager.ParentPOIs(null, true);
        transform.SetParent(originalParent);
        Destroy(pivotObject);

        offset.x = transform.position.x - table.position.x;
        offset.z = transform.position.z - table.position.z;

        poiManager.CheckPOIVisibility();
        StartCoroutine(CheckIfPayerStillRotates());
    }

    private IEnumerator CheckIfPayerStillRotates()
    {
        rotationCounter++;
        yield return new WaitForSeconds(0.1f);
        rotationCounter--;
        if (rotationCounter == 0)
        {
            closestPOI = null;
        }
    }

    private void MoveTheMap(Vector2 steerStickDirection, bool movePOIs)
    {
        if (steerStickDirection.x < 0.1f && steerStickDirection.y < 0.1f && steerStickDirection.x > -0.1f && steerStickDirection.y > -0.1f) { return; }

        Vector2 movement = new Vector2(steerStickDirection.x * moveMapPower, steerStickDirection.y * moveMapPower);
        offset.x += movement.x;
        offset.z += movement.y;
        poiManager.SetExtraOffset(offset);

        if (movePOIs)
        {
            poiManager.MovePOIs(new Vector3(movement.x, 0, movement.y));
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

    public void ChangeMapScaleToChosenScale(Vector3 chosenScale)
    {
        SetScaleOfMap(chosenScale);
    }

    private void SetScaleOfMap(Vector3 nextScale)
    {
        transform.localScale = nextScale;
        oldScale = nextScale;
        float oldMaxTileOffset = maxTileOffset;
        maxTileOffset = originalMaxTileOffset * nextScale.x - table.localScale.z / 2;
        nextScale.y = nextScale.z;
        poiManager.SetPOIsScale(nextScale);

        MoveTheMap(new Vector2(BaseCalculations.CalculatePosDiff(oldMaxTileOffset, maxTileOffset, offset.x, moveMapPower),
            BaseCalculations.CalculatePosDiff(oldMaxTileOffset, maxTileOffset, offset.z, moveMapPower)), false);
    }

    private void CheckIfMapIsStillOnTable()
    {
        Vector3 offsetValueWithYZero = offset;
        offsetValueWithYZero.y = 0;

        if (Vector3.Distance(Vector3.zero, offsetValueWithYZero) > maxTileOffset)
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
        poiManager.SetExtraOffset(offset);
        poiManager.SetPOIsScale(oldScale);
        transform.position = table.position + offset;
        ChangeMapScaleToChosenScale(oldScale);
    }

    private void ComputerControls()
    {
        if (Input.GetKey(KeyCode.Z) && !isTweening)
        {
            RotateMap(rotationPower);
        } else if (Input.GetKey(KeyCode.X) && !isTweening)
        {
            RotateMap(-rotationPower);
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
