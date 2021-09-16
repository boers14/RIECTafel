using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Utils;
using Mapbox.Unity.Map;

public class BackToStartPositionButton : MonoBehaviour
{
    [SerializeField]
    private AbstractMap map = null;

    [System.NonSerialized]
    public Vector2d startPosition = Vector2d.zero;

    private MoveMap mapMovement = null;

    private void Start()
    {
        mapMovement = map.GetComponent<MoveMap>();
        startPosition = map.CenterLatitudeLongitude;
        GetComponent<Button>().onClick.AddListener(ChangeMapPositionAndScale);
    }

    private void ChangeMapPositionAndScale()
    {
        mapMovement.ChangeMapScaleToOne();
        mapMovement.SetNewMapCenter(startPosition);
    }
}
