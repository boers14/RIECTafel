using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShiftKey : KeyBoardKey
{
    /// <summary>
    /// Swap keyboard state when button is activated
    /// </summary>

    public override void KeyFunction()
    {
        keyBoard.SwapKeyBoardState();
    }
}
