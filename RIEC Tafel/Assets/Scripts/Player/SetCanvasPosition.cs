using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCanvasPosition : MonoBehaviour
{
    [SerializeField]
    private Transform canvas = null;

    [SerializeField]
    private LayerMask layer = 0;

    [SerializeField]
    private PlayerTable playerTable = null;

    private void Start()
    {
        ChangeCanvasPosition();
    }

    public void ChangeCanvasPosition()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit wallRay, Mathf.Infinity, layer))
        {
            canvas.position = wallRay.transform.position + wallRay.transform.up * 0.15f;
            Vector3 canvasRotation = Vector3.zero;
            canvasRotation.y = wallRay.transform.eulerAngles.y + 90;
            canvas.eulerAngles = canvasRotation;
        }

        playerTable.CheckYPosition();
    }
}
