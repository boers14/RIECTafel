using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHandsRayInteractor : MonoBehaviour
{
    public UnityEvent objectHoverEnteredEvent = new UnityEvent(), objectHoverExitedEvent = new UnityEvent();

    public bool drawLineOnOneHandControls = false;

    private PlayerGrab hands = null;

    private List<PlayerHandRays> handRays = new List<PlayerHandRays>();

    private void Start()
    {
        if (drawLineOnOneHandControls)
        {
            hands = FindObjectOfType<PlayerGrab>();
            handRays.AddRange(FindObjectsOfType<PlayerHandRays>());

            objectHoverEnteredEvent.AddListener(DrawLineOnHoverEnter);
            objectHoverExitedEvent.AddListener(ExitLineDrawOnHoverExit);
        }
    }

    private void DrawLineOnHoverEnter()
    {
        if (!hands.oneButtonControl) { return; }

        for (int i = 0; i < handRays.Count; i++)
        {
            if (handRays[i].hoveredObjects.Contains(this))
            {
                handRays[i].ChangeInvalidColorGradientOfLineRenderer(1);
            }
        }
    }

    private void ExitLineDrawOnHoverExit()
    {
        for (int i = 0; i < handRays.Count; i++)
        {
            if (!handRays[i].hoveredObjects.Contains(this) && handRays[i].hoveredObjects.Count == 0)
            {
                handRays[i].ChangeInvalidColorGradientOfLineRenderer(0);
            }
        }
    }
}
