using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialControllerTextOfArrows : MonoBehaviour
{
    private Vector3 rotOffset = new Vector3(0, 180, 0);
    private Transform playerTransform = null;

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

    private void RotateText()
    {
        transform.LookAt(playerTransform);
        transform.eulerAngles += rotOffset;
        Vector3 newRot = transform.eulerAngles;
        newRot.x *= -1;
        transform.eulerAngles = newRot;
    }
}
