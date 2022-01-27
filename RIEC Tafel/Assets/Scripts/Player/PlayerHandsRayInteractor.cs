using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHandsRayInteractor : MonoBehaviour
{
    // Made my own, because these dont require rigidbodys to be attached
    // Fill these lists with events to activate when on hover enter and on hover exit
    public UnityEvent objectHoverEnteredEvent = new UnityEvent(), objectHoverExitedEvent = new UnityEvent();

    public bool drawLineOnOneHandControls = false;

    private PlayerGrab hands = null;

    private List<PlayerHandRays> handRays = new List<PlayerHandRays>();

    /// <summary>
    /// Initialize variables if handrays should be visible when pointing at the object
    /// </summary>

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

    /// <summary>
    /// Draw handlines if the object is hovered by the player and has hand controls activated
    /// </summary>

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

    /// <summary>
    /// Turn handlines off if the object is no longer hovered and isnt pointing at something else
    /// </summary>

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
