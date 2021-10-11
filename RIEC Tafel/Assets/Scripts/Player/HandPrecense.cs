using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandPrecense : MonoBehaviour
{
    private InputDevice inputDevice;

    [SerializeField]
    private GameObject handPrefab = null;

    private GameObject handController = null;

    private Animator handAnimator = null;

    [SerializeField]
    private InputDeviceCharacteristics characteristics = InputDeviceCharacteristics.None;

    private void Start()
    {
        InitializeController();
    }

    private void Update()
    {
        if (!inputDevice.isValid)
        {
            InitializeController();
            return;
        }

        UpdateHandAnimations();
    }

    private void UpdateHandAnimations()
    {
        if (inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Grip", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }

    private void InitializeController()
    {
        inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);

        if (inputDevice.isValid)
        {
            handController = Instantiate(handPrefab, transform);
            handAnimator = handController.GetComponent<Animator>();
        }
    }
}
