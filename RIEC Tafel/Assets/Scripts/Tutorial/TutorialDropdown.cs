using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDropdown : DropdownSelection
{
    [System.NonSerialized]
    public bool scrolledWithControllerInput = false;

    /// <summary>
    /// Has a bool to let the tutorial manager know that this function has been used (controller controls tutorial step)
    /// </summary>

    public override void UpdateScrollbarValue(Vector2 steerStickInput)
    {
        base.UpdateScrollbarValue(steerStickInput);
        scrolledWithControllerInput = true;
    }
}
