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

    private float originalScale = 0, originalYvalue = 0, maxStandardDeviation = 0.19f,
        minStandardDeviation = 0.425f;

    private bool expanded = false;

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
        if (expanded) { return; }

        expanded = true;
        float fontSize = poiText.fontSize;
        poiText.enableAutoSizing = false;
        poiText.fontSize = fontSize;
        poiText.gameObject.AddComponent<ContentSizeFitter>();
        poiText.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        poiText.text += "\n" + textExtend;
        StartCoroutine(ChangeTextPos());
    }

    private IEnumerator ChangeTextPos()
    {
        yield return new WaitForEndOfFrame();
        Vector3 newPos = originalPos;
        newPos.y += poiText.rectTransform.sizeDelta.y / (amountOfHits * 2) / (transform.localScale.x / originalScale);
        poiText.rectTransform.localPosition = newPos;
    }

    private void UnExpandText(HoverExitEventArgs args)
    {
        expanded = false;
        poiText.text = normalText;
        Destroy(poiText.GetComponent<ContentSizeFitter>());
        poiText.enableAutoSizing = true;
        poiText.rectTransform.localPosition = originalPos;
    }

    public void UpdateScaleOfPoi(Vector3 newScale, int amountOfHits, float minMapScale, float maxMapScale, float poiScale)
    {
        if (originalPos == Vector3.zero)
        {
            Start();

            float completeReduction = 0;
            float addedReduction = 0.27f;
            float maxStandardPowerReduction = 2.6f;
            for (int i = 0; i < amountOfHits - 1; i++)
            {
                completeReduction += addedReduction;
                addedReduction /= 3;
                maxStandardDeviation /= 1.15f;
                minStandardDeviation /= maxStandardPowerReduction;
                maxStandardPowerReduction -= 0.35f;
            }
            originalPos.y -= completeReduction;
            this.amountOfHits = amountOfHits;

            originalYvalue = originalPos.y;
            originalScale = newScale.x * poiScale;
        }

        float addedY = 0;
        if (newScale.x * poiScale < originalScale - 0.01f)
        {
            addedY = maxStandardDeviation * ((1f - newScale.x) / (1f - minMapScale));
        }
        else if (newScale.x * poiScale > originalScale)
        {
            addedY -= minStandardDeviation * (newScale.x / maxMapScale);
        }
        originalPos.y = originalYvalue + addedY;

        poiText.transform.SetParent(null);
        newScale.y = newScale.y * 0.1f * amountOfHits;
        newScale.x *= poiScale;
        newScale.z *= poiScale;
        transform.localScale = newScale;
        poiText.transform.SetParent(transform);
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
