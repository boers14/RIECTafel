using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class DataExplanations : GrabbebleObjects
{
    [SerializeField]
    private GameObject dataExplanationSet = null;

    [SerializeField]
    private TMP_Text explantionText = null, title = null, explanationTitle = null;

    [SerializeField]
    private TMP_Dropdown poiConclusionSelectionDropdown = null;

    [SerializeField]
    private POIManager poiManager = null;

    private Vector3 originalPosition = Vector3.zero, originalImageScale = Vector3.one, explanationTitlePos = Vector3.zero,
        explanationCenterPos = Vector3.zero;

    private float originalYPosOfExplanationText = 0, oldYSize = 0, changeFontSizeTimer = 0;

    public enum DataSetNeeded
    {
        Conclusion,
        Indication
    }

    public DataSetNeeded dataSetNeeded = DataSetNeeded.Conclusion;

    [SerializeField]
    private string titleText = "";

    private bool dataSetIsOn = false, poiSelectionDropdownIsHovered = false;

    [SerializeField]
    private DataExplanations otherExplanation = null;

    private int lastChosenOption = 0;

    /// <summary>
    /// Initialize variables not done with serialize field
    /// </summary>

    public override void Start()
    {
        poiConclusionSelectionDropdown.onValueChanged.AddListener(ChangeExplanation);
        originalImageScale = explantionText.rectTransform.localScale;

        SetBasePosses();

        EnableMenu(false);
        base.Start();
    }

    /// <summary>
    /// Block update if the other dataset is on or the poi selectiondropdown is opened/ hovered
    /// </summary>

    public override void Update()
    {
        if (title.text != titleText || !dataSetIsOn || poiConclusionSelectionDropdown.transform.childCount == 4 || 
            poiSelectionDropdownIsHovered) { return; }
        changeFontSizeTimer -= Time.deltaTime;
        base.Update();
    }

    /// <summary>
    /// Change the fontsize of text, start setting the text to the position the reader was
    /// </summary>

    public override void ChangeImageScale(float scalePower, GameObject image, Vector3 vector3, float extraYMovement)
    {
        if (scalePower > 0)
        {
            scalePower = 1;
        } else
        {
            scalePower = -1;
        }

        if (changeFontSizeTimer > 0 || explantionText.fontSize == maximumScale && scalePower > 0 || 
            explantionText.fontSize == minimumScale && scalePower < 0) { return; }
        changeFontSizeTimer = 0.3f;
        oldYSize = explantionText.rectTransform.sizeDelta.y;
        explantionText.fontSize += (int)scalePower;

        StartCoroutine(SetExplanationTextPos(true, scalePower));
    }

    /// <summary>
    /// Move image with the correct variables
    /// </summary>

    public override void MoveImage(Vector2 steerStickInput, GameObject image, Vector3 vector3, float extraYMovement, bool nullifyMovement)
    {
        base.MoveImage(steerStickInput, explantionText.gameObject, explanationCenterPos, 25, nullifyMovement);
    }

    /// <summary>
    /// Set correct data for data explanation when grabbed
    /// </summary>

    public override void OnGrabEnter(SelectEnterEventArgs selectEnterEventArgs, bool setOriginalVectors)
    {
        base.OnGrabEnter(selectEnterEventArgs, setOriginalVectors);
        dataSetIsOn = true;

        dataExplanationSet.transform.SetAsLastSibling();
        EnableMenu(true);
        title.text = titleText;

        // Only change explanation if poi manager got all data.
        if (poiConclusionSelectionDropdown.options.Count > 0)
        {
            ChangeExplanation(lastChosenOption);
        }
    }

    /// <summary>
    /// Turn off the object if the other dataexplantio is not grabbed else turn the other object on
    /// </summary>
    
    public override void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
    {
        base.OnSelectExit(selectExitEventArgs);
        dataSetIsOn = false;

        if (title.text == titleText && !otherExplanation.dataSetIsOn)
        {
            EnableMenu(false);
        } else if (title.text == titleText && otherExplanation.dataSetIsOn)
        {
            otherExplanation.OnGrabEnter(new SelectEnterEventArgs(), false);
        }
    }

    /// <summary>
    /// Enable active state of the data explanation set
    /// </summary>

    private void EnableMenu(bool enabled)
    {
        dataExplanationSet.SetActive(enabled);
        // Is set active in ChangeExplanation when enabled set to true
        explantionText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Set the explanation text to have the correct text together with the title
    /// </summary>
    
    private void ChangeExplanation(int value)
    {
        if (dataSetIsOn && title.text == titleText)
        {
            lastChosenOption = value;

            explantionText.gameObject.SetActive(true);
            explantionText.rectTransform.localScale = originalImageScale;
            explantionText.rectTransform.localPosition = originalPosition;

            explanationTitle.text = poiConclusionSelectionDropdown.options[value].text;

            switch (dataSetNeeded)
            {
                case DataSetNeeded.Conclusion:
                    explantionText.text = poiManager.conclusions[value];
                    break;
                case DataSetNeeded.Indication:
                    explantionText.text = poiManager.indications[value];
                    break;
            }
            StartCoroutine(SetExplanationTextPos(false));
        }
    }

    /// <summary>
    /// Set y-pos to the of the text to the start of the text
    /// </summary>

    private IEnumerator SetExplanationTextPos(bool setOffset, float fontSizeDiff = 0)
    {
        yield return new WaitForEndOfFrame();
        Vector3 oldPos = explantionText.rectTransform.localPosition;
        float originalYPos = oldPos.y;

        Vector3 newPos = explantionText.rectTransform.localPosition;
        newPos.y = originalYPosOfExplanationText - explantionText.rectTransform.sizeDelta.y / 2;
        explantionText.rectTransform.localPosition = newPos;
        explanationCenterPos = newPos;
        explanationTitle.rectTransform.position = explanationTitlePos;

        // Move text to its original position if the fontsize changed
        if (setOffset)
        {
            oldPos.y += BaseCalculations.CalculatePosDiff(oldYSize, explantionText.rectTransform.sizeDelta.y, originalYPos);
            oldPos.y += -fontSizeDiff * explantionText.fontSize;
            explantionText.rectTransform.localPosition = oldPos;
            MoveImage(Vector2.one, explantionText.gameObject, explanationCenterPos, 25, true);
        }
    }

    /// <summary>
    /// Fill the poiConclusionSelectionDropdown with all required data
    /// </summary>

    public void FillOptionsList()
    {
        poiConclusionSelectionDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < poiManager.locationNames.Count; i++)
        {
            options.Add(poiManager.locationNames[i] + ": " + poiManager.featureAmounts[i]);
        }
        poiConclusionSelectionDropdown.AddOptions(options);
    }

    /// <summary>
    /// Set base posses for calculations
    /// </summary>

    public void SetBasePosses()
    {
        originalYPosOfExplanationText = explantionText.rectTransform.localPosition.y;
        explanationTitlePos = explanationTitle.rectTransform.position;
        originalPosition = explantionText.rectTransform.localPosition;
    }

    /// <summary>
    /// Set the poi selection dropdaown to be hovered or not hovered based on if the player is aiming at it.
    /// </summary>

    public void SetPOISelectionDropDownToHovered()
    {
        poiSelectionDropdownIsHovered = true;
    }

    public void SetPOISelectionDropDownToIsNotHovered()
    {
        poiSelectionDropdownIsHovered = false;
    }
}
