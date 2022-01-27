using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectHolder : MonoBehaviour
{
    /// <summary>
    /// Set all variables that playerconnection needs to grab to function
    /// </summary>

    public TMP_Text chooseSeatTitle = null, mapOwnerText = null;

    public BackToStartPositionButton backToStartPositionButton = null;

    public POISelectionDropdown poiSelectionDropdown = null;

    public PlayerMapControlDropdown playerMapControlDropdown = null;
}
