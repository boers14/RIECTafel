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

    public static List<InputDevice> InitializeControllersBasedOnHandRays(List<InputDevice> inputDevices, List<PlayerHandRays> handRays, 
        InputDeviceCharacteristics rightCharacteristics, InputDeviceCharacteristics leftCharacteristics)
    {
        for (int i = 0; i < handRays.Count; i++)
        {
            switch (handRays[i].hand)
            {
                case PlayerHandRays.Hand.Right:
                    FetchController(inputDevices, rightCharacteristics);
                    break;
                case PlayerHandRays.Hand.Left:
                    FetchController(inputDevices, leftCharacteristics);
                    break;
            }
        }

        return inputDevices;
    }

    private static void FetchController(List<InputDevice> actualInputDevices, InputDeviceCharacteristics characteristics)
    {
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, inputDevices);
        if (inputDevices.Count > 0)
        {
            if (inputDevices[0].isValid)
            {
                actualInputDevices.Add(inputDevices[0]);
            }
        }
    }
}
