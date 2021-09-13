using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;

public class VRPlayer : MonoBehaviour
{
    private List<Vector2d> locationCoordinates = new List<Vector2d>();

    [SerializeField]
    private AbstractMap map = null;

    [SerializeField]
    private Transform table = null;

    [System.NonSerialized]
    public GameManager.DataType dataType = GameManager.DataType.Regular;

    [SerializeField]
    private GameObject regularPOI = null, policePOI = null, taxPOI = null, ppoPOI = null, bankPOI = null, emptyTransform = null;

    private Vector3 offset = Vector3.zero;

    private List<GameObject> allPOIs = new List<GameObject>();

    private void Start()
    {
        offset = table.transform.position;
        offset.y += table.transform.localScale.y / 2;
    }

    public void SetLocationData(List<List<string>> locationData, List<string> dataTypes)
    {
        locationCoordinates.Clear();
        for (int i = 0; i < allPOIs.Count; i++)
        {
            Destroy(allPOIs[i]);
        }
        allPOIs.Clear();
        GameObject rotationObject = Instantiate(emptyTransform, map.transform.position, Quaternion.identity);

        for (int i = 0; i < locationData.Count; i++)
        {
            GameManager.DataType dataType = (GameManager.DataType)System.Enum.Parse(typeof(GameManager.DataType), dataTypes[i]);
            List<string> locations = locationData[i];

            for (int j = 0; j < locations.Count; j++)
            {
                Vector2d coordinate = map.ReturnCoordinateFromString(locations[j]);
                Vector3 position = Conversions.GeoToWorldPosition(coordinate, map.CenterMercator, map.WorldRelativeScale).ToVector3xz();
                position = offset + (position * 0.02f);
                GameObject POI = null;

                switch (dataType)
                {
                    case GameManager.DataType.Regular:
                        POI = Instantiate(regularPOI, position, map.transform.rotation);
                        break;
                    case GameManager.DataType.Tax:
                        POI = Instantiate(taxPOI, position, map.transform.rotation);
                        break;
                    case GameManager.DataType.Police:
                        POI = Instantiate(policePOI, position, map.transform.rotation);
                        break;
                    case GameManager.DataType.PPO:
                        POI = Instantiate(ppoPOI, position, map.transform.rotation);
                        break;
                    case GameManager.DataType.Bank:
                        POI = Instantiate(bankPOI, position, map.transform.rotation);
                        break;
                }

                allPOIs.Add(POI);
                POI.transform.SetParent(rotationObject.transform);
                locationCoordinates.Add(coordinate);
            }
        }

        Vector3 newRot = map.transform.eulerAngles;
        rotationObject.transform.eulerAngles = newRot;

        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].transform.SetParent(map.transform);
        }
        Destroy(rotationObject);

        CheckPOIVisibility();
    }

    public void CheckPOIVisibility()
    {
        for (int i = 0; i < allPOIs.Count; i++)
        {
            if (allPOIs[i].transform.position.x < table.position.x - table.localScale.x / 2 ||
                allPOIs[i].transform.position.x > table.position.x + table.localScale.x / 2 ||
                allPOIs[i].transform.position.z < table.position.z - table.localScale.z / 2 ||
                allPOIs[i].transform.position.z > table.position.z + table.localScale.z / 2)
            {
                allPOIs[i].SetActive(false);
            } else
            {
                allPOIs[i].SetActive(true);
            }
        }
    }
}
