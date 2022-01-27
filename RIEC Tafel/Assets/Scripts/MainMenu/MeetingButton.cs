using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeetingButton : MonoBehaviour
{
    public GameObject objectToAcivate = null, objectToDeactivate = null;

    [System.NonSerialized]
    public Button button = null;

    /// <summary>
    /// Switch active state of objectToAcivate and always deactivate objectToDeactivate
    /// </summary>

    public virtual void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ActivateMeetingSet);
    }

    public virtual void ActivateMeetingSet()
    {
        if (objectToAcivate != null)
        {
            objectToAcivate.SetActive(!objectToAcivate.activeSelf);
        }

        if (objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(false);
        }
    }
}
