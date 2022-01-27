using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Mapbox.Unity.Map.TileProviders;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;

public class MiniMap : MonoBehaviour
{
    [SerializeField]
    private Vector3 scaleOfMiniMap = Vector3.zero, rotOfMiniMap = Vector3.zero, offsetFromMiddleOFWall = Vector3.zero;

    [SerializeField]
    private GameObject playerIndicationOfMiniMapPrefab = null, emptyPlane = null;

    [SerializeField, Tooltip("Abstractmap-UnityTileSize * 10")]
    private float maxTileOffset = 600f;

    private GameObject currentPlayerIndicationOfMiniMap = null;

    private Vector3 currentPlayerIndicationOffset = new Vector3(0, 0.02f, 0), basePlayerIndicationScale = Vector3.zero;

    private float scaleChange = 1;

    private List<GameObject> allClonedPOIs = new List<GameObject>();

    /// <summary>
    /// Create an exact copy of the current map, without any of the mapbox script on there
    /// </summary>

    public void CopyMap(AbstractMap map, List<GameObject> allPOIs, Transform table, List<Vector2d> coordinatesOfPOIs, List<int> poiAmountOfHits,
        float poiScale, Vector3 playerRot)
    {
        allClonedPOIs.Clear();

        transform.eulerAngles = Vector3.zero;
        transform.localScale = Vector3.one;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // Loop throught all map parts
        for (int i = 0; i < map.transform.childCount; i++)
        {
            if (map.transform.GetChild(i).GetComponent<RangeTileProvider>()) { continue; }
            Transform copyOfMapPart = Instantiate(emptyPlane.transform);

            // Create the map material required for this map part
            Material mapMat = new Material(emptyPlane.GetComponent<MeshRenderer>().sharedMaterial);
            mapMat.mainTexture = map.transform.GetChild(i).GetComponent<UnityTile>().GetRasterData();
            copyOfMapPart.GetComponent<MeshRenderer>().material = mapMat;

            // Set map part position and scale
            copyOfMapPart.SetParent(transform);
            copyOfMapPart.localPosition = map.transform.GetChild(i).localPosition;
            copyOfMapPart.localScale = Vector3.one * 6;
        }

        // Copy all POI's
        for (int i = 0; i < allPOIs.Count; i++)
        {
            GameObject copyOfPOI = Instantiate(allPOIs[i], transform.position, Quaternion.identity);

            // Remove all components except the visuals
            for (int j = copyOfPOI.transform.childCount - 1; j >= 0; j--)
            {
                Destroy(copyOfPOI.transform.GetChild(j).gameObject);
            }
            Destroy(copyOfPOI.GetComponent<POIText>());
            Destroy(copyOfPOI.GetComponent<XRGrabInteractable>());
            Destroy(copyOfPOI.GetComponent<Rigidbody>());
            Destroy(copyOfPOI.GetComponent<CapsuleCollider>());
            copyOfPOI.tag = "Untagged";

            // Set scale and position
            copyOfPOI.transform.localScale = new Vector3(poiScale, 0.1f * poiAmountOfHits[i], poiScale) * 2;

            copyOfPOI.transform.SetParent(transform);
            Vector3 copyOfPOIPos = Conversions.GeoToWorldPosition(coordinatesOfPOIs[i], map.CenterMercator, map.WorldRelativeScale).ToVector3xz();
            copyOfPOI.transform.localPosition += new Vector3(copyOfPOIPos.x, 0, copyOfPOIPos.z);

            SetActiveStateOfPOI(copyOfPOI);

            allClonedPOIs.Add(copyOfPOI);
        }

        // Create an player indication and set its position and scale
        currentPlayerIndicationOfMiniMap = Instantiate(playerIndicationOfMiniMapPrefab);
        currentPlayerIndicationOfMiniMap.transform.localScale = table.localScale / 10;
        currentPlayerIndicationOfMiniMap.transform.SetParent(transform);
        basePlayerIndicationScale = currentPlayerIndicationOfMiniMap.transform.localScale;
        currentPlayerIndicationOfMiniMap.transform.localPosition = Vector3.zero + currentPlayerIndicationOffset;

        transform.localScale = scaleOfMiniMap;
        RotateMiniMap(map.transform.localEulerAngles, playerRot);
    }

    /// <summary>
    /// Check whether the POI is on the minimap, deactivate it if it isnt
    /// </summary>

    private void SetActiveStateOfPOI(GameObject copyOfPOI)
    {
        if (copyOfPOI.transform.localPosition.x < -maxTileOffset || copyOfPOI.transform.localPosition.x > maxTileOffset ||
                copyOfPOI.transform.localPosition.z < -maxTileOffset || copyOfPOI.transform.localPosition.z > maxTileOffset)
        {
            copyOfPOI.SetActive(false);
        }
        else
        {
            copyOfPOI.SetActive(true);
        }
    }

    /// <summary>
    /// Set active state of POI's, check whether they are on the map if set to true (POI's outside of the map should 
    /// always be invisible)
    /// </summary>

    public void SetActiveStateOfPOIs(List<bool> poiActiveStates)
    {
        for (int i = 0; i < allClonedPOIs.Count; i++)
        {
            allClonedPOIs[i].SetActive(poiActiveStates[i]);
            if (poiActiveStates[i])
            {
                SetActiveStateOfPOI(allClonedPOIs[i]);
            }
        }
    }

    /// <summary>
    /// Move the player indication based on what the local position of the table is compared to the map
    /// </summary>

    public void MovePlayerIndication(Transform map, Vector3 offset)
    {
        if (!currentPlayerIndicationOfMiniMap) { return; }
        Vector3 distance = -offset;
        distance.y = 0;
        Vector3 relativePosition = Vector3.zero;
        relativePosition.x = Vector3.Dot(distance, map.right.normalized);
        relativePosition.z = Vector3.Dot(distance, map.forward.normalized);

        currentPlayerIndicationOfMiniMap.transform.localPosition = (relativePosition * 100 / 2) * scaleChange + currentPlayerIndicationOffset;
    }

    /// <summary>
    /// Set the rotation of the minimap based on the new rotation given and what the position of the player is around the table
    /// (else it would be upside down while for the player it was right side up)
    /// </summary>

    public void RotateMiniMap(Vector3 newRotation, Vector3 playerRot)
    {
        if (!currentPlayerIndicationOfMiniMap) { return; }

        transform.localEulerAngles = new Vector3(-newRotation.y + (playerRot.y - 90), rotOfMiniMap.y, rotOfMiniMap.z);
        currentPlayerIndicationOfMiniMap.transform.eulerAngles = rotOfMiniMap;
    }

    /// <summary>
    /// Change the scale of the player indication based on its original scale and new scale given
    /// </summary>

    public void ScalePlayerIndication(Vector3 newScale)
    {
        if (!currentPlayerIndicationOfMiniMap) { return; }

        scaleChange = 1 / newScale.x;
        currentPlayerIndicationOfMiniMap.transform.localScale = basePlayerIndicationScale * scaleChange;
    }

    /// <summary>
    /// Set local position on wall (calculation wouldnt work)
    /// </summary>

    public void PlaceMiniMapOnWall()
    {
        transform.localPosition = offsetFromMiddleOFWall;
    }

    /// <summary>
    /// Change base Y rot, so it stays flat on wall
    /// </summary>

    public void ChangeBaseRot(float newYRot)
    {
        rotOfMiniMap = new Vector3(0, newYRot, rotOfMiniMap.z);
        transform.eulerAngles = rotOfMiniMap;
    }
}
