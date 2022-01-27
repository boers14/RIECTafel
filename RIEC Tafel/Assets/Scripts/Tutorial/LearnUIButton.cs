using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LearnUIButton : MonoBehaviour
{
    [System.NonSerialized]
    public bool hasBeenClicked = false;

    /// <summary>
    /// Turns a bool to true when the buttons has been clicked for the tutorial
    /// </summary>

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(SetBeingClicked);
    }

    private void SetBeingClicked()
    {
        hasBeenClicked = true;
    }
}
