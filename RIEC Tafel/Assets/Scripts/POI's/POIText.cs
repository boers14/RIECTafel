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

    private string normalText = "";

    [System.NonSerialized]
    public string textExtend = "";

    private Vector3 originalPos = Vector3.zero;

    private int amountOfHits = 0;

    private float originalScale = 0, originalYvalue = 0, maxStandardDeviation = 0.19f, minStandardDeviation = 0.425f, originalYTextSize = 0, 
        originalTextBoxYScale = 0, canInteractWithControllerTimer = 0, canInteractWithControllerCooldown = 0.75f;

    private bool expanded = false, startUnExpand = false, checkTextBoxWidth = true;

    private List<bool> canInteractWithController = new List<bool>();

    [SerializeField]
    private Transform textBox = null;

    private List<InputDevice> inputDevices = new List<InputDevice>();

    private Transform player, cameraTransform = null;

    private MoveMap map = null;

    private SpriteRenderer textBoxRenderer = null;

    /// <summary>
    /// Initialize variables
    /// </summary>

    public virtual void Start()
    {
        if (originalPos != Vector3.zero) { return; }

        cameraTransform = Camera.main.transform;

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

    /// <summary>
    /// Rotate POI text to always look at player cam.
    /// Once the text is expanded color the text from red to white. Once the timer is at 0 set background to white and with a
    /// primary button press the player can pull the POI in front of him.
    /// </summary>

    private void Update()
    {
        transform.LookAt(cameraTransform);
        Vector3 newRot = transform.eulerAngles;
        newRot.x = 0;
        newRot.z = 0;
        newRot.y += 180;
        transform.eulerAngles = newRot;

        if (!expanded) 
        { 
            // Keep setting defualt not hovered values while not expanded
            for (int i = 0; i < canInteractWithController.Count; i++)
            {
                canInteractWithController[i] = false;
            }
            canInteractWithControllerTimer = canInteractWithControllerCooldown;
            return; 
        }

        for (int i = 0; i < inputDevices.Count; i++)
        {
            if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton 
                && canInteractWithController[i] && canInteractWithControllerTimer < 0)
            {
                // Pull POI to player if the button is pressed and the timer is done
                PullPOIToPlayer(i);
            } else if (!primaryButton)
            {
                // can only start pulling POI to player if the player didnt have the button pressed before hovering the POI
                // else the player first has to release the button before pressing it again (can also happen if timer is still going
                // down)
                canInteractWithController[i] = true;
            }
        }

        // Color the background of the POI text from red to white based on time left
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

    /// <summary>
    /// Virtual for tutorial. Calculates the new position in front of the player and the difference between the current pos and the
    /// new pos. Uses that as map movement.
    /// </summary>

    public virtual void PullPOIToPlayer(int index)
    {
        Vector3 newPos = player.position + (player.forward * 1.5f);
        Vector2 diffInPos = Vector2.zero;
        diffInPos.x = newPos.x - transform.position.x;
        diffInPos.y = newPos.z - transform.position.z;
        map.MoveTheMap(diffInPos, true, true);
        canInteractWithController[index] = false;
    }

    /// <summary>
    /// Initialize more variables set throught the POI manager
    /// </summary>

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

    /// <summary>
    /// Check whether width if the text is bigger then the current background. If so increase the width of the background and move
    /// it to the right side.
    /// Activate the textbox and add the extra text. Add a content size fitter to calculate a new rect transform size.
    /// </summary>

    public virtual void ExpandText(HoverEnterEventArgs args)
    {
        startUnExpand = false;
        if (expanded) { return; }

        if (checkTextBoxWidth)
        {
            checkTextBoxWidth = false;
            if (poiText.textBounds.size.x > poiText.rectTransform.sizeDelta.x)
            {
                // Calculate how much bigger the text is percentually and move/ scale the textbox based on this percentual increase
                float percentualIncrease = poiText.textBounds.size.x / poiText.rectTransform.sizeDelta.x;
                Vector3 textBoxScale = textBox.transform.localScale;
                Vector3 oldTextBoxScale = textBox.transform.localScale;
                textBoxScale.x *= percentualIncrease;
                textBoxScale.x += 0.025f;
                textBox.transform.localScale = textBoxScale;

                Vector3 newPos = textBox.transform.localPosition;
                newPos.x += ((oldTextBoxScale.x * percentualIncrease) - oldTextBoxScale.x) * 4;
                textBox.transform.localPosition = newPos;
            }
        }

        textBox.gameObject.SetActive(true);
        textBoxRenderer.material.color = Color.red;
        expanded = true;
        poiText.gameObject.AddComponent<ContentSizeFitter>();
        poiText.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        poiText.text += "\n" + textExtend;
        StartCoroutine(ChangeTextPos());
    }

    /// <summary>
    /// At the end of the frame the new y-size is known and can be used to move the textbox upward. 
    /// The textbox is also scaled upward
    /// </summary>

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

    /// <summary>
    /// Did look buggy (if the ray didnt detect this object for a fraction of a second) if it instantly performed this function 
    /// and thus it start with a little delay
    /// </summary>

    private void StartUnExpandText(HoverExitEventArgs args)
    {
        startUnExpand = true;
        StartCoroutine(UnExpandText());
    }

    /// <summary>
    /// Only perform function if player didnt hover again. Resets all variables to their base values. Removes content size fitter.
    /// </summary>

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

    /// <summary>
    /// Calculates the original value of the y position that the text requires to be above the POI.
    /// Moves the POI text up/ down based on the if new scale is bigger/ smaller. Use this scale to also scale the POI.
    /// </summary>

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

        // Calculate added Y based on if th scale is smaller or bigger then the original scale
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

        // Unparent poi text to not deform it
        poiText.transform.SetParent(null);
        // The y scale of an POI is also linked to the amount of hits and thus is differently calculates then z/ x
        newScale.y = newScale.y * 0.1f * amountOfHits;
        newScale.x *= poiScale;
        newScale.z *= poiScale;
        transform.localScale = newScale;
        poiText.transform.SetParent(transform);
        poiText.rectTransform.localPosition = originalPos;
    }

    /// <summary>
    /// Gets text component from POI 
    /// </summary>

    private void SetPoiTextComponent()
    {
        if (!poiText)
        {
            poiText = GetComponentInChildren<TMP_Text>();
            originalYTextSize = poiText.rectTransform.sizeDelta.y;
        }
    }
}
