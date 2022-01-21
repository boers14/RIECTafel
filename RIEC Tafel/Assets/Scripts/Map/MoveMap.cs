using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using UnityEngine.XR;
using Mapbox.Utils;
using UnityEngine.XR.Interaction.Toolkit;

public class MoveMap : MonoBehaviour
{
    public Transform table = null;

    [System.NonSerialized]
    public Vector3 offset = Vector3.zero;

    private Vector3 oldScale = Vector3.one, lastRotationOrder = Vector3.zero;

    private AbstractMap abstractMap = null;

    [SerializeField]
    private InputDeviceCharacteristics rightCharacteristics = InputDeviceCharacteristics.None, leftCharacteristics = InputDeviceCharacteristics.None;

    [System.NonSerialized]
    public List<InputDevice> inputDevices = new List<InputDevice>();

    private bool isTweening = false;

    [SerializeField]
    private POIManager poiManager = null;

    [SerializeField]
    private float moveMapPower = 1, scalePower = 0.1f, scaleMapCorrecter = 2;

    public float minimumScale = 0.5f, maximumScale = 5;

    [SerializeField, Tooltip("Abstractmap-UnityTileSize * 2 / 10")]
    private float maxTileOffset = 12f;

    [SerializeField]
    private GameObject emptyTransform = null;

    private GameObject pivotObject = null;

    private Transform closestPOI = null;

    private float originalMaxTileOffset = 0, rotationPower = 2, playerWasScalingCooldown = 0.25f, playerWasScalingTimer = 0;

    [SerializeField]
    private XRRayInteractor rightRayInteractor = null;

    private LineRenderer lineVisual = null;

    [System.NonSerialized]
    public Transform playerConnectionTransform = null, originalParent = null;

    private int rotationCounter = 0, rotationTries = 0;

    [SerializeField]
    private List<PlayerGrab> hands = new List<PlayerGrab>();

    private MiniMap miniMap = null;

    private List<Vector3> prevHandPosses = new List<Vector3>(), prevHandDirections = new List<Vector3>(), 
        prevHandPossesLineRendering = new List<Vector3>(), prevHandRotationsLineRendering = new List<Vector3>();
    private List<Quaternion> prevHandRotations = new List<Quaternion>();
    private List<PlayerHandRays> handRays = new List<PlayerHandRays>();
    private List<bool> handLinesVisibility = new List<bool>();

    private List<GrabbebleObjects> grabbebleObjects = new List<GrabbebleObjects>();

    private PlayerHandsRayInteractor oneHandControlsInteractorObject = null;

    [System.NonSerialized]
    public bool ffaMap = false;

    [System.NonSerialized]
    public PlayerConnection ownPlayer = null;

    private void Start()
    {
        for (int i = 0; i < hands.Count; i++)
        {
            prevHandDirections.Add(Vector3.zero);
            prevHandPosses.Add(Vector3.zero);
            prevHandRotationsLineRendering.Add(Vector3.zero);
            prevHandPossesLineRendering.Add(Vector3.zero);
            prevHandRotations.Add(Quaternion.identity);
            handRays.Add(hands[i].GetComponent<PlayerHandRays>());
            handLinesVisibility.Add(false);
        }

        GrabbebleObjects[] grabbebleObjects = FindObjectsOfType<GrabbebleObjects>();
        this.grabbebleObjects.AddRange(grabbebleObjects);

        lineVisual = rightRayInteractor.GetComponent<LineRenderer>();

        originalParent = transform.parent;

        originalMaxTileOffset = maxTileOffset;
        maxTileOffset -= table.localScale.z / 2;
        offset.y = table.transform.localScale.y / 2 + 0.01f;

        abstractMap = GetComponent<AbstractMap>();
        abstractMap.SetPlacementType(MapPlacementType.AtTileCenter);

        miniMap = FindObjectOfType<MiniMap>();
        oneHandControlsInteractorObject = table.GetComponent<PlayerHandsRayInteractor>();

        GrabControllers();

        transform.position = table.position + offset;
    }

    private void Update()
    {
        if (!ownPlayer)
        {
            return;
        }

        if (!isTweening && transform.localScale.x > maximumScale && transform.parent == originalParent)
        {
            if (transform.localScale.x * originalParent.transform.localScale.x >= minimumScale)
            {
                ChangeMapScaleToChosenScale(transform.localScale * originalParent.transform.localScale.x);
            }
        }

        if (!ffaMap && ownPlayer.playerIsInControlOfMap)
        {
            for (int i = 0; i < handRays.Count; i++)
            {
                if (handRays[i].hoveredObjects.Contains(oneHandControlsInteractorObject))
                {
                    handLinesVisibility[i] = true;
                    if (prevHandPossesLineRendering[i] != hands[i].transform.position ||
                        prevHandRotationsLineRendering[i] != hands[i].transform.eulerAngles)
                    {
                        ownPlayer.CmdDrawHandLines((int)handRays[i].hand, ownPlayer.playerNumber, 
                            table.InverseTransformPoint(handRays[i].hitPoints[handRays[i].hoveredObjects.
                            IndexOf(oneHandControlsInteractorObject)]), poiManager.transform.eulerAngles.y);
                    }
                } else
                {
                    if (handLinesVisibility[i])
                    {
                        handLinesVisibility[i] = false;
                        ownPlayer.CmdTurnOffhandLine((int)handRays[i].hand, ownPlayer.playerNumber);
                    }
                }

                prevHandPossesLineRendering[i] = hands[i].transform.position;
                prevHandRotationsLineRendering[i] = hands[i].transform.eulerAngles;
            }
        }

        if (isTweening || !ffaMap && !ownPlayer.playerIsInControlOfMap)
        {
            return;
        }

        ComputerControls();

        if (inputDevices.Count < 2)
        {
            GrabControllers();
            return;
        }

        if (hands[0].oneButtonControl)
        {
            int hoverCount = 0;

            for (int i = 0; i < inputDevices.Count; i++)
            {
                if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton)
                {
                    if (handRays[i].hoveredObjects.Contains(oneHandControlsInteractorObject))
                    {
                        hoverCount++;
                    }
                }
            }

            for (int i = 0; i < inputDevices.Count; i++)
            {
                if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton)
                {
                    if (handRays[i].hoveredObjects.Contains(oneHandControlsInteractorObject))
                    {
                        if (hoverCount != inputDevices.Count && playerWasScalingTimer < 0)
                        {
                            if (inputDevices[i].TryGetFeatureValue(CommonUsages.grip, out float gripButton) && gripButton > 0.1f)
                            {
                                Quaternion newAngle = hands[i].transform.rotation;
                                Quaternion oldAngle = prevHandRotations[i];
                                float angle = Quaternion.Angle(oldAngle, newAngle);

                                if (GetRotationDirection(oldAngle, newAngle))
                                {
                                    angle *= -1;
                                }

                                RotateMap(angle);
                            }
                            else
                            {
                                Vector3 movement = hands[i].transform.position - prevHandPosses[i];
                                movement.y = movement.z * 200;
                                movement.x *= 150;
                                movement.z = 0;
                                MoveTheMap(movement, true, false);
                            }
                        } else if (hoverCount == inputDevices.Count)
                        {
                            playerWasScalingTimer = playerWasScalingCooldown;
                            float oldDist = Vector3.Distance(prevHandPosses[0], prevHandPosses[1]);
                            float newDist = Vector3.Distance(hands[0].transform.position, hands[1].transform.position);
                            ChangeMapScale((newDist - oldDist));
                        }
                    }
                }

                prevHandDirections[i] = hands[i].transform.eulerAngles - prevHandRotations[i].eulerAngles;
                prevHandPosses[i] = hands[i].transform.position;
                prevHandRotations[i] = hands[i].transform.rotation;
            }

            playerWasScalingTimer -= Time.deltaTime;
        }
        else
        {
            TwoControllerControls();
        }
    }

    private bool GetRotationDirection(Quaternion from, Quaternion to)
    {
        float fromY = from.eulerAngles.y;
        float toY = to.eulerAngles.y;
        float clockWise = 0;
        float counterClockWise = 0;

        if (fromY <= toY)
        {
            clockWise = toY - fromY;
            counterClockWise = fromY + (360 - toY);
        }
        else
        {
            clockWise = (360 - fromY) + toY;
            counterClockWise = fromY - toY;
        }
        return clockWise <= counterClockWise;
    }

    public void RotateMap(float rotation, bool ignoreRotationCheck = false)
    {
        if (!ignoreRotationCheck)
        {
            if (rotation > 5 || rotation < -5) { return; }
        }

        Vector3 nextRotation = transform.eulerAngles;
        if (ignoreRotationCheck)
        {
            nextRotation.y += rotation;
        }
        else
        {
            nextRotation.y += rotation * SettingsManager.rotateMapFactor;
        }

        RotateTowardsAngle(nextRotation);
    }

    public void RotateTowardsAngle(Vector3 nextRotation, bool ignoreIsTweeningCheck = false)
    {
        if (!ignoreIsTweeningCheck)
        {
            if (isTweening) { return; }
        }

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

        lastRotationOrder = nextRotation;

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

        if (miniMap)
        {
            miniMap.RotateMiniMap(transform.localEulerAngles, poiManager.transform.eulerAngles);
        }

        if (transform.eulerAngles.y <= lastRotationOrder.y - 1 || transform.eulerAngles.y >= lastRotationOrder.y + 1)
        {
            if (rotationTries < 5)
            {
                rotationTries++;
                RotateTowardsAngle(lastRotationOrder);
            } else
            {
                rotationTries = 0;
            }
        } else
        {
            rotationTries = 0;
        }

        poiManager.CheckPOIVisibility();
        StartCoroutine(CheckIfMapStillRotates());
    }

    private IEnumerator CheckIfMapStillRotates()
    {
        rotationCounter++;
        yield return new WaitForSeconds(0.1f);
        rotationCounter--;
        if (rotationCounter == 0)
        {
            closestPOI = null;
        }
    }

    public void MoveTheMap(Vector2 steerStickDirection, bool movePOIs, bool disregardMoveMapPower, bool skipSteerStickDirectionCheck = false)
    {
        if (steerStickDirection.x < 0.1f && steerStickDirection.y < 0.1f && steerStickDirection.x > -0.1f && steerStickDirection.y > -0.1f &&
            !skipSteerStickDirectionCheck) { return; }

        Vector2 movement = new Vector2(steerStickDirection.x, steerStickDirection.y) * moveMapPower * SettingsManager.moveMapSpeedFactor;
        if (disregardMoveMapPower)
        {
            movement = steerStickDirection;
        }

        Vector3 newPos = offset;
        newPos.x += movement.x;
        newPos.z += movement.y;
        SetMapToNewPos(newPos, movePOIs);
    }

    public void SetMapToNewPos(Vector3 newPos, bool movePOIs, bool ignoreIsTweeningCheck = false)
    {
        if (!ignoreIsTweeningCheck)
        {
            if (isTweening) { return; }
        }

        Vector3 oldOffset = offset;
        offset = newPos;

        if (miniMap)
        {
            miniMap.MovePlayerIndication(transform, offset);
        }

        poiManager.SetExtraOffset(offset);

        Vector3 movement = offset - oldOffset;
        if (movePOIs)
        {
            poiManager.MovePOIs(new Vector3(movement.x, 0, movement.z));
        }

        CheckIfMapIsStillOnTable();

        Vector3 newMapPos = transform.position;
        newMapPos.x += movement.x;
        newMapPos.z += movement.z;
        transform.position = newMapPos;
    }

    public virtual void ChangeMapScale(float changedScale)
    {
        if (changedScale < 0 && transform.localScale.x == minimumScale || changedScale > 0 && transform.localScale.x == maximumScale) { return; }

        float amountOfChangedScale = changedScale * SettingsManager.scaleMapFactor * oldScale.x;
        Vector3 nextScale = transform.localScale;
        nextScale.x += amountOfChangedScale;
        nextScale.z += amountOfChangedScale;

        if (nextScale.x < minimumScale)
        {
            nextScale.x = minimumScale;
            nextScale.z = minimumScale;
        } else if (nextScale.x > maximumScale)
        {
            nextScale.x = maximumScale;
            nextScale.z = maximumScale;
        }

        SetScaleOfMap(nextScale, true, amountOfChangedScale);
    }

    public void ChangeMapScaleToChosenScale(Vector3 chosenScale, bool ignoreIsTweeningCheck = false)
    {
        SetScaleOfMap(chosenScale, false, 0, ignoreIsTweeningCheck);
    }

    private void SetScaleOfMap(Vector3 nextScale, bool limitMovement, float changedScale, bool ignoreIsTweeningCheck = false)
    {
        if (!ignoreIsTweeningCheck)
        {
            if (isTweening) { return; }
        }

        transform.localScale = nextScale;
        oldScale = nextScale;
        maxTileOffset = originalMaxTileOffset * nextScale.x - table.localScale.z / 2;
        nextScale.y = nextScale.z;
        poiManager.SetPOIsScale(nextScale);
        if (miniMap)
        {
            miniMap.ScalePlayerIndication(nextScale);
        }

        if (changedScale != 0)
        {
            Vector3 mapPosDiff = table.position - transform.position;
            Vector2 playerMapPart = Vector2.zero;
            if (mapPosDiff.x < -0.1f)
            {
                playerMapPart.x = -1;
            }
            else if (mapPosDiff.x > 0.1f)
            {
                playerMapPart.x = 1;
            }

            if (mapPosDiff.z < -0.1f)
            {
                playerMapPart.y = -1;
            }
            else if (mapPosDiff.z > 0.1f)
            {
                playerMapPart.y = 1;
            }

            Vector2 movement = playerMapPart;
            float mapScaler = scaleMapCorrecter;
            if (changedScale < 0)
            {
                mapScaler *= 1.5f;
            }
            movement *= mapScaler * Mathf.Clamp(changedScale, -0.02f, 0.03f);
            MoveTheMap(movement, limitMovement, true, true);
        }
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

    public void SetNewMapCenter(Vector2d newCenter, bool playerMapControlCheck = true)
    {
        if (playerMapControlCheck)
        {
            if (!ffaMap && !ownPlayer.playerIsInControlOfMap)
            {
                return;
            }
        }

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
        if (miniMap)
        {
            miniMap.CopyMap(abstractMap, poiManager.allPOIs, table, poiManager.locationCoordinates, poiManager.poiHits, poiManager.poiScale,
            poiManager.transform.eulerAngles);
        }
        ChangeMapScaleToChosenScale(oldScale);

        if(!ffaMap && ownPlayer.playerIsInControlOfMap)
        {
            ownPlayer.CmdSetNewMapCenter(ownPlayer.playerNumber, newCenter);
        }
    }

    public Vector2d RetrieveMapCenter()
    {
        return abstractMap.CenterLatitudeLongitude;
    }

    private void TwoControllerControls()
    {
        if (inputDevices[0].TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 steerStickInput) && steerStickInput != Vector2.zero)
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

            steerStickInput.x *= -1;
            steerStickInput.y *= -1;

            MoveTheMap(steerStickInput, true, false);
        }

        if (lineVisual.colorGradient.alphaKeys[0].alpha != 0 || hands[0].grabTimer > 0 )
        {
            return;
        }

        if (inputDevices[0].TryGetFeatureValue(CommonUsages.primaryButton, out bool AButton) && AButton)
        {
            ChangeMapScale(scalePower);
        }
        else if (inputDevices[0].TryGetFeatureValue(CommonUsages.secondaryButton, out bool BButton) && BButton)
        {
            ChangeMapScale(-scalePower);
        }

        if (inputDevices[0].TryGetFeatureValue(CommonUsages.trigger, out float triggerButton) && triggerButton > 0.1f)
        {
            RotateMap(rotationPower);
        }
        else if (inputDevices[0].TryGetFeatureValue(CommonUsages.grip, out float gripButton) && gripButton > 0.1f)
        {
            RotateMap(-rotationPower);
        }
    }

    private void GrabControllers()
    {
        inputDevices.Clear();
        AddControllersToList(rightCharacteristics);
        AddControllersToList(leftCharacteristics);

        if (inputDevices.Count == 2)
        {
            for (int i = 0; i < grabbebleObjects.Count; i++)
            {
                grabbebleObjects[i].SetInputDevices(inputDevices, hands, handRays, miniMap);
            }
        }
    }

    private void AddControllersToList(InputDeviceCharacteristics characteristics)
    {
        List<InputDevice> inputDeviceList = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, inputDeviceList);
        if (inputDeviceList.Count > 0)
        {
            if (inputDeviceList[0].isValid)
            {
                inputDevices.Add(inputDeviceList[0]);
            }
        }
    }

    private void ComputerControls()
    {
        if (Input.GetKey(KeyCode.Z) && !isTweening)
        {
            RotateMap(rotationPower);
        }
        else if (Input.GetKey(KeyCode.X) && !isTweening)
        {
            RotateMap(-rotationPower);
        }

        if (Input.GetKey(KeyCode.RightArrow) && !isTweening)
        {
            MoveTheMap(new Vector2(-1, 0), true, false);
        }

        if (Input.GetKey(KeyCode.LeftArrow) && !isTweening)
        {
            MoveTheMap(new Vector2(1, 0), true, false);
        }

        if (Input.GetKey(KeyCode.UpArrow) && !isTweening)
        {
            MoveTheMap(new Vector2(0, -1), true, false);
        }

        if (Input.GetKey(KeyCode.DownArrow) && !isTweening)
        {
            MoveTheMap(new Vector2(0, 1), true, false);
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
