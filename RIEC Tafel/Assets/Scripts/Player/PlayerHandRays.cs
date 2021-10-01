using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHandRays : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> objectsToHit = new List<GameObject>();

    [SerializeField]
    private List<UnityEvent> objectHoverEnterEvents = new List<UnityEvent>(), objectHoverExitEvents = new List<UnityEvent>();

    private List<bool> objectsAreHovered = new List<bool>();
    private List<int> frameCounters = new List<int>();

    [SerializeField]
    private LayerMask mask = 0;

    private void Start()
    {
        for (int i = 0; i < objectsToHit.Count; i++)
        {
            objectsAreHovered.Add(false);
            frameCounters.Add(0);
        }
    } 

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, mask))
        {
            if (objectsToHit.Contains(hit.transform.gameObject))
            {
                int index = objectsToHit.IndexOf(hit.transform.gameObject);
                if (!objectsAreHovered[index])
                {
                    objectsAreHovered[index] = true;
                    objectHoverEnterEvents[index].Invoke();
                    frameCounters[index] = 2;
                } else
                {
                    frameCounters[index] = 2;
                }
            }
        }

        for (int i = 0; i < objectsToHit.Count; i++)
        {
            if (objectsAreHovered[i])
            {
                frameCounters[i]--;
                if (frameCounters[i] <= 0)
                {
                    objectsAreHovered[i] = false;
                    objectHoverExitEvents[i].Invoke();
                }
            }
        }
    }
}
