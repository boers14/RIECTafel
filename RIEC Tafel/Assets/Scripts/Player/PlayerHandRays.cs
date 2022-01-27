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

    /// <summary>
    /// Initialize variables
    /// </summary>

    private void Start()
    {
        lineRenderer = GetComponent<XRInteractorLineVisual>();
    }

    /// <summary>
    /// Performs a raycats that checks whether an object is an interactor and then adds it to a list off currently hit interactors
    /// After 2 frames of an objects not being hit the object is removed from the list
    /// </summary>

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
                    // Perform the on hovered events
                    interactor.objectHoverEnteredEvent.Invoke();
                } else
                {
                    // Keep resetting frame counter if object is being hit
                    int index = hoveredObjects.IndexOf(interactor);
                    frameCounters[index] = 2;
                    hitPoints[index] = hit.point;
                }
            }
        }

        // Count down so it doesnt crash when removing an item from the list
        for (int i = hoveredObjects.Count - 1; i >= 0 ; i--)
        {
            frameCounters[i]--;
            // Remove object from list
            if (frameCounters[i] <= 0)
            {
                PlayerHandsRayInteractor interactor = hoveredObjects[i];
                frameCounters.RemoveAt(i);
                hoveredObjects.RemoveAt(i);
                hitPoints.RemoveAt(i);
                // Call on hover exit event
                interactor.objectHoverExitedEvent.Invoke();
            }
        }
    }

    /// <summary>
    /// Change the invalid color gradient alpha to make the line visible or inviseble
    /// </summary>

    public void ChangeInvalidColorGradientOfLineRenderer(float alpha)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lineRenderer.invalidColorGradient = gradient;
    }

    /// <summary>
    /// Check if an interactor is being hit that would require turning on/ off handlines after switching hand controls in settings
    /// manager
    /// </summary>

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
