using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;

public static class InitializeControllers
{
    /// <summary>
    /// Grabs an input device based on the given characteristics if there is one and returns it
    /// </summary>

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

    /// <summary>
    /// Returns a list of input devices that is in the same order as the handrays given in the list.
    /// Right controller on index of right hand, left controller index on left hand
    /// </summary>

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

    /// <summary>
    /// Adds a valid controller to the list based on the characteristics
    /// </summary>

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
