using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeetingButton : MonoBehaviour
{
    public GameObject objectToAcivate = null, objectToDeactivate = null;

    [System.NonSerialized]
    public Button button = null;

    public virtual void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ActivateMeetingSet);
    }

    private void ActivateMeetingSet()
    {
        objectToAcivate.SetActive(!objectToAcivate.activeSelf);
        objectToDeactivate.SetActive(false);
    }
}
