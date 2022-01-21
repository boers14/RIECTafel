using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerHandRays : MonoBehaviour
{
    [System.NonSerialized]
    public List<PlayerHandsRayInteractor> hoveredObjects = new List<PlayerHandsRayInteractor>();

    [System.NonSerialized]
    public List<Vector3> hitPoints = new List<Vector3>();

    private List<int> frameCounters = new List<int>();

    [SerializeField]
    private LayerMask mask = 0;

    private XRInteractorLineVisual lineRenderer = null;

    public enum Hand
    {
        Left,
        Right
    }

    public Hand hand = 0;

    private void Start()
    {
        lineRenderer = GetComponent<XRInteractorLineVisual>();
    } 

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, mask))
        {
            PlayerHandsRayInteractor interactor = hit.transform.GetComponent<PlayerHandsRayInteractor>();
            if (interactor)
            {
                if (!hoveredObjects.Contains(interactor))
                {
                    hoveredObjects.Add(interactor);
                    frameCounters.Add(2);
                    hitPoints.Add(hit.point);
                    interactor.objectHoverEnteredEvent.Invoke();
                } else
                {
                    int index = hoveredObjects.IndexOf(interactor);
                    frameCounters[index] = 2;
                    hitPoints[index] = hit.point;
                }
            }
        }

        for (int i = hoveredObjects.Count - 1; i >= 0 ; i--)
        {
            frameCounters[i]--;
            if (frameCounters[i] <= 0)
            {
                PlayerHandsRayInteractor interactor = hoveredObjects[i];
                frameCounters.RemoveAt(i);
                hoveredObjects.RemoveAt(i);
                hitPoints.RemoveAt(i);
                interactor.objectHoverExitedEvent.Invoke();
            }
        }
    }

    public void ChangeInvalidColorGradientOfLineRenderer(float alpha)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lineRenderer.invalidColorGradient = gradient;
    }

    public void ChangeColorGradientOfRayIfHandRayIsHittingRegisteredObject(float alpha)
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, mask))
        {
            PlayerHandsRayInteractor interactor = hit.transform.GetComponent<PlayerHandsRayInteractor>();
            if (hoveredObjects.Contains(interactor))
            {
                if (interactor.drawLineOnOneHandControls)
                {
                    ChangeInvalidColorGradientOfLineRenderer(alpha);
                }
            }
        }
    }
}
