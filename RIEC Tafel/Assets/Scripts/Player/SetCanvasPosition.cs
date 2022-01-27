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

    [SerializeField]
    private MoveMap map = null;

    private MiniMap miniMap = null;

    private List<DataExplanations> dataExplanations = new List<DataExplanations>();

    private bool hasSetPlayerRotation = false;

    /// <summary>
    /// Initialize variables
    /// </summary>

    private void Start()
    {
        miniMap = FindObjectOfType<MiniMap>();
        dataExplanations.AddRange(FindObjectsOfType<DataExplanations>());
    }

    /// <summary>
    /// Only works once. Shoot a ray out of the player that only detects the wall layer. Move the minimap and canvas to the position
    /// of the wall that is hit.
    /// </summary>

    public void ChangeCanvasPosition()
    {
        if (hasSetPlayerRotation) { return; }

        hasSetPlayerRotation = true;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit wallRay, Mathf.Infinity, layer))
        {
            // Place canvas a bit in front of the wall
            Vector3 newCanvasPos = wallRay.transform.position + wallRay.transform.up * 0.05f;
            canvas.position = newCanvasPos;

            // Set minimap position on wall
            Transform building = miniMap.transform.parent;
            miniMap.transform.SetParent(wallRay.transform);
            miniMap.PlaceMiniMapOnWall();
            miniMap.transform.SetParent(building);

            // Set canvas rotation flat on wall
            Vector3 canvasRotation = Vector3.zero;
            canvasRotation.y = wallRay.transform.eulerAngles.y + 90;
            canvas.eulerAngles = canvasRotation;
            miniMap.ChangeBaseRot(wallRay.transform.eulerAngles.y);
        }

        // Move playtable to ground level
        playerTable.CheckYPosition();

        // Set base posses for data explanations, so their calculations work properly
        for (int i = 0; i < dataExplanations.Count; i++)
        {
            dataExplanations[i].SetBasePosses();
        }

        // Rotate map towards player
        float diffInYRotation = transform.eulerAngles.y - map.transform.eulerAngles.y;
        map.RotateMap(diffInYRotation, true);
    }
}
