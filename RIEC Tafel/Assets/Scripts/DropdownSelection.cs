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

    public virtual void Start()
    {
        inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Update()
    {
        if (!inputDevice.isValid)
        {
            inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);
            return;
        }

        if (dropdown.transform.childCount == 4)
        {
            if (scrollBar == null)
            {
                scrollBar = GetComponentInChildren<Scrollbar>();
            }
            else
            {
                if (inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 steerStickInput) && steerStickInput != Vector2.zero)
                {
                    if (dropdown.options.Count == 0) { return; }
                    scrollBar.value = Mathf.Clamp(scrollBar.value + (1 / ((float)dropdown.options.Count * 2) / 10 * steerStickInput.y), 0, 1);
                }
            }
        }
    }
}
