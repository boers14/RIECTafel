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

        //if (inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton)
        //{
        //    print(primaryButton);
        //}
        //Grip is under button trigger upper button
        //if (inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float trigger) && trigger >= 0.1f)
        //{
        //    print(trigger);
        //}

        //if (inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axisInput) && axisInput != Vector2.zero)
        //{
        //    print(axisInput);
        //}
    }

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
