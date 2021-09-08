using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;

public static class InitializeControllers
{
    public static InputDevice ReturnInputDeviceBasedOnCharacteristics(InputDeviceCharacteristics characteristics, InputDevice inputDevice)
    {
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, inputDevices);

        if (inputDevices.Count > 0)
        {
            inputDevice = inputDevices[0];
        }

        return inputDevice;
    }
}
