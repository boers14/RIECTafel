using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class PlayerGrab : MonoBehaviour
{
    [SerializeField]
    private InputDeviceCharacteristics characteristics = InputDeviceCharacteristics.None;
    private InputDevice inputDevice;

    private XRInteractorLineVisual lineRenderer = null;

    public bool oneButtonControl = false;

    private bool followHand = false, pressedButton = false;

    [SerializeField]
    private LayerMask mask = 0;

    [SerializeField]
    private bool grabConnectionManagerStats = true;

    public float grabTimer { get; set; } = 0;

    private float grabTimerCooldown = 0.5f;

    private Transform grabbedParentObject = null, grabbedObject = null;

    /// <summary>
    /// Initialize variables, doesnt always need to check with settings manager, in tutorials the controls version is set
    /// </summary>

    private void Start()
    {
        inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
        lineRenderer = GetComponent<XRInteractorLineVisual>();

        if (grabConnectionManagerStats)
        {
            oneButtonControl = SettingsManager.oneHandControls;
        }
    }

    /// <summary>
    /// If hand has grabbed object, let the grabbed object follow the position and rotation of the hand. Also check if the button 
    /// is pressed drop the object.
    /// If there is no grabbed object shoot a ray and it hit something check if the grip is down. In that case grab the object.
    /// </summary>

    private void Update()
    {
        if (!inputDevice.isValid)
        {
            inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
            return;
        }

        if (grabbedObject)
        {
            // Set a grab timer so a dropped object cant instantly be grabbed again
            grabTimer = grabTimerCooldown;
            if (followHand)
            {
                grabbedObject.position = transform.position;
                grabbedObject.rotation = transform.rotation;
            }

            if (inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool grip) && grip && !pressedButton)
            {
                OnDetach();
            } else if (!grip)
            {
                // Player first needs to release the grip button again before he can drop the object
                pressedButton = false;
            }
        }
        else
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, mask))
            {
                if (inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool grip) && grip && grabTimer < 0)
                {
                    OnGrab(hit.transform);
                }
            }
        }

        grabTimer -= Time.deltaTime;
    }

    /// <summary>
    /// If the object contains a grab interacteble, perform the connected functions of the select entered. 
    /// Disable linerenderer if grabbed object contains the DontDisableLineOnGrab tag
    /// </summary>

    private void OnGrab(Transform grabbedObject)
    {
        pressedButton = true;
        this.grabbedObject = grabbedObject;

        XRGrabInteractable grabInteractable = grabbedObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable)
        {
            grabInteractable.selectEntered.Invoke(new SelectEnterEventArgs());
            // Move object to hand only if track position is set to true in XRGrabInteractable
            if (grabInteractable.trackPosition)
            {
                iTween.MoveTo(grabbedObject.gameObject, iTween.Hash("position", transform.position, "time",
                grabInteractable.attachEaseInTime, "easetype", iTween.EaseType.linear,
                "oncomplete", "StartGrabbedObjectFollowHand", "oncompletetarget", gameObject));
            }
        }
        
        if (grabbedObject.tag != "DontDisableLineOnGrab")
        {
            lineRenderer.enabled = false;
        }

        EnableHandModel(false);

        // Remember the parent so it can be set back again later
        grabbedParentObject = grabbedObject.parent;
        grabbedObject.SetParent(null);
    }

    /// <summary>
    /// Follow hand set  to true after tween is complete, so the object doesnt instantly go to the hand
    /// </summary>

    private void StartGrabbedObjectFollowHand()
    {
        followHand = true;
    }

    /// <summary>
    /// Reset the hand to what it was before grabbing an object
    /// </summary>

    private void OnDetach()
    {
        followHand = false;
        grabbedObject.SetParent(grabbedParentObject);
        if (grabbedObject.GetComponent<XRGrabInteractable>())
        {
            // Invoke the select exit event before setting the object to null
            grabbedObject.GetComponent<XRGrabInteractable>().selectExited.Invoke(new SelectExitEventArgs());
        }
        grabbedObject = null;
        lineRenderer.enabled = true;
        EnableHandModel(true);
    }

    /// <summary>
    /// (de)Activate hand model. Check for skinned meshrenderer for regular hand, regular mesh renderer for tutorial hand
    /// </summary>

    private void EnableHandModel(bool enabled)
    {
        if (GetComponentInChildren<SkinnedMeshRenderer>() != null)
        {
            GetComponentInChildren<SkinnedMeshRenderer>().enabled = enabled;
        } else
        {
            GetComponentInChildren<MeshRenderer>().enabled = enabled;
            DisableAllSpriteRenderers(transform, enabled);
        }
    }

    /// <summary>
    /// On the tutorial hand also disable all arrows to the buttons
    /// </summary>

    private void DisableAllSpriteRenderers(Transform child, bool enabled)
    {
        for (int i = 0; i < child.childCount; i++)
        {
            if (child.GetChild(i).GetComponent<SpriteRenderer>() != null)
            {
                child.GetChild(i).GetComponent<SpriteRenderer>().enabled = enabled;
            }

            DisableAllSpriteRenderers(child.GetChild(i), enabled);
        }
    }
}
