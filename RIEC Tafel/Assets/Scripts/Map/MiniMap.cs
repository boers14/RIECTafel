using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Mapbox.Unity.Map.TileProviders;
using Mapbox.Unity.MeshGeneration.Data;

public class MiniMap : MonoBehaviour
{
    [SerializeField]
    private Vector3 scaleOfMiniMap = Vector3.zero, rotOfMiniMap = Vector3.zero;

    [SerializeField]
    private GameObject playerIndicationOfMiniMapPrefab = null, emptyPlane = null;

    private GameObject currentPlayerIndicationOfMiniMap = null;

    public void CopyMap(Transform map, List<GameObject> allPOIs, Transform table)
    {
        transform.eulerAngles = Vector3.zero;
        transform.localScale = Vector3.one;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < map.childCount; i++)
        {
            if (map.GetChild(i).GetComponent<RangeTileProvider>()) { continue; }
            Transform copyOfMapPart = Instantiate(emptyPlane.transform);

            Material mapMat = new Material(emptyPlane.GetComponent<MeshRenderer>().sharedMaterial);
            mapMat.mainTexture = map.GetChild(i).GetComponent<UnityTile>().GetRasterData();
            copyOfMapPart.GetComponent<MeshRenderer>().material = mapMat;

            copyOfMapPart.SetParent(transform);
            copyOfMapPart.localPosition = map.GetChild(i).localPosition;
            copyOfMapPart.localScale = Vector3.one * 6;
        }

        for (int i = 0; i < allPOIs.Count; i++)
        {
            GameObject copyOfPOI = Instantiate(allPOIs[i]);

            for (int j = copyOfPOI.transform.childCount - 1; j >= 0; j--)
            {
                Destroy(copyOfPOI.transform.GetChild(j).gameObject);
            }
            Destroy(copyOfPOI.GetComponent<POIText>());
            Destroy(copyOfPOI.GetComponent<XRGrabInteractable>());
            Destroy(copyOfPOI.GetComponent<Rigidbody>());
            Destroy(copyOfPOI.GetComponent<CapsuleCollider>());
            copyOfPOI.tag = "Untagged";

            copyOfPOI.transform.SetParent(transform);
            Vector3 copyOfPOIPos = (allPOIs[i].transform.position - table.position) / transform.parent.localScale.x;
            copyOfPOI.transform.localPosition = new Vector3(copyOfPOIPos.x, 0, copyOfPOIPos.z);
            copyOfPOI.transform.localScale *= 2;
            copyOfPOI.SetActive(true);
        }

        currentPlayerIndicationOfMiniMap = Instantiate(playerIndicationOfMiniMapPrefab);
        currentPlayerIndicationOfMiniMap.transform.localScale = table.localScale / 10;
        currentPlayerIndicationOfMiniMap.transform.SetParent(transform);
        currentPlayerIndicationOfMiniMap.transform.localPosition = Vector3.zero + new Vector3(0, 0.001f, 0);

        transform.localScale = scaleOfMiniMap;
        RotateMiniMap(map.localEulerAngles);
    }

    public void MovePlayerIndication(Transform map, Vector3 offset)
    {
        if (!currentPlayerIndicationOfMiniMap) { return; }
        Vector3 distance = -offset;
        distance.y = 0;
        Vector3 relativePosition = Vector3.zero;
        relativePosition.x = Vector3.Dot(distance, map.right.normalized);
        relativePosition.z = Vector3.Dot(distance, map.forward.normalized);

        currentPlayerIndicationOfMiniMap.transform.localPosition = relativePosition * 100 / 2;
    }

    public void RotateMiniMap(Vector3 newRotation)
    {
        transform.localEulerAngles = new Vector3(-newRotation.y - 90, rotOfMiniMap.y, rotOfMiniMap.z);
        if (currentPlayerIndicationOfMiniMap)
        {
            currentPlayerIndicationOfMiniMap.transform.eulerAngles = rotOfMiniMap;
        }
    }
}
