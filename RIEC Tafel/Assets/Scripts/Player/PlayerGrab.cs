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

    private void Start()
    {
        inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
        lineRenderer = GetComponent<XRInteractorLineVisual>();

        if (grabConnectionManagerStats)
        {
            oneButtonControl = SettingsManager.oneHandControls;
        }
    }

    private void Update()
    {
        if (!inputDevice.isValid)
        {
            inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
            return;
        }

        if (grabbedObject)
        {
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

    private void OnGrab(Transform grabbedObject)
    {
        pressedButton = true;
        this.grabbedObject = grabbedObject;

        if (grabbedObject.GetComponent<XRGrabInteractable>())
        {
            grabbedObject.GetComponent<XRGrabInteractable>().selectEntered.Invoke(new SelectEnterEventArgs());
            if (grabbedObject.GetComponent<XRGrabInteractable>().trackPosition)
            {
                iTween.MoveTo(grabbedObject.gameObject, iTween.Hash("position", transform.position, "time",
                grabbedObject.GetComponent<XRGrabInteractable>().attachEaseInTime, "easetype", iTween.EaseType.linear,
                "oncomplete", "StartGrabbedObjectFollowHand", "oncompletetarget", gameObject));
            }
        }

        if (grabbedObject.tag == "DontDisableLineOnGrab")
        {
            EnableHandModel(false);
        }
        else if (grabbedObject.tag != "DontDisableHandOnGrab")
        {
            lineRenderer.enabled = false;
            EnableHandModel(false);
        }

        grabbedParentObject = grabbedObject.parent;
        grabbedObject.SetParent(null);
    }

    private void StartGrabbedObjectFollowHand()
    {
        followHand = true;
    }

    private void OnDetach()
    {
        followHand = false;
        grabbedObject.SetParent(grabbedParentObject);
        if (grabbedObject.GetComponent<XRGrabInteractable>())
        {
            grabbedObject.GetComponent<XRGrabInteractable>().selectExited.Invoke(new SelectExitEventArgs());
        }
        grabbedObject = null;
        lineRenderer.enabled = true;
        EnableHandModel(true);
    }

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
