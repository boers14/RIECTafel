using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LearnUIButton : MonoBehaviour
{
    [System.NonSerialized]
    public bool hasBeenClicked = false;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(SetBeingClicked);
    }

    private void SetBeingClicked()
    {
        hasBeenClicked = true;
    }
}
