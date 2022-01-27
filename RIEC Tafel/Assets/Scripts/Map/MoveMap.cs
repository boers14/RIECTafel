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

    [SerializeField, Tooltip("Abstractmap-UnityTileSize * 2 / 10, this variable always returns 1 if gotten from Abstractmap at runtime")]
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

    /// <summary>
    /// Initialize variables
    /// </summary>

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

    /// <summary>
    /// Perform map movement functions (moving rotating, scaling) with all different controls
    /// If player is the map owner send draw line functions over to other players to show what the players is pointing at
    /// </summary>

    private void Update()
    {
        if (!ownPlayer)
        {
            return;
        }

        // If map scale got bigger then its supposed to then scale the map down to a normal size
        if (!isTweening && transform.localScale.x > maximumScale && transform.parent == originalParent)
        {
            if (transform.localScale.x * originalParent.transform.localScale.x >= minimumScale)
            {
                ChangeMapScaleToChosenScale(transform.localScale * originalParent.transform.localScale.x);
            }
        }

        // Check whether player should send draw line commands
        if (!ffaMap && ownPlayer.playerIsInControlOfMap)
        {
            for (int i = 0; i < handRays.Count; i++)
            {
                if (handRays[i].hoveredObjects.Contains(oneHandControlsInteractorObject))
                {
                    handLinesVisibility[i] = true;
                    // Check whether hands moved or rotated
                    if (prevHandPossesLineRendering[i] != hands[i].transform.position ||
                        prevHandRotationsLineRendering[i] != hands[i].transform.eulerAngles)
                    {
                        // Send the command to draw hand lines for other players
                        ownPlayer.CmdDrawHandLines((int)handRays[i].hand, ownPlayer.playerNumber, 
                            table.InverseTransformPoint(handRays[i].hitPoints[handRays[i].hoveredObjects.
                            IndexOf(oneHandControlsInteractorObject)]), poiManager.transform.eulerAngles.y);
                    }
                } else
                {
                    if (handLinesVisibility[i])
                    {
                        //  Send the command to turn off handlines
                        handLinesVisibility[i] = false;
                        ownPlayer.CmdTurnOffhandLine((int)handRays[i].hand, ownPlayer.playerNumber);
                    }
                }

                prevHandPossesLineRendering[i] = hands[i].transform.position;
                prevHandRotationsLineRendering[i] = hands[i].transform.eulerAngles;
            }
        }

        // The player needs to be in control of the map or it needs to be a free for all map to use the map functions
        if (isTweening || !ffaMap && !ownPlayer.playerIsInControlOfMap)
        {
            return;
        }

        // These are for if the player has no VR headset on
        ComputerControls();

        if (inputDevices.Count < 2)
        {
            GrabControllers();
            return;
        }

        // Hand controls
        if (hands[0].oneButtonControl)
        {
            // Check whether the players has both primary buttons down and is aiming them at the table
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
                        // Check if player is trying to scale the map
                        if (hoverCount != inputDevices.Count && playerWasScalingTimer < 0)
                        {
                            // Rotate the map if the grip is pressed
                            if (inputDevices[i].TryGetFeatureValue(CommonUsages.grip, out float gripButton) && gripButton > 0.1f)
                            {
                                // Based on the direction the controller moves rotate the map
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
                                // Based on where the controller moves, move the map
                                Vector3 movement = hands[i].transform.position - prevHandPosses[i];
                                movement.y = movement.z * 200;
                                movement.x *= 150;
                                movement.z = 0;
                                MoveTheMap(movement, true, false);
                            }
                        } else if (hoverCount == inputDevices.Count)
                        {
                            // Based on if the controller get close or further away from each other scale the map
                            playerWasScalingTimer = playerWasScalingCooldown;
                            float oldDist = Vector3.Distance(prevHandPosses[0], prevHandPosses[1]);
                            float newDist = Vector3.Distance(hands[0].transform.position, hands[1].transform.position);
                            ChangeMapScale((newDist - oldDist));
                        }
                    }
                }

                // Update variables
                prevHandDirections[i] = hands[i].transform.eulerAngles - prevHandRotations[i].eulerAngles;
                prevHandPosses[i] = hands[i].transform.position;
                prevHandRotations[i] = hands[i].transform.rotation;
            }

            playerWasScalingTimer -= Time.deltaTime;
        }
        else
        {
            // Controller controls
            TwoControllerControls();
        }
    }

    /// <summary>
    /// Check in which direction the hand moves in order to decide whether to rotate left or right
    /// </summary>

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

    /// <summary>
    /// Add the given rotation to the current rotation if the rotation isnt to big
    /// </summary>

    public void RotateMap(float rotation, bool ignoreRotationCheck = false)
    {
        if (!ignoreRotationCheck)
        {
            if (rotation > 5 || rotation < -5) { return; }
        }

        Vector3 nextRotation = transform.eulerAngles;

        // If the added rotation skips the rotation check then dont amplify it with the settings (this is for precisely setting the
        // the rotation)
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

    /// <summary>
    /// Set the rotation of the map to a given angle, chooses the POI closest to the center of the map to rotate around
    /// </summary>

    public void RotateTowardsAngle(Vector3 nextRotation, bool ignoreIsTweeningCheck = false)
    {
        if (!ignoreIsTweeningCheck)
        {
            if (isTweening) { return; }
        }

        isTweening = true;

        // If there is no closest POI grab one, if there is still not one then use the table as rotation point
        if (!closestPOI)
        {
            closestPOI = poiManager.ReturnClosestPOIToTransform(table.position);
            if (!closestPOI)
            {
                closestPOI = table;
            }
        }

        // Use a empty transform as an object to rotate around
        pivotObject = Instantiate(emptyTransform, closestPOI.position, transform.rotation);
        transform.SetParent(pivotObject.transform);
        poiManager.ParentPOIs(pivotObject.transform);

        lastRotationOrder = nextRotation;

        // Use iTween to rotate, just setting the rotation directly bugged the map and it wouldnt show the correct angle
        iTween.RotateTo(pivotObject, iTween.Hash("rotation", nextRotation, "time", 0.01f, "easetype", iTween.EaseType.linear,
            "oncomplete", "CanTweenAgain", "oncompletetarget", gameObject));
    }

    /// <summary>
    /// Perform this function when the rotation tween is done, update variables that changed throught the rotation (including 
    /// POI's and minimap)
    /// </summary>

    private void CanTweenAgain()
    {
        isTweening = false;
        poiManager.ParentPOIs(null);
        transform.SetParent(originalParent);
        Destroy(pivotObject);

        offset.x = transform.position.x - table.position.x;
        offset.z = transform.position.z - table.position.z;

        if (miniMap)
        {
            miniMap.RotateMiniMap(transform.localEulerAngles, poiManager.transform.eulerAngles);
        }

        // If the rotation failed to correctly be set to the last given rotation, try to rotate to that given rotation again
        if (transform.eulerAngles.y <= lastRotationOrder.y - 1 || transform.eulerAngles.y >= lastRotationOrder.y + 1)
        {
            // To prevent it from going on infinetly, stop at 5 tries
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

    /// <summary>
    /// To keep the same POI as pivot point, have a delay before setting the closest POI to null
    /// </summary>

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

    /// <summary>
    /// Move the map by a given steerStickDirection, set a new position based on the current offset
    /// skipSteerStickDirectionCheck if its not user input but used for moving the map after scaling
    /// </summary>

    public void MoveTheMap(Vector2 steerStickDirection, bool movePOIs, bool disregardMoveMapPower, 
        bool skipSteerStickDirectionCheck = false)
    {
        // Make sure that the movement of the map doesnt get too weird if its hand movement and that the map doesnt
        // always start moving if a controller has bugged input
        if (steerStickDirection.x < 0.1f && steerStickDirection.y < 0.1f && steerStickDirection.x > -0.1f 
            && steerStickDirection.y > -0.1f && !skipSteerStickDirectionCheck) { return; }

        Vector2 movement = new Vector2(steerStickDirection.x, steerStickDirection.y) * moveMapPower * 
            SettingsManager.moveMapSpeedFactor;
        if (disregardMoveMapPower)
        {
            movement = steerStickDirection;
        }

        Vector3 newPos = offset;
        newPos.x += movement.x;
        newPos.z += movement.y;
        SetMapToNewPos(newPos, movePOIs);
    }

    /// <summary>
    /// Set the map to a given position, update minimap and POI's based on the new position
    /// Check whether the map is still on the table
    /// </summary>

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

    /// <summary>
    /// Calculate the next vector3 to set as the new map scale. Make sure its within the min/ max map scale bounderies
    /// </summary>

    public virtual void ChangeMapScale(float changedScale)
    {
        if (changedScale < 0 && transform.localScale.x == minimumScale || changedScale > 0 
            && transform.localScale.x == maximumScale) { return; }

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

    /// <summary>
    /// Set the map to a given scale
    /// </summary>

    public void ChangeMapScaleToChosenScale(Vector3 chosenScale, bool ignoreIsTweeningCheck = false)
    {
        SetScaleOfMap(chosenScale, false, 0, ignoreIsTweeningCheck);
    }

    /// <summary>
    /// Set a new scale of the map and also update this new scale for the POI's and the minimap. Update all other required 
    /// variables as well. Try to keep the same map position on this changed scale (TODO: Fix this)
    /// </summary>

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
            // Based on what quadrant of the map the player is on calculate in which direction the map should theoratically move
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

    /// <summary>
    /// Check if the current position of the map didnt move past any of its edges
    /// </summary>

    private void CheckIfMapIsStillOnTable()
    {
        Vector3 offsetValueWithYZero = offset;
        offsetValueWithYZero.y = 0;

        if (Vector3.Distance(Vector3.zero, offsetValueWithYZero) > maxTileOffset)
        {
            SetNewMapCenter(abstractMap.WorldToGeoPosition(table.position));
        }
    }

    /// <summary>
    /// Sets a new map center to a given center and updates to minimap and POI's accordingly
    /// </summary>

    public void SetNewMapCenter(Vector2d newCenter, bool playerMapControlCheck = true)
    {
        if (playerMapControlCheck)
        {
            if (!ffaMap && !ownPlayer.playerIsInControlOfMap)
            {
                return;
            }
        }

        // Reset scale and set offset accordingly. Then caluclate what the map position would be on original scale.
        // This is required to accurately get the new center of the map
        transform.localScale = Vector3.one;
        float regularMaxTileOffset = originalMaxTileOffset - table.localScale.z / 2;
        float percentageGrowth = maxTileOffset / regularMaxTileOffset;
        offset.x /= percentageGrowth;
        offset.z /= percentageGrowth;
        transform.position = table.position + offset;

        // Set new center and force update
        abstractMap.SetCenterLatitudeLongitude(newCenter);
        abstractMap.UpdateMap();

        // Update POI's and reset position
        offset = Vector3.zero;
        offset.y = table.transform.localScale.y / 2 + 0.01f;
        poiManager.SetExtraOffset(offset);
        poiManager.SetPOIsScale(oldScale);
        transform.position = table.position + offset;

        // Make a new copy of the minimap based on the new look of the map
        if (miniMap)
        {
            miniMap.CopyMap(abstractMap, poiManager.allPOIs, table, poiManager.locationCoordinates, poiManager.poiHits, 
                poiManager.poiScale, poiManager.transform.eulerAngles);
        }
        ChangeMapScaleToChosenScale(oldScale);

        // Make sure all players in the discussion have the same map
        if(!ffaMap && ownPlayer.playerIsInControlOfMap)
        {
            ownPlayer.CmdSetNewMapCenter(ownPlayer.playerNumber, newCenter);
        }
    }

    /// <summary>
    /// Returns the center point of the map in latlong (world coordinates)
    /// </summary>

    public Vector2d RetrieveMapCenter()
    {
        return abstractMap.CenterLatitudeLongitude;
    }

    /// <summary>
    /// The controllers controls for moving, rotating and scaling the map
    /// </summary>

    private void TwoControllerControls()
    {
        if (inputDevices[0].TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 steerStickInput) && steerStickInput != Vector2.zero)
        {
            // Calculate the player rotation in when moving the map (so that it moves correcly based from the perspective of the
            // player)

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

        // Dont perform the other functions if a handray is visible or if the player just grabbed something with his right hand
        if (lineVisual.colorGradient.alphaKeys[0].alpha != 0 || hands[0].grabTimer > 0 )
        {
            return;
        }

        // Input for scaling the map
        if (inputDevices[0].TryGetFeatureValue(CommonUsages.primaryButton, out bool AButton) && AButton)
        {
            ChangeMapScale(scalePower);
        }
        else if (inputDevices[0].TryGetFeatureValue(CommonUsages.secondaryButton, out bool BButton) && BButton)
        {
            ChangeMapScale(-scalePower);
        }

        // Input for rotating the map
        if (inputDevices[0].TryGetFeatureValue(CommonUsages.trigger, out float triggerButton) && triggerButton > 0.1f)
        {
            RotateMap(rotationPower);
        }
        else if (inputDevices[0].TryGetFeatureValue(CommonUsages.grip, out float gripButton) && gripButton > 0.1f)
        {
            RotateMap(-rotationPower);
        }
    }

    /// <summary>
    /// Grab controllers based on characteristics, if both controllers are there set the stats for pickupable objects
    /// </summary>

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

    /// <summary>
    /// Add a controller to the inputdevice list based on a given charactaristic
    /// </summary>

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

    /// <summary>
    /// The controls if there is no VR headset connected, does not change based on player position
    /// </summary>

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
