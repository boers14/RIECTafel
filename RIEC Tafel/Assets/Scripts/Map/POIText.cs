using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class POIText : MonoBehaviour
{
    private TMP_Text poiText = null;

    private string normalText = "", textExtend = "";

    private Vector3 originalPos = Vector3.zero;

    private int amountOfHits = 0;

    private void Start()
    {
        if (originalPos != Vector3.zero) { return; }

        originalPos = poiText.rectTransform.localPosition;
        GetComponent<XRGrabInteractable>().hoverEntered.AddListener(ExpandText);
        GetComponent<XRGrabInteractable>().hoverExited.AddListener(UnExpandText);

        SetPoiText();
    }

    public void SetText(string text, string textExtend)
    {
        SetPoiText();
        normalText = text;
        this.textExtend = textExtend;
        poiText.text = text;
    }

    public void SetTextRotation(Transform playerTransform)
    {
        Quaternion lookRotation = Quaternion.RotateTowards(transform.rotation, playerTransform.rotation, 360);
        lookRotation.x = 0;
        lookRotation.z = 0;
        transform.rotation = lookRotation;
    }

    private void ExpandText(HoverEnterEventArgs args)
    {
        float fontSize = poiText.fontSize;
        poiText.enableWordWrapping = true;
        poiText.enableAutoSizing = false;
        poiText.fontSize = fontSize;
        gameObject.AddComponent<ContentSizeFitter>();
        poiText.text += "\n" + textExtend;
        StartCoroutine(ChangeTextPos());
    }

    private IEnumerator ChangeTextPos()
    {
        yield return new WaitForEndOfFrame();
        CalculateNewTextPos();
    }

    private void CalculateNewTextPos()
    {
        Vector3 newPos = originalPos;
        newPos.y += poiText.rectTransform.sizeDelta.y / amountOfHits;
        poiText.rectTransform.localPosition = newPos;
    }

    public void UpdateScaleOfPoi(Vector3 newScale, int amountOfHits)
    {
        if (originalPos == Vector3.zero)
        {
            Start();

            float completeReduction = 0;
            float addedReduction = 0.27f;
            for (int i = 0; i < amountOfHits - 1; i++)
            {
                completeReduction += addedReduction;
                addedReduction /= 3;
            }
            originalPos.y -= completeReduction;
            this.amountOfHits = amountOfHits;
        }
        
        poiText.transform.SetParent(null);
        newScale.y *= amountOfHits;
        transform.localScale = newScale / 10;
        poiText.transform.SetParent(transform);
        poiText.rectTransform.localPosition = originalPos;
    }

    private void UnExpandText(HoverExitEventArgs args)
    {
        poiText.text = normalText;
        Destroy(gameObject.GetComponent<ContentSizeFitter>());
        poiText.enableWordWrapping = false;
        poiText.enableAutoSizing = true;
        poiText.rectTransform.localPosition = originalPos;
    }

    private void SetPoiText()
    {
        if (!poiText)
        {
            poiText = GetComponentInChildren<TMP_Text>();
        }
    }
}
