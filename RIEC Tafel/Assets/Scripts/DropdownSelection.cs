using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;

public class DropdownSelection : MonoBehaviour
{
    [System.NonSerialized]
    public TMP_Dropdown dropdown = null;

    [SerializeField]
    private InputDeviceCharacteristics characteristics = InputDeviceCharacteristics.None;

    private InputDevice inputDevice;

    private Scrollbar scrollBar = null;

    private PlayerGrab hand = null;

    /// <summary>
    /// Initialize variables
    /// </summary>

    public virtual void Start()
    {
        hand = FindObjectOfType<PlayerGrab>();
        inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
        dropdown = GetComponent<TMP_Dropdown>();
    }

    /// <summary>
    /// Move dropdown list based on the user input if the dropdown has content
    /// </summary>

    private void Update()
    {
        if (!inputDevice.isValid)
        {
            inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
            return;
        }

        // Return if it is hand controls
        if (hand)
        {
            if (hand.oneButtonControl)
            {
                return;
            }
        }

        // Check if dropdown is open
        if (dropdown.transform.childCount == 4)
        {
            if (scrollBar == null)
            {
                scrollBar = GetComponentInChildren<Scrollbar>();
            }
            else
            {
                if (inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 steerStickInput) && 
                    steerStickInput != Vector2.zero)
                {
                    if (dropdown.options.Count == 0) { return; }
                    UpdateScrollbarValue(steerStickInput);
                }
            }
        }
    }

    /// <summary>
    /// Move the dropdown based on the y-input of the input of the controller
    /// </summary>

    public virtual void UpdateScrollbarValue(Vector2 steerStickInput)
    {
        scrollBar.value = Mathf.Clamp(scrollBar.value + (1 / ((float)dropdown.options.Count * 2) / 10 * steerStickInput.y), 0, 1);
    }
}
