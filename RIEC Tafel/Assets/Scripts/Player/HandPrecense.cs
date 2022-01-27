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

    /// <summary>
    /// Initialize controller
    /// </summary>

    private void Start()
    {
        InitializeController();
    }

    /// <summary>
    /// Only update if there is a controller. Update hand animations.
    /// </summary>

    private void Update()
    {
        if (!inputDevice.isValid)
        {
            InitializeController();
            return;
        }

        UpdateHandAnimations();
    }

    /// <summary>
    /// Set hand animations equal to the amount that the grip is pressed
    /// </summary>

    private void UpdateHandAnimations()
    {
        if (inputDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }

    /// <summary>
    /// Fetch the input device and create the hand prefab if the device is valid 
    /// </summary>

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
