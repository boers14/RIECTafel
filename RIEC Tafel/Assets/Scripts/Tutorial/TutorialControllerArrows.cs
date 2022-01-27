using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialControllerArrows : MonoBehaviour
{
    [System.NonSerialized]
    public float arrowMoveTweenTime = 1.75f;
    
    [SerializeField]
    private float arrowMovementSpace = 0.03f, textDistanceFactor = 1.2f;

    [SerializeField]
    private Transform emptyTransform = null;

    private Transform targetStartPos = null;

    private bool isGoingUpwards = true;

    [System.NonSerialized]
    public GrabbebleObjects connectedGameObject = null;

    [SerializeField]
    private bool continuesTweening = true;

    private TMP_Text buttonText = null;

    /// <summary>
    /// Start the tween of the arrow if it always needs to tween, else make sure it goes down in the tween.
    /// </summary>

    private void Start()
    {
        // Not all arrows have text
        buttonText = GetComponentInChildren<TMP_Text>();
        if (buttonText)
        {
            buttonText.transform.SetParent(transform.parent);
            buttonText.transform.localScale = Vector3.one / 10;
        }

        CreateTargetStartPos();

        if (continuesTweening)
        {
            DisplayTween();
        }
        else
        {
            isGoingUpwards = false;
        }
    }

    /// <summary>
    /// Create a position it needs to tween from and to
    /// </summary>

    private void CreateTargetStartPos()
    {
        if (targetStartPos == null)
        {
            targetStartPos = Instantiate(emptyTransform, transform.position, transform.rotation);
            targetStartPos.SetParent(transform.parent);
        }
    }

    /// <summary>
    /// If the the arrow doesnt continually tweens, reset it to the start position and reset text start position
    /// Start the tween
    /// </summary>

    public void DisplayTween()
    {
        if (!continuesTweening)
        {
            CreateTargetStartPos();
            transform.position = targetStartPos.position - transform.right * arrowMovementSpace;
            SetButtonTextPos();
        }

        if (buttonText)
        {
            buttonText.gameObject.SetActive(true);
        }

        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", arrowMoveTweenTime, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdatePosition", "oncomplete", "SwitchTweenPosition", "oncompletetarget", gameObject));
    }

    /// <summary>
    /// Update og the tween
    /// Update the position of the arrow based on how far the tween is
    /// </summary>

    private void UpdatePosition(float val)
    {
        Vector3 startPos = targetStartPos.position;
        Vector3 expectedEndPos = startPos - transform.right * arrowMovementSpace;

        // if going downwards, switch start and end pos
        if (!isGoingUpwards)
        {
            startPos = targetStartPos.position - transform.right * arrowMovementSpace;
            expectedEndPos = targetStartPos.position;
        }

        Vector3 newPos = transform.position;
        newPos.x = ((1f - val) * startPos.x) + (val * expectedEndPos.x);
        newPos.y = ((1f - val) * startPos.y) + (val * expectedEndPos.y);
        newPos.z = ((1f - val) * startPos.z) + (val * expectedEndPos.z);
        transform.position = newPos;
        SetButtonTextPos();
    }

    /// <summary>
    /// Set the position of the text above the arrow
    /// </summary>

    private void SetButtonTextPos()
    {
        if (buttonText)
        {
            buttonText.transform.position = transform.position + (-transform.right * (transform.localScale.x * textDistanceFactor));
        }
    }

    /// <summary>
    /// On complete of the tween
    /// Switch direction that the tween is going and start the tween again
    /// </summary>

    private void SwitchTweenPosition()
    {
        if (continuesTweening)
        {
            isGoingUpwards = !isGoingUpwards;
            DisplayTween();
        }
    }

    /// <summary>
    /// Change the tween position of the arrow to a new given position
    /// </summary>

    public void SetTweenPosition(Transform newTargetStartPos)
    {
        targetStartPos = newTargetStartPos;
        transform.position = new Vector3(newTargetStartPos.position.x, transform.position.y, newTargetStartPos.position.z);
        SetButtonTextPos();
    }
}
