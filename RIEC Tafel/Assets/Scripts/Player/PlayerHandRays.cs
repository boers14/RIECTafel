using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerHandRays : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> objectsToHit = new List<GameObject>();

    [SerializeField]
    private List<UnityEvent> objectHoverEnterEvents = new List<UnityEvent>(), objectHoverExitEvents = new List<UnityEvent>();

    [SerializeField]
    private List<bool> drawRayLine = new List<bool>();

    [System.NonSerialized]
    public List<bool> objectsAreHovered = new List<bool>();
    private List<int> frameCounters = new List<int>();

    [SerializeField]
    private LayerMask mask = 0;

    private XRInteractorLineVisual lineRenderer = null;

    private PlayerGrab playerGrab = null;

    private void Start()
    {
        for (int i = 0; i < objectsToHit.Count; i++)
        {
            objectsAreHovered.Add(false);
            frameCounters.Add(0);
        }

        lineRenderer = GetComponent<XRInteractorLineVisual>();
        playerGrab = GetComponent<PlayerGrab>();
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
                    if (drawRayLine[index] && playerGrab.oneButtonControl)
                    {
                        ChangeInvalidColorGradientOfLineRenderer(1);
                    }

                    objectsAreHovered[index] = true;
                    if (objectHoverEnterEvents[index] != null)
                    {
                        objectHoverEnterEvents[index].Invoke();
                    }
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

                    if (drawRayLine[i] && playerGrab.oneButtonControl)
                    {
                        ChangeInvalidColorGradientOfLineRenderer(0);
                    }

                    if (objectHoverExitEvents[i] != null)
                    {
                        objectHoverExitEvents[i].Invoke();
                    }
                }
            }
        }
    }

    private void ChangeInvalidColorGradientOfLineRenderer(float alpha)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lineRenderer.invalidColorGradient = gradient;
    }
}
