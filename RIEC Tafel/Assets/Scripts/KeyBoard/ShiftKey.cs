using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShiftKey : KeyBoardKey
{
    public override void KeyFunction()
    {
        keyBoard.SwapKeyBoardState();
    }
}
