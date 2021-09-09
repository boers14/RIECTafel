using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTable : MonoBehaviour
{
    [SerializeField]
    private Transform ground = null;

    public void CheckYPosition()
    {
        if (transform.position.y - transform.localScale.y / 2 < ground.position.y)
        {
            Vector3 pos = transform.position;
            pos.y = ground.position.y + transform.localScale.y / 2;
            transform.position = pos;
        }
    }
}
