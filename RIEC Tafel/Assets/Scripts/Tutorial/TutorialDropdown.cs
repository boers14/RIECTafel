using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDropdown : DropdownSelection
{
    [System.NonSerialized]
    public bool scrolledWithControllerInput = false;

    public override void UpdateScrollbarValue(Vector2 steerStickInput)
    {
        base.UpdateScrollbarValue(steerStickInput);
        scrolledWithControllerInput = true;
    }
}
