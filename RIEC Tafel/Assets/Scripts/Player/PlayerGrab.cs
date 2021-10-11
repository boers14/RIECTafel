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

    private bool followHand = false;

    [SerializeField]
    private LayerMask mask = 0;

    private Transform grabbedObject = null, grabbedParentObject = null;

    private float grabCooldown = 0, cooldown = 0.35f;

    private void Start()
    {
        inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
        lineRenderer = GetComponent<XRInteractorLineVisual>();
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
            if (followHand)
            {
                grabbedObject.position = transform.position;
                grabbedObject.rotation = transform.rotation;
            }

            if (inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger) && trigger && grabCooldown <= 0)
            {
                OnDetach();
            }
        }
        else
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, mask))
            {
                if (inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger) && trigger && grabCooldown <= 0)
                {
                    OnGrab(hit.transform);
                }
            }
        }

        grabCooldown -= Time.deltaTime;
    }

    private void OnGrab(Transform grabbedObject)
    {
        grabCooldown = cooldown;
        this.grabbedObject = grabbedObject;

        if (grabbedObject.GetComponent<XRGrabInteractable>())
        {
            followHand = false;
            grabbedObject.GetComponent<XRGrabInteractable>().selectEntered.Invoke(new SelectEnterEventArgs());
            iTween.MoveTo(grabbedObject.gameObject, iTween.Hash("position", transform.position, "time",
                grabbedObject.GetComponent<XRGrabInteractable>().attachEaseInTime, "easetype", iTween.EaseType.linear, 
                "oncomplete", "StartGrabbedObjectFollowHand", "oncompletetarget", gameObject));
        }

        if (grabbedObject.tag == "DontDisableLineOnGrab")
        {
            GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        else if (grabbedObject.tag != "DontDisableHandOnGrab")
        {
            lineRenderer.enabled = false;
            GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
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
        grabbedObject.SetParent(grabbedParentObject);
        grabCooldown = cooldown;
        if (grabbedObject.GetComponent<XRGrabInteractable>())
        {
            grabbedObject.GetComponent<XRGrabInteractable>().selectExited.Invoke(new SelectExitEventArgs());
        }
        grabbedObject = null;
        lineRenderer.enabled = true;
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
    }
}
