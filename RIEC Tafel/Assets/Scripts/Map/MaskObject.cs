using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskObject : MonoBehaviour
{
    /// <summary>
    /// Set mask object to correct render queue so its renders before transparent objects
    /// </summary>

    private void Start()
    {
        GetComponent<MeshRenderer>().material.renderQueue = 2001;
    }
}
