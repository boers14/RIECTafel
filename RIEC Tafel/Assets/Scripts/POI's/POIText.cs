using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class POIText : MonoBehaviour
{
    private TMP_Text poiText = null;

    private string normalText = "", textExtend = "";

    private Vector3 originalPos = Vector3.zero;

    private int amountOfHits = 0;

    private float originalScale = 0, originalYvalue = 0, maxStandardDeviation = 0.19f, minStandardDeviation = 0.425f, originalYTextSize = 0, 
        originalTextBoxYScale = 0, canInteractWithControllerTimer = 0, canInteractWithControllerCooldown = 0.75f;

    private bool expanded = false, startUnExpand = false;

    private List<bool> canInteractWithController = new List<bool>();

    [SerializeField]
    private Transform textBox = null;

    private List<InputDevice> inputDevices = new List<InputDevice>();

    private Transform player = null;

    private MoveMap map = null;

    private SpriteRenderer textBoxRenderer = null;

    public virtual void Start()
    {
        if (originalPos != Vector3.zero) { return; }

        textBoxRenderer = textBox.GetComponent<SpriteRenderer>();
        textBox.gameObject.SetActive(false);
        originalPos = poiText.rectTransform.localPosition;
        originalTextBoxYScale = textBox.localScale.y;
        GetComponent<XRGrabInteractable>().hoverEntered.AddListener(ExpandText);
        GetComponent<XRGrabInteractable>().hoverExited.AddListener(StartUnExpandText);

        for (int i = 0; i < 2; i++)
        {
            canInteractWithController.Add(false);
        }

        SetPoiTextComponent();
    }

    private void Update()
    {
        if (!expanded) 
        { 
            for (int i = 0; i < canInteractWithController.Count; i++)
            {
                canInteractWithController[i] = false;
            }
            canInteractWithControllerTimer = canInteractWithControllerCooldown;
            return; 
        }

        for (int i = 0; i < inputDevices.Count; i++)
        {
            if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton && canInteractWithController[i]
                && canInteractWithControllerTimer < 0)
            {
                PullPOIToPlayer(i);
            } else if (!primaryButton)
            {
                canInteractWithController[i] = true;
            }
        }

        if (canInteractWithControllerTimer > 0)
        {
            canInteractWithControllerTimer -= Time.deltaTime;
            if (canInteractWithControllerTimer < 0)
            {
                textBoxRenderer.material.color = Color.white;
            } else
            {
                Color32 cooldownColor = Color.red;
                byte colorValue = (byte)(255 * (1 - (canInteractWithControllerTimer / canInteractWithControllerCooldown)));
                cooldownColor.g = colorValue;
                cooldownColor.b = colorValue;
                textBoxRenderer.material.color = cooldownColor;
            }
        }
    }

    public virtual void PullPOIToPlayer(int index)
    {
        Vector3 newPos = player.position + (player.forward * 1.5f);
        Vector2 diffInPos = Vector2.zero;
        diffInPos.x = newPos.x - transform.position.x;
        diffInPos.y = newPos.z - transform.position.z;
        map.MoveTheMap(diffInPos, true, true);
        canInteractWithController[index] = false;
    }

    public void SetText(string text, string textExtend, Transform player, MoveMap map, List<InputDevice> inputDevices)
    {
        SetPoiTextComponent();
        normalText = text;
        this.textExtend = textExtend;
        poiText.text = text;
        this.player = player;
        this.map = map;
        this.inputDevices = inputDevices;
    }

    public void SetTextRotation(Transform playerTransform)
    {
        Quaternion lookRotation = Quaternion.RotateTowards(transform.rotation, playerTransform.rotation, 360);
        lookRotation.x = 0;
        lookRotation.z = 0;
        transform.rotation = lookRotation;
    }

    public virtual void ExpandText(HoverEnterEventArgs args)
    {
        startUnExpand = false;
        if (expanded) { return; }

        textBox.gameObject.SetActive(true);
        textBoxRenderer.material.color = Color.red;
        expanded = true;
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

        Vector3 newTextboxScale = textBox.localScale;
        newTextboxScale.y = originalTextBoxYScale * (poiText.rectTransform.sizeDelta.y * 1.3f / originalYTextSize);
        textBox.localScale = newTextboxScale;
    }

    private void StartUnExpandText(HoverExitEventArgs args)
    {
        startUnExpand = true;
        StartCoroutine(UnExpandText());
    }

    private IEnumerator UnExpandText()
    {
        yield return new WaitForSeconds(0.1f);
        if (startUnExpand)
        {
            textBox.gameObject.SetActive(false);
            expanded = false;
            poiText.text = normalText;
            Destroy(poiText.GetComponent<ContentSizeFitter>());
            poiText.rectTransform.localPosition = originalPos;
        }
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

    private void SetPoiTextComponent()
    {
        if (!poiText)
        {
            poiText = GetComponentInChildren<TMP_Text>();
            originalYTextSize = poiText.rectTransform.sizeDelta.y;
        }
    }
}
