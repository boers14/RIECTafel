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
    public GameManager.DataType dataType = GameManager.DataType.Bank;

    [SerializeField]
    private GameObject regularPOI = null, policePOI = null, taxPOI = null, ppoPOI = null, bankPOI = null;

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
                locationCoordinates.Add(coordinate);
            }
        }
    }
}
