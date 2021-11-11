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

    private List<InputDevice> inputDevices = new List<InputDevice>();

    [SerializeField]
    private int indexOfUIInHandRays = 2;

    private List<PlayerGrab> hands = new List<PlayerGrab>();

    private List<Vector3> prevHandPosses = new List<Vector3>();
    private List<PlayerHandRays> handRays = new List<PlayerHandRays>();

    [System.NonSerialized]
    public bool isGrabbed = false;

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
    }

    public virtual void Update()
    {
        if (inputDevices.Count < 2)
        {
            return;
        }

        if (hands[0].oneButtonControl)
        {
            int hoverCount = 0;

            for (int i = 0; i < inputDevices.Count; i++)
            {
                if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton)
                {
                    if (handRays[i].objectsAreHovered[indexOfUIInHandRays])
                    {
                        hoverCount++;
                    }
                }
            }

            for (int i = 0; i < inputDevices.Count; i++)
            {
                if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton)
                {
                    if (handRays[i].objectsAreHovered[indexOfUIInHandRays])
                    {
                        if (hoverCount != inputDevices.Count)
                        {
                            Vector3 movement = hands[i].transform.position - prevHandPosses[i];
                            movement.y *= -1000;
                            movement.x *= -1000;
                            movement.z = 0;
                            MoveImage(movement, null, Vector3.zero, 0, false);
                        }
                        else
                        {
                            float oldDist = Vector3.Distance(prevHandPosses[0], prevHandPosses[1]);
                            float newDist = Vector3.Distance(hands[0].transform.position, hands[1].transform.position);
                            ChangeImageScale((newDist - oldDist), null, Vector3.zero, 0);
                        }
                    }
                }

                prevHandPosses[i] = hands[i].transform.position;
            }
        }
        else
        {
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

    public virtual void ChangeImageScale(float scalePower, GameObject image, Vector3 originalPosition, float extraYMovement)
    {
        if (!image) { return; }

        RectTransform imageRectTransform = image.GetComponent<RectTransform>();
        if (scalePower < 0 && imageRectTransform.localScale.x == minimumScale ||
            scalePower > 0 && imageRectTransform.localScale.x == maximumScale) { return; }

        Vector3 oldScale = imageRectTransform.localScale;
        Vector3 nextScale = imageRectTransform.localScale;
        nextScale.x += scalePower * oldScale.x;
        nextScale.y += scalePower * oldScale.y;

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

        MoveImage(Vector2.one, image, originalPosition, extraYMovement, true);
    } 

    public virtual void MoveImage(Vector2 steerStickInput, GameObject image, Vector3 originalPosition, float extraYMovement, bool nullifyMovement)
    {
        if (steerStickInput.x < 0.1f && steerStickInput.y < 0.1f && steerStickInput.x > -0.1f && steerStickInput.y > -0.1f || !image) { return; }

        if (nullifyMovement)
        {
            steerStickInput = Vector2.zero;
        }

        RectTransform imageRectTransform = image.GetComponent<RectTransform>();
        Vector3 newPos = imageRectTransform.localPosition;
        float scaleFactor = imageRectTransform.localScale.x - 1;
        float regularScaleFactor = imageRectTransform.localScale.x;

        float xMovementFactor = scaleFactor * imageRectTransform.sizeDelta.x / 2;
        newPos.x += -steerStickInput.x * movementPower * regularScaleFactor;
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

    private void TurnGravityBackOn()
    {
        rigidbody.useGravity = true;
        rigidbody.velocity = Vector3.zero;
        collider.enabled = true;
        isPlayingTween = false;
    }

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
        collider.enabled = false;
        isGrabbed = true;
    }

    public virtual void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
    {
        isGrabbed = false;
        transform.localScale = originalScale;
        rigidbody.useGravity = true;
        ReturnToPos();
    }

    public void SetInputDevices(List<InputDevice> inputDevices, List<PlayerGrab> hands, List<PlayerHandRays> handRays)
    {
        this.inputDevices = inputDevices;
        this.hands = hands;
        this.handRays = handRays;
    }
}
