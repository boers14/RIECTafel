using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialKeyBoardKey : KeyBoardKey
{
    [SerializeField]
    private SpatialKeyType spatialKeyType = SpatialKeyType.Enter;

    private enum SpatialKeyType
    {
        Enter,
        Space
    }

    private string addedString = "";

    /// <summary>
    /// Decide what the key does based on its type
    /// </summary>

    public override void Start()
    {
        base.Start();

        switch (spatialKeyType)
        {
            case SpatialKeyType.Enter:
                addedString = "\n";
                break;
            case SpatialKeyType.Space:
                addedString = " ";
                break;
        }
    }

    /// <summary>
    /// Add given string to the text that is currently being edited
    /// </summary>

    public override void KeyFunction()
    {
        currentText.EditText(addedString, false, keyBoard);
    }
}
