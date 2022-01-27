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

    [System.NonSerialized]
    public Button button = null;

    /// <summary>
    /// Grab start position and apply it to the map functions whenever the start button is clicked
    /// </summary>

    private void Start()
    {
        mapMovement = map.GetComponent<MoveMap>();
        startPosition = map.CenterLatitudeLongitude;
        button = GetComponent<Button>();
        button.onClick.AddListener(ChangeMapPositionAndScale);
    }

    private void ChangeMapPositionAndScale()
    {
        mapMovement.ChangeMapScaleToChosenScale(Vector3.one);
        mapMovement.SetNewMapCenter(startPosition);
    }
}
