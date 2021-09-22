using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class DataExplanations : GrabbebleObjects
{
    [SerializeField]
    private GameObject dataExplanationSet = null;

    [SerializeField]
    private Image bgImage = null;

    [SerializeField]
    private Text explantionText = null, title = null, explanationTitle = null;

    [SerializeField]
    private Dropdown poiConclusionSelectionDropdown = null;

    [SerializeField]
    private VRPlayer player = null;

    private Vector3 originalPosition = Vector3.zero, originalImageScale = Vector3.one, explanationTitlePos = Vector3.zero,
        explanationCenterPos = Vector3.zero;

    private float originalYPosOfExplanationText = 0, oldYSize = 0;

    private enum DataSetNeeded
    {
        Conclusion,
        Indication
    }

    [SerializeField]
    private DataSetNeeded dataSetNeeded = DataSetNeeded.Conclusion;

    private float changeFontSizeTimer = 0;

    public override void Start()
    {
        poiConclusionSelectionDropdown.onValueChanged.AddListener(ChangeExplanation);
        originalYPosOfExplanationText = explantionText.rectTransform.localPosition.y;

        explanationTitlePos = explanationTitle.rectTransform.position;
        originalImageScale = explantionText.rectTransform.localScale;
        originalPosition = explantionText.rectTransform.localPosition;
        EnableMenu(false);
        base.Start();
    }

    public override void Update()
    {
        if (!bgImage.enabled) { return; }

        changeFontSizeTimer -= Time.deltaTime;
        base.Update();
    }

    public override void ChangeImageScale(float scalePower, GameObject image, Vector3 vector3, float extraYMovement)
    {
        if (changeFontSizeTimer > 0) { return; }
        changeFontSizeTimer = 0.3f;
        oldYSize = explantionText.rectTransform.sizeDelta.y;
        explantionText.fontSize += (int)scalePower;

        if (explantionText.fontSize > maximumScale)
        {
            explantionText.fontSize = (int)maximumScale;
        } else if (explantionText.fontSize < minimumScale)
        {
            explantionText.fontSize = (int)minimumScale;
        }
        StartCoroutine(SetExplanationTextPos(true));
    }

    public override void MoveImage(Vector2 steerStickInput, GameObject image, Vector3 vector3, float extraYMovement, bool nullifyMovement)
    {
        base.MoveImage(steerStickInput, explantionText.gameObject, explanationCenterPos, 25, nullifyMovement);
    }

    public override void OnGrabEnter(SelectEnterEventArgs selectEnterEventArgs)
    {
        base.OnGrabEnter(selectEnterEventArgs);

        poiConclusionSelectionDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < player.locationCoordinates.Count; i++)
        {
            options.Add(player.locationCoordinates[i].ToString());
        }
        poiConclusionSelectionDropdown.AddOptions(options);

        dataExplanationSet.transform.SetAsLastSibling();
        EnableMenu(true);

        switch (dataSetNeeded)
        {
            case DataSetNeeded.Conclusion:
                title.text = "Conclusie";
                break;
            case DataSetNeeded.Indication:
                title.text = "Indicatie";
                break;
        }

        if (poiConclusionSelectionDropdown.options.Count > 0)
        {
            ChangeExplanation(0);
        }
    }

    public override void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
    {
        base.OnSelectExit(selectExitEventArgs);
        EnableMenu(false);
    }

    private void EnableMenu(bool enabled)
    {
        dataExplanationSet.SetActive(enabled);
        explantionText.gameObject.SetActive(false);
    }

    private void ChangeExplanation(int value)
    {
        explantionText.gameObject.SetActive(true);
        explantionText.rectTransform.localScale = originalImageScale;
        explantionText.rectTransform.localPosition = originalPosition;

        explanationTitle.text = poiConclusionSelectionDropdown.options[value].text;

        switch(dataSetNeeded)
        {
            case DataSetNeeded.Conclusion:
                explantionText.text = player.conclusions[value];
                break;
            case DataSetNeeded.Indication:
                explantionText.text = player.indications[value];
                break;
        }
        StartCoroutine(SetExplanationTextPos(false));
    }

    private IEnumerator SetExplanationTextPos(bool setOffset)
    {
        yield return new WaitForEndOfFrame();
        Vector3 oldPos = explantionText.rectTransform.localPosition;
        float originalYPos = oldPos.y;

        Vector3 newPos = explantionText.rectTransform.localPosition;
        newPos.y = originalYPosOfExplanationText - explantionText.rectTransform.sizeDelta.y / 2;
        explantionText.rectTransform.localPosition = newPos;
        explanationCenterPos = newPos;
        explanationTitle.rectTransform.position = explanationTitlePos;

        if (setOffset)
        {
            oldPos.y += BaseCalculations.CalculatePosDiff(oldYSize, explantionText.rectTransform.sizeDelta.y, originalYPos, 1);
            explantionText.rectTransform.localPosition = oldPos;
            MoveImage(Vector2.one, explantionText.gameObject, explanationCenterPos, 25, true);
        }
    }
}
