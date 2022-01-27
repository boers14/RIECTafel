using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialControllerTextOfArrows : MonoBehaviour
{
    private Vector3 rotOffset = new Vector3(0, 180, 0);
    private Transform playerTransform = null;

    /// <summary>
    /// Grab player head transform
    /// </summary>

    private void Start()
    {
        playerTransform = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        RotateText();
    }

    private void OnEnable()
    {
        RotateText();
    }

    /// <summary>
    /// Rotate to look at the main camera in the scene
    /// </summary>

    private void RotateText()
    {
        transform.LookAt(playerTransform);
        transform.eulerAngles += rotOffset;
        Vector3 newRot = transform.eulerAngles;
        newRot.x *= -1;
        transform.eulerAngles = newRot;
    }
}
