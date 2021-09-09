using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerGrab : MonoBehaviour
{
    private XRRayInteractor rayInteractor = null;

    private XRInteractorLineVisual lineRenderer = null;

    private void Start()
    {
        lineRenderer = GetComponent<XRInteractorLineVisual>();
        rayInteractor = GetComponent<XRRayInteractor>();
        rayInteractor.selectEntered.AddListener(OnGrab);
        rayInteractor.selectExited.AddListener(OnDetach);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        lineRenderer.enabled = false;
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
    }

    private void OnDetach(SelectExitEventArgs args)
    {
        lineRenderer.enabled = true;
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
    }
}
