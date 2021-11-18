using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialControllerArrows : MonoBehaviour
{
    [System.NonSerialized]
    public float arrowMoveTweenTime = 1.75f;
    
    [SerializeField]
    private float arrowMovementSpace = 0.03f;

    [SerializeField]
    private Transform emptyTransform = null;

    private Transform targetStartPos = null;

    private bool isGoingUpwards = true;

    [System.NonSerialized]
    public GrabbebleObjects connectedGameObject = null;

    [SerializeField]
    private bool continuesTweening = true;

    private void Start()
    {
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

    private void CreateTargetStartPos()
    {
        if (targetStartPos == null)
        {
            targetStartPos = Instantiate(emptyTransform, transform.position, transform.rotation);
            targetStartPos.SetParent(transform.parent);
        }
    }

    public void DisplayTween()
    {
        if (!continuesTweening)
        {
            CreateTargetStartPos();
            transform.position = targetStartPos.position - transform.right * arrowMovementSpace;
        }

        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", arrowMoveTweenTime, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdatePosition", "oncomplete", "SwitchTweenPosition", "oncompletetarget", gameObject));
    }

    private void UpdatePosition(float val)
    {
        Vector3 startPos = targetStartPos.position;
        Vector3 expectedEndPos = startPos - transform.right * arrowMovementSpace;

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
    }

    private void SwitchTweenPosition()
    {
        if (continuesTweening)
        {
            isGoingUpwards = !isGoingUpwards;
            DisplayTween();
        }
    }

    public void SetTweenPosition(Transform newTargetStartPos)
    {
        targetStartPos = newTargetStartPos;
        transform.position = new Vector3(newTargetStartPos.position.x, transform.position.y, newTargetStartPos.position.z);
    }
}
