using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTable : MonoBehaviour
{
    [SerializeField]
    private Transform ground = null;

    /// <summary>
    /// Make sure player table is at ground level
    /// </summary>

    private void Start()
    {
        CheckYPosition();
    }

    public void CheckYPosition()
    {
        Vector3 pos = transform.position;
        pos.y = ground.position.y + transform.localScale.y / 2;
        transform.position = pos;
    }
}
