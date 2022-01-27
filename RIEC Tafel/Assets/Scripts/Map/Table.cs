using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    [SerializeField]
    private Transform maskObject = null;

    /// <summary>
    /// Place four masks around the table that block all transparent object from being visible (mostly the map)
    /// </summary>

    private void Start()
    {
        float rotation = 0;

        for (int i = 0; i < 4; i++)
        {
            Transform mask = Instantiate(maskObject, transform.position, Quaternion.identity);
            Vector3 scale = transform.localScale;
            scale.x *= 2;
            scale.y += 0.2f;
            mask.localScale = scale;

            Vector3 rot = Vector3.zero;
            rot.y += rotation;
            mask.transform.eulerAngles = rot;

            Vector3 offset = Vector3.zero;
            switch (i)
            {
                case 0:
                    offset = transform.forward * transform.localScale.x;
                    break;
                case 1:
                    offset = transform.right * transform.localScale.x;
                    break;
                case 2:
                    offset = -transform.forward * transform.localScale.x;
                    break;
                case 3:
                    offset = -transform.right * transform.localScale.x;
                    break;
            }

            mask.position += offset;
            rotation += 90;
            mask.SetParent(transform);
        }
    }
}
