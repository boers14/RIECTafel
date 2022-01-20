using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.Events;

public class GrabbebleObjects : MonoBehaviour
{
    private XRGrabInteractable grabInteractable = null;

    private Vector3 originalPos = Vector3.zero, originalRot = Vector3.zero, originalScale = Vector3.zero;

    private new Rigidbody rigidbody = null;

    private new Collider collider = null;

    private bool isPlayingTween = false;

    [SerializeField]
    private float distanceFactor = 0.3f;

    public float scalePower = 0.1f, minimumScale = 1, maximumScale = 5, movementPower = 0.1f;

    [SerializeField, Tooltip("Not required to set with map around")]
    private InputDeviceCharacteristics rightCharacteristics = 0, leftCharacteristics = 0;

    private List<InputDevice> inputDevices = new List<InputDevice>();

    [SerializeField]
    private PlayerHandsRayInteractor connectedCanvasObject = null;

    [SerializeField, Tooltip("Not required to set with map around")]
    private List<PlayerGrab> hands = new List<PlayerGrab>();

    private List<Vector3> prevHandPosses = new List<Vector3>();
    private List<PlayerHandRays> handRays = new List<PlayerHandRays>();

    [System.NonSerialized]
    public bool isGrabbed = false;

    private MiniMap miniMap = null;

    private GameSceneSettingsButton settingsButton = null;

    /// <summary>
    /// Initialize all variables not set with serialize fields
    /// </summary>

    public virtual void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            prevHandPosses.Add(Vector3.zero);
        }

        originalScale = transform.localScale;

        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();

        grabInteractable = GetComponent<XRGrabInteractable>();
        SelectEnterEventArgs selectEnterEventArgs = new SelectEnterEventArgs();
        grabInteractable.selectEntered.AddListener((selectEnterEventArgs) => OnGrabEnter(selectEnterEventArgs, true));
        grabInteractable.selectExited.AddListener(OnSelectExit);

        settingsButton = FindObjectOfType<GameSceneSettingsButton>();

        if (!FindObjectOfType<MoveMap>())
        {
            GrabControllers();
        }
    }

    /// <summary>
    /// Perform hand control functions if that is set in setting else perform controller functions.
    /// </summary>

    public virtual void Update()
    {
        if (inputDevices.Count < 2)
        {
            if (!FindObjectOfType<MoveMap>())
            {
                GrabControllers();
            }
            return;
        }

        // Hand controls
        if (hands[0].oneButtonControl)
        {
            // Check if hovered by two controllers and both controllers have the primary button down
            int hoverCount = 0;

            for (int i = 0; i < inputDevices.Count; i++)
            {
                if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton)
                {
                    if (handRays[i].hoveredObjects.Contains(connectedCanvasObject))
                    {
                        hoverCount++;
                    }
                }
            }

            for (int i = 0; i < inputDevices.Count; i++)
            {
                if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton)
                {
                    if (handRays[i].hoveredObjects.Contains(connectedCanvasObject))
                    {
                        // Move object if not hovered by both controllers
                        if (hoverCount != inputDevices.Count)
                        {
                            Vector3 movement = hands[i].transform.position - prevHandPosses[i];
                            movement.y *= -1000;
                            movement.x *= -1000;
                            movement.z = 0;
                            MoveImage(movement, null, Vector3.zero, 0, false);
                        }
                        else
                        // Scale object if hovered by both controllers
                        {
                            float oldDist = Vector3.Distance(prevHandPosses[0], prevHandPosses[1]);
                            float newDist = Vector3.Distance(hands[0].transform.position, hands[1].transform.position);
                            ChangeImageScale((newDist - oldDist), null, Vector3.zero, 0);
                        }
                    }
                }

                // Update previous handposses for moving and scaling objects
                prevHandPosses[i] = hands[i].transform.position;
            }
        }
        else
        {
            // Check for input and if correct input perform function
            if (inputDevices[1].TryGetFeatureValue(CommonUsages.primaryButton, out bool XButton) && XButton)
            {
                ChangeImageScale(scalePower, null, Vector3.zero, 0);
            }
            else if (inputDevices[1].TryGetFeatureValue(CommonUsages.secondaryButton, out bool YButton) && YButton)
            {
                ChangeImageScale(-scalePower, null, Vector3.zero, 0);
            }

            if (inputDevices[1].TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 steerStickInput) && steerStickInput != Vector2.zero)
            {
                MoveImage(steerStickInput, null, Vector3.zero, 0, false);
            }
        }
    }

    /// <summary>
    /// Change the scale of the ui object based on scale power.
    /// </summary>

    public virtual void ChangeImageScale(float scalePower, GameObject image, Vector3 originalPosition, float extraYMovement)
    {
        if (!image) { return; }

        RectTransform imageRectTransform = image.GetComponent<RectTransform>();
        if (scalePower < 0 && imageRectTransform.localScale.x == minimumScale ||
            scalePower > 0 && imageRectTransform.localScale.x == maximumScale) { return; }

        Vector3 oldScale = imageRectTransform.localScale;
        Vector3 nextScale = imageRectTransform.localScale;
        nextScale.x += scalePower * oldScale.x * SettingsManager.scaleGrabbedObjectFactor;
        nextScale.y += scalePower * oldScale.y * SettingsManager.scaleGrabbedObjectFactor;

        if (nextScale.x < minimumScale)
        {
            nextScale.x = minimumScale;
            nextScale.y = minimumScale;
        }
        else if (nextScale.x > maximumScale)
        {
            nextScale.x = maximumScale;
            nextScale.y = maximumScale;
        }

        imageRectTransform.localScale = nextScale;

        // Make sure objects stays within bounderies
        MoveImage(Vector2.one, image, originalPosition, extraYMovement, true);
    }

    /// <summary>
    /// Move ui object based on input en movement power.
    /// </summary>

    public virtual void MoveImage(Vector2 steerStickInput, GameObject image, Vector3 originalPosition, float extraYMovement, bool nullifyMovement)
    {
        if (steerStickInput.x < 0.1f && steerStickInput.y < 0.1f && steerStickInput.x > -0.1f && steerStickInput.y > -0.1f || !image) { return; }

        // For letting objects stay within bounderies.
        if (nullifyMovement)
        {
            steerStickInput = Vector2.zero;
        }

        steerStickInput *= SettingsManager.moveGrabbedObjectSpeedFactor;

        RectTransform imageRectTransform = image.GetComponent<RectTransform>();
        Vector3 newPos = imageRectTransform.localPosition;
        float scaleFactor = imageRectTransform.localScale.x - 1;
        float regularScaleFactor = imageRectTransform.localScale.x;

        float xMovementFactor = scaleFactor * imageRectTransform.sizeDelta.x / 2;
        newPos.x += -steerStickInput.x * movementPower * regularScaleFactor;

        // Bounderies check for x-axis
        if (newPos.x < originalPosition.x - xMovementFactor)
        {
            newPos.x = originalPosition.x - xMovementFactor;
        }
        else if (newPos.x > originalPosition.x + xMovementFactor)
        {
            newPos.x = originalPosition.x + xMovementFactor;
        }

        float yMovementFactor = scaleFactor * imageRectTransform.sizeDelta.y / 2;
        newPos.y += -steerStickInput.y * movementPower * regularScaleFactor;

        // Bounderies check for y-axis
        if (newPos.y < originalPosition.y - yMovementFactor)
        {
            newPos.y = originalPosition.y - yMovementFactor;
        }
        else if (newPos.y > -originalPosition.y + yMovementFactor + imageRectTransform.sizeDelta.y / extraYMovement)
        {
            newPos.y = -originalPosition.y + yMovementFactor + imageRectTransform.sizeDelta.y / extraYMovement;
        }

        imageRectTransform.localPosition = newPos;
    }

    /// <summary>
    /// Move the object back to its original position after being dropped using tweening
    /// </summary>

    private void ReturnToPos()
    {
        float time = Vector3.Distance(transform.position, originalPos) * distanceFactor;
        originalPos.y += 0.075f;
        iTween.MoveTo(gameObject, iTween.Hash("position", originalPos, "time", time, "easetype", iTween.EaseType.linear,
            "oncomplete", "TurnGravityBackOn", "oncompletetarget", gameObject));
        iTween.RotateTo(gameObject, iTween.Hash("rotation", originalRot, "time", time * 0.8f, "easetype", iTween.EaseType.linear));
        rigidbody.useGravity = false;
        isPlayingTween = true;
    }

    /// <summary>
    /// Turn the gravity of the object back on
    /// </summary>

    private void TurnGravityBackOn()
    {
        rigidbody.useGravity = true;
        rigidbody.velocity = Vector3.zero;
        collider.enabled = true;
        isPlayingTween = false;
    }

    /// <summary>
    /// Perform this function when object is being grabbed, turn off object that would block vision of canvas objects.
    /// </summary>

    public virtual void OnGrabEnter(SelectEnterEventArgs selectEnterEventArgs, bool setOriginalVectors)
    {
        if (setOriginalVectors)
        {
            originalPos = transform.position;
            originalRot = transform.eulerAngles;
        }

        if (isPlayingTween)
        {
            iTween.Stop(gameObject);
            TurnGravityBackOn();
        }

        EnableCanvasBlockingObjects(false);

        collider.enabled = false;
        isGrabbed = true;
    }

    /// <summary>
    /// Perform this function when object is dropped, turn all turned off object back on again.
    /// </summary>

    public virtual void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
    {
        EnableCanvasBlockingObjects(true);

        isGrabbed = false;
        transform.localScale = originalScale;
        rigidbody.useGravity = true;

        // Move object back to original position
        ReturnToPos();
    }

    /// <summary>
    /// Turn canvas blocking objects on/ off
    /// </summary>

    private void EnableCanvasBlockingObjects(bool enabled)
    {
        if (miniMap)
        {
            miniMap.gameObject.SetActive(enabled);
        }

        if (settingsButton)
        {
            settingsButton.gameObject.SetActive(enabled);
        }
    }

    /// <summary>
    /// Get all required stats from the movemap function to properly function
    /// </summary>

    public void SetInputDevices(List<InputDevice> inputDevices, List<PlayerGrab> hands, List<PlayerHandRays> handRays, MiniMap miniMap)
    {
        this.miniMap = miniMap;
        this.inputDevices = inputDevices;
        this.hands = hands;
        this.handRays = handRays;
    }

    /// <summary>
    /// Get controllers based on characteristics, order matters for controller based controls
    /// </summary>

    private void GrabControllers()
    {
        inputDevices.Clear();
        handRays.Clear();
        AddControllersToList(rightCharacteristics);
        AddControllersToList(leftCharacteristics);

        for (int i = 0; i < hands.Count; i++)
        {
            handRays.Add(hands[i].GetComponent<PlayerHandRays>());
        }
    }

    /// <summary>
    /// Add controller to list with given characteristics
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
}
