using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class GrabbebleObjects : MonoBehaviour
{
    private XRGrabInteractable grabInteractable = null;

    private Vector3 originalPos = Vector3.zero, originalRot = Vector3.zero, originalScale = Vector3.zero;

    private new Rigidbody rigidbody = null;

    private new Collider collider = null;

    private bool isPlayingTween = false;

    [SerializeField]
    private float distanceFactor = 0.3f;

    private void Start()
    {
        originalScale = transform.localScale;

        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();

        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabEnter);
        grabInteractable.selectExited.AddListener(OnSelectExit);
    }

    private void ReturnToPos()
    {
        float time = Vector3.Distance(transform.position, originalPos) * distanceFactor;
        iTween.MoveTo(gameObject, iTween.Hash("position", originalPos, "time", time, "easetype", iTween.EaseType.linear));
        iTween.RotateTo(gameObject, iTween.Hash("rotation", originalRot, "time", time, "easetype", iTween.EaseType.linear,
            "oncomplete", "TurnGravityBackOn", "oncompletetarget", gameObject));
        rigidbody.useGravity = false;
        isPlayingTween = true;
    }

    private void TurnGravityBackOn()
    {
        rigidbody.useGravity = true;
        rigidbody.velocity = Vector3.zero;
        collider.enabled = true;
        isPlayingTween = false;
    }

    private void OnGrabEnter(SelectEnterEventArgs selectEnterEventArgs)
    {
        originalPos = transform.position;
        originalRot = transform.eulerAngles;

        if (isPlayingTween)
        {
            iTween.Stop(gameObject);
            TurnGravityBackOn();
        }
        collider.enabled = false;
    }

    private void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
    {
        //
        transform.localScale = originalScale;
        rigidbody.useGravity = true;
        ReturnToPos();
    }
}
