using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OculusAnimations : MonoBehaviour
{
    private Vector3 basePos = Vector3.zero, exampleRayPos = Vector3.zero, exampleMapPos = Vector3.zero, exampleMapRot = Vector3.zero,
        exampleMapScale = Vector3.zero, otherExampleRayPos = Vector3.zero, otherOculusControllerbasePos = Vector3.zero,
        exampleGrabbebleObjectBasePos = Vector3.zero, exampleGrabbebleObjectOpenedBasePos = Vector3.zero,
        exampleGrabbebleObjectOpenedBaseScale = Vector3.zero;

    private string neededFunctionAfterTurnOnButtonEffect = "", neededFunctionAfterSecondGrabPhase = "", neededFunctionAfterEndOfGrabAnimation = "";

    [SerializeField]
    private GameObject primaryButtonPressEffect = null, secondaryButtonPressEffect = null, triggerPressEffect = null, steerStickUseEffect = null,
        steerStickLeftArrow = null, steerStickRightArrow = null, steerStickUpArrow = null, steerStickDownArrow = null, gripPressEffect = null,
        mapOfNetherlands = null, exampleButton = null, exampleInputfield = null, exampleRay = null, exampleKeyboard = null, examplePOI = null,
        otherOculusController = null, otherExampleRay = null, exampleGrabbebleObject = null, exampleGrabbebleObjectOpened = null;

    [SerializeField]
    private TMP_Text examplePOIText = null;

    [SerializeField]
    private TMP_Dropdown exampleDropdown = null;

    private Scrollbar exampleDrowdownScrollbar = null;

    [SerializeField]
    private Transform beforeRayTransform = null, scaleMapTransform = null, exampleRayBasePosScaleMap = null, beforeRayTransformScaleMap = null,
        otherBeforeRayTransformScaleMap = null;

    [SerializeField]
    private List<Transform> exampleKeyBoardPosses = new List<Transform>();

    private Transform exampleRayParent = null;

    private int keyBoardPosCount = 0;

    private bool showGripAndPrimary = false, showPrimaryOnly = true, showGripOnly = false;

    private string baseExampleText = "";

    private List<GameObject> allTweenObjects = new List<GameObject>();

    private Image ownImage = null;

    private void Start()
    {
        ownImage = GetComponent<Image>();

        exampleGrabbebleObjectBasePos = exampleGrabbebleObject.transform.position;
        exampleGrabbebleObjectOpenedBasePos = exampleGrabbebleObjectOpened.transform.position;
        exampleGrabbebleObjectOpenedBaseScale = exampleGrabbebleObjectOpened.transform.localScale;

        baseExampleText = examplePOIText.text;

        otherExampleRayPos = otherExampleRay.transform.position;
        otherOculusControllerbasePos = otherOculusController.transform.position;

        exampleRayParent = exampleRay.transform.parent;
        exampleRayPos = exampleRay.transform.position;

        basePos = transform.position;

        exampleMapPos = mapOfNetherlands.transform.position;
        exampleMapRot = mapOfNetherlands.transform.eulerAngles;
        exampleMapScale = mapOfNetherlands.transform.localScale;

        allTweenObjects.AddRange(new GameObject[] { primaryButtonPressEffect, gripPressEffect, mapOfNetherlands, exampleButton, exampleInputfield,
            exampleRay, exampleKeyboard, examplePOI, otherOculusController, otherExampleRay, examplePOIText.gameObject, exampleGrabbebleObject,
            exampleGrabbebleObjectOpened, secondaryButtonPressEffect, steerStickDownArrow, steerStickLeftArrow, steerStickRightArrow,
            steerStickUpArrow, steerStickUseEffect, triggerPressEffect, exampleDropdown.gameObject});

        ShowExampleButtonAnimation();
    }

    public void ShowExampleButtonAnimation()
    {
        neededFunctionAfterTurnOnButtonEffect = "ShowExampleButtonAnimation";
        TurnOffRayAndButtonEffect(true);
        exampleButton.SetActive(true);
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
    }

    public void ShowExampleInputFieldAnimation()
    {
        neededFunctionAfterTurnOnButtonEffect = "ShowExampleTypeInInputFieldAnimation";
        keyBoardPosCount = 0;
        exampleInputfield.GetComponentInChildren<TMP_Text>().text = "";
        exampleInputfield.SetActive(true);
        exampleKeyboard.SetActive(true);
        TurnOffRayAndButtonEffect(true);
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
    }

    private void ShowExampleTypeInInputFieldAnimation()
    {
        exampleRay.transform.SetParent(transform);
        TurnOffRayAndButtonEffect(false);

        iTween.MoveTo(gameObject, iTween.Hash("position", exampleKeyBoardPosses[keyBoardPosCount].position, "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));

        switch (keyBoardPosCount)
        {
            case 1:
                exampleInputfield.GetComponentInChildren<TMP_Text>().text += "h";
                break;
            case 2:
                exampleInputfield.GetComponentInChildren<TMP_Text>().text += "a";
                break;
            case 3: case 4:
                exampleInputfield.GetComponentInChildren<TMP_Text>().text += "l";
                break;
        }

        keyBoardPosCount++;
        if (keyBoardPosCount >= exampleKeyBoardPosses.Count)
        {
            neededFunctionAfterTurnOnButtonEffect = "AddLastLetterToInputField";
        }
    }

    private void AddLastLetterToInputField()
    {
        TurnOffRayAndButtonEffect(false);
        exampleInputfield.GetComponentInChildren<TMP_Text>().text += "o";
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 2f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleInputFieldAnimation", "oncompletetarget", gameObject));
    }

    public void StartExampleMoveMap()
    {
        neededFunctionAfterTurnOnButtonEffect = "ShowExampleMoveMapFirstStep";
        TurnOffRayAndButtonEffect(true);
        mapOfNetherlands.SetActive(true);
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapFirstStep()
    {
        mapOfNetherlands.transform.SetParent(transform);
        exampleRay.transform.SetParent(transform);
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position + new Vector3(-0.75f, 0.1f, 0), "time", 2f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleMoveMapSecondStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapSecondStep()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position + new Vector3(-0.5f, -0.2f, 0), "time", 2f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleMoveMapThirdStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapThirdStep()
    {
        neededFunctionAfterTurnOnButtonEffect = "StartExampleMoveMap";
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position, "time", 2f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
    }

    public void StartExampleRotateMap()
    {
        showGripAndPrimary = true;
        neededFunctionAfterTurnOnButtonEffect = "ShowExampleRotateMapFirstStep";
        TurnOffRayAndButtonEffect(true);
        mapOfNetherlands.SetActive(true);
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
    }

    private void ShowExampleRotateMapFirstStep()
    {
        exampleRay.transform.SetParent(transform);
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position + new Vector3(-0.45f, 0, 0), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleRotateMapSecondStep", "oncompletetarget", gameObject));
        iTween.RotateTo(mapOfNetherlands, iTween.Hash("rotation", new Vector3(0, 0, -75), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine));
    }

    private void ShowExampleRotateMapSecondStep()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position + new Vector3(0.35f, 0, 0), "time", 3f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleRotateMapThirdStep", "oncompletetarget", gameObject));
        iTween.RotateTo(mapOfNetherlands, iTween.Hash("rotation", new Vector3(0, 0, 75), "time", 3f,
            "easetype", iTween.EaseType.easeInOutSine));
    }

    private void ShowExampleRotateMapThirdStep()
    {
        neededFunctionAfterTurnOnButtonEffect = "StartExampleRotateMap";
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position, "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
        iTween.RotateTo(mapOfNetherlands, iTween.Hash("rotation", Vector3.zero, "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine));
    }

    public void StartExampleScaleMap()
    {
        showGripAndPrimary = false;
        neededFunctionAfterTurnOnButtonEffect = "ShowExampleScaleMapFirstStep";

        exampleRay.transform.SetParent(exampleRayParent);
        otherExampleRay.transform.SetParent(exampleRayParent);

        TurnOffRayAndButtonEffect(false);
        mapOfNetherlands.SetActive(true);
        otherOculusController.SetActive(true);
        otherOculusController.transform.GetChild(0).gameObject.SetActive(false);
        otherExampleRay.SetActive(false);

        mapOfNetherlands.transform.eulerAngles = exampleMapRot;
        mapOfNetherlands.transform.localScale = Vector3.one;
        mapOfNetherlands.transform.position = scaleMapTransform.position;
        transform.position = basePos;
        otherOculusController.transform.position = otherOculusControllerbasePos;
        exampleRay.transform.position = exampleRayBasePosScaleMap.position;
        otherExampleRay.transform.position = otherExampleRayPos;

        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransformScaleMap.position, "time", 1.5f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
        iTween.MoveTo(otherOculusController, iTween.Hash("position", otherBeforeRayTransformScaleMap.position, "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "TurnOnOtherExampleRay", "oncompletetarget", gameObject));
    }

    private void TurnOnOtherExampleRay()
    {
        otherExampleRay.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.75f, "onupdate", "EmptyUpdate",
            "oncomplete", "TurnOnOtherOculusControllerPrimaryButtonEffect", "oncompletetarget", gameObject));
    }

    private void TurnOnOtherOculusControllerPrimaryButtonEffect()
    {
        otherOculusController.transform.GetChild(0).gameObject.SetActive(true);
    }

    private void ShowExampleScaleMapFirstStep()
    {
        exampleRay.transform.SetParent(transform);
        otherExampleRay.transform.SetParent(otherOculusController.transform);

        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransformScaleMap.position + new Vector3(0.25f, 0, 0), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleScaleMapSecondStep", "oncompletetarget", gameObject));
        iTween.MoveTo(otherOculusController, iTween.Hash("position", otherBeforeRayTransformScaleMap.position + new Vector3(-0.25f, 0, 0), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine));
        iTween.ScaleTo(mapOfNetherlands, iTween.Hash("scale", new Vector3(1.5f, 1.5f, 1.5f), "time", 1.5f, "easetype", iTween.EaseType.easeInOutSine));
    }

    private void ShowExampleScaleMapSecondStep()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransformScaleMap.position + new Vector3(-0.4f, 0, 0), "time", 3f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleScaleMapThirdStep", "oncompletetarget", gameObject));
        iTween.MoveTo(otherOculusController, iTween.Hash("position", otherBeforeRayTransformScaleMap.position + new Vector3(0.4f, 0, 0), "time", 3f,
            "easetype", iTween.EaseType.easeInOutSine));
        iTween.ScaleTo(mapOfNetherlands, iTween.Hash("scale", new Vector3(0.6f, 0.6f, 0.6f), "time", 3f, "easetype", iTween.EaseType.easeInOutSine));
    }

    private void ShowExampleScaleMapThirdStep()
    {
        neededFunctionAfterTurnOnButtonEffect = "StartExampleScaleMap";
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransformScaleMap.position, "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
        iTween.MoveTo(otherOculusController, iTween.Hash("position", otherBeforeRayTransformScaleMap.position, "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine));
        iTween.ScaleTo(mapOfNetherlands, iTween.Hash("scale", Vector3.one, "time", 1.5f, "easetype", iTween.EaseType.easeInOutSine));
    }

    public void ShowExampleShowingExtraPOIInformation()
    {
        showPrimaryOnly = false;
        neededFunctionAfterTurnOnButtonEffect = "ShowExampleOpeningExtraPOIInformation";
        TurnOffRayAndButtonEffect(true);
        examplePOI.SetActive(true);
        examplePOIText.gameObject.SetActive(true);
        examplePOIText.text = baseExampleText;
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
    }

    private void ShowExampleOpeningExtraPOIInformation()
    {
        examplePOIText.text += "\nGeen hypotheek,\nHypotheeknemer buitenland,\nVeiling";
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 3f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleShowingExtraPOIInformation", "oncompletetarget", gameObject));
    }

    public void ShowExamplePullingPOITowardsPlayer()
    {
        showPrimaryOnly = true;
        neededFunctionAfterTurnOnButtonEffect = "ShowExamplePullingPOITowardsPlayer";
        TurnOffRayAndButtonEffect(true);
        examplePOI.SetActive(true);
        examplePOIText.gameObject.SetActive(true);
        examplePOIText.text = baseExampleText;
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
    }

    public void StartExampleGrabObject()
    {
        showPrimaryOnly = false;
        showGripOnly = true;
        TurnOffRayAndButtonEffect(true);
        exampleGrabbebleObject.SetActive(true);
        neededFunctionAfterTurnOnButtonEffect = "ShowExampleGrabObjectFirstStep";
        neededFunctionAfterSecondGrabPhase = "ShowExampleGrabObjectThirdStep";
        neededFunctionAfterEndOfGrabAnimation = "StartExampleGrabObject";

        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
    }

    private void ShowExampleGrabObjectFirstStep()
    {
        gripPressEffect.SetActive(false);
        exampleRay.SetActive(false);
        iTween.MoveTo(exampleGrabbebleObject, iTween.Hash("position", beforeRayTransform.position, "time", 2f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleGrabObjectSecondStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleGrabObjectSecondStep()
    {
        exampleGrabbebleObjectOpened.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 1.5f, "onupdate", "EmptyUpdate",
            "oncomplete", neededFunctionAfterSecondGrabPhase, "oncompletetarget", gameObject));
    }

    private void ShowExampleGrabObjectThirdStep()
    {
        gripPressEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleGrabObjectFourthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleGrabObjectFourthStep()
    {
        exampleGrabbebleObjectOpened.SetActive(false);
        gripPressEffect.SetActive(false);
        iTween.MoveTo(exampleGrabbebleObject, iTween.Hash("position", exampleGrabbebleObjectBasePos, "time", 2f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", neededFunctionAfterEndOfGrabAnimation, "oncompletetarget", gameObject));
    }

    public void StartExampleMoveCanvasObject()
    {
        StartExampleGrabObject();
        neededFunctionAfterSecondGrabPhase = "SetUpExampleMoveCanvasObjectFirstStep";
        neededFunctionAfterEndOfGrabAnimation = "StartExampleMoveCanvasObject";
    }

    private void SetUpExampleMoveCanvasObjectFirstStep()
    {
        gripPressEffect.SetActive(false);
        showPrimaryOnly = true;
        neededFunctionAfterTurnOnButtonEffect = "ShowExampleMoveCanvasObjectFirstStep";
        TurnOnExampleRay();
    }

    private void ShowExampleMoveCanvasObjectFirstStep()
    {
        exampleGrabbebleObjectOpened.transform.SetParent(exampleGrabbebleObject.transform);
        exampleRay.transform.SetParent(exampleGrabbebleObject.transform);
        transform.SetParent(exampleGrabbebleObject.transform);
        iTween.MoveTo(exampleGrabbebleObject, iTween.Hash("position", beforeRayTransform.position + new Vector3(0, 0.3f, 0), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleMoveCanvasObjectSecondStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveCanvasObjectSecondStep()
    {
        iTween.MoveTo(exampleGrabbebleObject, iTween.Hash("position", beforeRayTransform.position + new Vector3(0, -0.6f, 0), "time", 3f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleMoveCanvasObjectThirdStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveCanvasObjectThirdStep()
    {
        iTween.MoveTo(exampleGrabbebleObject, iTween.Hash("position", beforeRayTransform.position, "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleMoveCanvasObjectFourthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveCanvasObjectFourthStep()
    {
        primaryButtonPressEffect.SetActive(false);
        showPrimaryOnly = false;
        exampleGrabbebleObjectOpened.transform.SetParent(exampleRayParent);
        exampleRay.transform.SetParent(exampleRayParent);
        transform.SetParent(exampleRayParent);
        exampleRay.SetActive(false);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleGrabObjectThirdStep", "oncompletetarget", gameObject));
    }

    public void StartExampleScaleCanvasObject()
    {
        StartExampleGrabObject();
        neededFunctionAfterSecondGrabPhase = "SetUpExampleScaleCanvasObjectFirstStep";
        neededFunctionAfterEndOfGrabAnimation = "StartExampleScaleCanvasObject";
    }

    private void SetUpExampleScaleCanvasObjectFirstStep()
    {
        gripPressEffect.SetActive(false);
        showGripOnly = false;
        neededFunctionAfterTurnOnButtonEffect = "ShowExampleScaleCanvasObjectFirstStep";
        TurnOnExampleRay();
    }

    private void ShowExampleScaleCanvasObjectFirstStep()
    {
        exampleGrabbebleObject.transform.SetParent(exampleGrabbebleObjectOpened.transform);
        exampleRay.transform.SetParent(exampleGrabbebleObjectOpened.transform);
        transform.SetParent(exampleGrabbebleObjectOpened.transform);

        iTween.MoveTo(exampleGrabbebleObjectOpened, iTween.Hash("position", scaleMapTransform.position, "time", 2f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleScaleCanvasObjectSecondStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleCanvasObjectSecondStep()
    {
        exampleGrabbebleObject.transform.SetParent(exampleRayParent);
        exampleRay.transform.SetParent(exampleRayParent);
        transform.SetParent(exampleRayParent);

        otherOculusController.SetActive(true);
        otherOculusController.transform.GetChild(0).gameObject.SetActive(false);
        otherExampleRay.SetActive(false);

        otherOculusController.transform.position = otherOculusControllerbasePos;
        otherExampleRay.transform.position = new Vector3(otherExampleRayPos.x, exampleRay.transform.position.y, otherExampleRayPos.z);

        iTween.MoveTo(otherOculusController, iTween.Hash("position",
            new Vector3(otherBeforeRayTransformScaleMap.position.x, transform.position.y, transform.position.z), "time", 1f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleScaleCanvasObjectThirdStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleCanvasObjectThirdStep()
    {
        otherExampleRay.SetActive(true);
        otherOculusController.transform.GetChild(0).gameObject.SetActive(true);
        primaryButtonPressEffect.SetActive(true);

        exampleRay.transform.SetParent(exampleGrabbebleObject.transform);
        transform.SetParent(exampleGrabbebleObject.transform);
        otherExampleRay.transform.SetParent(otherOculusController.transform);

        iTween.MoveTo(exampleGrabbebleObject, iTween.Hash("position", exampleGrabbebleObject.transform.position + new Vector3(0.25f, 0, 0), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleScaleCanvasObjectFourthStep", "oncompletetarget", gameObject));
        iTween.MoveTo(otherOculusController, iTween.Hash("position", otherOculusController.transform.position + new Vector3(-0.15f, 0, 0), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine));
        iTween.ScaleTo(exampleGrabbebleObjectOpened, iTween.Hash("scale", new Vector3(1.5f, 1.5f, 1.5f), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine));
    }

    private void ShowExampleScaleCanvasObjectFourthStep()
    {
        iTween.MoveTo(exampleGrabbebleObject, iTween.Hash("position", exampleGrabbebleObject.transform.position + new Vector3(-0.3f, 0, 0), "time", 3f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleScaleCanvasObjectFifthStep", "oncompletetarget", gameObject));
        iTween.MoveTo(otherOculusController, iTween.Hash("position", otherOculusController.transform.position + new Vector3(0.4f, 0, 0), "time", 3f,
            "easetype", iTween.EaseType.easeInOutSine));
        iTween.ScaleTo(exampleGrabbebleObjectOpened, iTween.Hash("scale", new Vector3(0.6f, 0.6f, 0.6f), "time", 3f,
            "easetype", iTween.EaseType.easeInOutSine));
    }

    private void ShowExampleScaleCanvasObjectFifthStep()
    {
        iTween.MoveTo(exampleGrabbebleObject, iTween.Hash("position", exampleGrabbebleObject.transform.position + new Vector3(0.1f, 0, 0), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleScaleCanvasObjectSixthStep", "oncompletetarget", gameObject));
        iTween.MoveTo(otherOculusController, iTween.Hash("position", otherOculusController.transform.position + new Vector3(-0.25f, 0, 0), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine));
        iTween.ScaleTo(exampleGrabbebleObjectOpened, iTween.Hash("scale", Vector3.one, "time", 1.5f, "easetype", iTween.EaseType.easeInOutSine));
    }

    private void ShowExampleScaleCanvasObjectSixthStep()
    {
        otherExampleRay.transform.SetParent(exampleRayParent);

        otherOculusController.SetActive(false);
        otherExampleRay.SetActive(false);
        primaryButtonPressEffect.SetActive(false);

        otherExampleRay.transform.position = otherExampleRayPos;

        exampleGrabbebleObjectOpened.transform.SetParent(exampleGrabbebleObject.transform);
        exampleRay.transform.SetParent(exampleGrabbebleObject.transform);
        transform.SetParent(exampleGrabbebleObject.transform);

        iTween.MoveTo(exampleGrabbebleObject, iTween.Hash("position", beforeRayTransform.position, "time", 2f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "EndSetupForScaleAnimation", "oncompletetarget", gameObject));
    }

    private void EndSetupForScaleAnimation()
    {
        exampleRay.SetActive(false);

        exampleGrabbebleObjectOpened.transform.SetParent(exampleRayParent);
        exampleRay.transform.SetParent(exampleRayParent);
        transform.SetParent(exampleRayParent);

        ShowExampleGrabObjectThirdStep();
    }

    public void ShowExampleMoveMapWithControllerFirstStep()
    {
        TurnOffRayAndButtonEffect(true);
        mapOfNetherlands.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.75f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveMapWithControllerSecondStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapWithControllerSecondStep()
    {
        steerStickUseEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveMapWithControllerThirdStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapWithControllerThirdStep()
    {
        steerStickLeftArrow.SetActive(true);
        iTween.MoveTo(mapOfNetherlands, iTween.Hash("position", exampleMapPos + new Vector3(-0.5f, 0, 0), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleMoveMapWithControllerFourthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapWithControllerFourthStep()
    {
        steerStickLeftArrow.SetActive(false);
        steerStickUpArrow.SetActive(true);
        iTween.MoveTo(mapOfNetherlands, iTween.Hash("position", exampleMapPos + new Vector3(-0.5f, 0.5f, 0), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleMoveMapWithControllerFifthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapWithControllerFifthStep()
    {
        steerStickUpArrow.SetActive(false);
        steerStickDownArrow.SetActive(true);
        iTween.MoveTo(mapOfNetherlands, iTween.Hash("position", exampleMapPos + new Vector3(-0.5f, -0.5f, 0), "time", 3f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleMoveMapWithControllerSixthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapWithControllerSixthStep()
    {
        steerStickDownArrow.SetActive(false);
        steerStickUpArrow.SetActive(true);
        iTween.MoveTo(mapOfNetherlands, iTween.Hash("position", exampleMapPos + new Vector3(-0.5f, 0, 0), "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleMoveMapWithControllerSeventhStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapWithControllerSeventhStep()
    {
        steerStickUpArrow.SetActive(false);
        steerStickRightArrow.SetActive(true);
        iTween.MoveTo(mapOfNetherlands, iTween.Hash("position", exampleMapPos, "time", 1.5f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleMoveMapWithControllerFirstStep", "oncompletetarget", gameObject));
    }

    public void ShowExampleRotateMapWithControllersFirstStep()
    {
        TurnOffRayAndButtonEffect(true);
        mapOfNetherlands.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.75f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleRotateMapWithControllersSecondStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleRotateMapWithControllersSecondStep()
    {
        triggerPressEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleRotateMapWithControllersThirdStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleRotateMapWithControllersThirdStep()
    {
        iTween.RotateTo(mapOfNetherlands, iTween.Hash("rotation", new Vector3(0, 0, 75), "time", 1.5f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleRotateMapWithControllersFourthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleRotateMapWithControllersFourthStep()
    {
        triggerPressEffect.SetActive(false);
        gripPressEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleRotateMapWithControllersFifthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleRotateMapWithControllersFifthStep()
    {
        iTween.RotateTo(mapOfNetherlands, iTween.Hash("rotation", new Vector3(0, 0, -75), "time", 3f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleRotateMapWithControllersSixthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleRotateMapWithControllersSixthStep()
    {
        gripPressEffect.SetActive(false);
        triggerPressEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleRotateMapWithControllersSeventhStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleRotateMapWithControllersSeventhStep()
    {
        iTween.RotateTo(mapOfNetherlands, iTween.Hash("rotation", Vector3.zero, "time", 1.5f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleRotateMapWithControllersFirstStep", "oncompletetarget", gameObject));
    }

    public void ShowExampleScaleMapWithControllersFirstStep()
    {
        TurnOffRayAndButtonEffect(true);
        mapOfNetherlands.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.75f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleScaleMapWithControllersSecondStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleMapWithControllersSecondStep()
    {
        primaryButtonPressEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleScaleMapWithControllersThirdStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleMapWithControllersThirdStep()
    {
        iTween.ScaleTo(mapOfNetherlands, iTween.Hash("scale", new Vector3(1.5f, 1.5f, 1.5f), "time", 1.5f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleScaleMapWithControllersFourthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleMapWithControllersFourthStep()
    {
        primaryButtonPressEffect.SetActive(false);
        secondaryButtonPressEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleScaleMapWithControllersFifthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleMapWithControllersFifthStep()
    {
        iTween.ScaleTo(mapOfNetherlands, iTween.Hash("scale", new Vector3(0.6f, 0.6f, 0.6f), "time", 3f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleScaleMapWithControllersSixthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleMapWithControllersSixthStep()
    {
        secondaryButtonPressEffect.SetActive(false);
        primaryButtonPressEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleScaleMapWithControllersSeventhStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleMapWithControllersSeventhStep()
    {
        iTween.ScaleTo(mapOfNetherlands, iTween.Hash("scale", Vector3.one, "time", 1.5f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleScaleMapWithControllersFirstStep", "oncompletetarget", gameObject));
    }

    public void ShowExampleMoveUIWithControllersFirstStep()
    {
        StartExampleGrabObject();
        neededFunctionAfterSecondGrabPhase = "ShowExampleMoveUIWithControllersSecondStep";
        neededFunctionAfterEndOfGrabAnimation = "ShowExampleMoveUIWithControllersFirstStep";
    }

    private void ShowExampleMoveUIWithControllersSecondStep()
    {
        gripPressEffect.SetActive(false);
        steerStickUseEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveUIWithControllersThirdStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveUIWithControllersThirdStep()
    {
        steerStickUpArrow.SetActive(true);
        iTween.MoveTo(exampleGrabbebleObjectOpened, iTween.Hash("position", exampleGrabbebleObjectOpenedBasePos + new Vector3(0, 0.5f, 0), 
            "time", 1.5f, "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleMoveUIWithControllersFourthStep", 
            "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveUIWithControllersFourthStep()
    {
        steerStickUpArrow.SetActive(false);
        steerStickDownArrow.SetActive(true);
        iTween.MoveTo(exampleGrabbebleObjectOpened, iTween.Hash("position", exampleGrabbebleObjectOpenedBasePos + new Vector3(0, -0.5f, 0), 
            "time", 3f, "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleMoveUIWithControllersFifthStep", 
            "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveUIWithControllersFifthStep()
    {
        steerStickUpArrow.SetActive(true);
        steerStickDownArrow.SetActive(false);
        iTween.MoveTo(exampleGrabbebleObjectOpened, iTween.Hash("position", exampleGrabbebleObjectOpenedBasePos, "time", 1.5f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "EndSetupForExampleMoveUIWithControllers", "oncompletetarget", gameObject));
    }

    private void EndSetupForExampleMoveUIWithControllers()
    {
        steerStickUpArrow.SetActive(false);
        steerStickUseEffect.SetActive(false);
        ShowExampleGrabObjectThirdStep();
    }

    public void ShowExampleScaleUIWithControllersFirstStep()
    {
        StartExampleGrabObject();
        neededFunctionAfterSecondGrabPhase = "ShowExampleScaleUIWithControllersSecondStep";
        neededFunctionAfterEndOfGrabAnimation = "ShowExampleScaleUIWithControllersFirstStep";
    }

    private void ShowExampleScaleUIWithControllersSecondStep()
    {
        gripPressEffect.SetActive(false);
        primaryButtonPressEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleScaleUIWithControllersThirdStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleUIWithControllersThirdStep()
    {
        iTween.ScaleTo(exampleGrabbebleObjectOpened, iTween.Hash("scale", new Vector3(1.5f, 1.5f, 1.5f), "time", 1.5f, 
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleScaleUIWithControllersFourthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleUIWithControllersFourthStep()
    {
        primaryButtonPressEffect.SetActive(false);
        secondaryButtonPressEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleScaleUIWithControllersFifthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleUIWithControllersFifthStep()
    {
        iTween.ScaleTo(exampleGrabbebleObjectOpened, iTween.Hash("scale", new Vector3(0.6f, 0.6f, 0.6f), "time", 3f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleScaleUIWithControllersSixthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleUIWithControllersSixthStep()
    {
        secondaryButtonPressEffect.SetActive(false);
        primaryButtonPressEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleScaleUIWithControllersSeventhStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleUIWithControllersSeventhStep()
    {
        iTween.ScaleTo(exampleGrabbebleObjectOpened, iTween.Hash("scale", Vector3.one, "time", 1.5f, "easetype", iTween.EaseType.easeInOutSine, 
            "oncomplete", "EndSetupForExampleScaleUIWithControllers", "oncompletetarget", gameObject));
    }

    private void EndSetupForExampleScaleUIWithControllers()
    {
        primaryButtonPressEffect.SetActive(false);
        steerStickUseEffect.SetActive(false);
        ShowExampleGrabObjectThirdStep();
    }

    public void ShowExampleMoveOpenedDropdownWithControllersFirstStep()
    {
        exampleDropdown.gameObject.SetActive(true);
        exampleDropdown.Hide();

        showPrimaryOnly = true;
        TurnOffRayAndButtonEffect(true);
        neededFunctionAfterTurnOnButtonEffect = "ShowExampleMoveOpenedDropdownWithControllersSecondStep";
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveOpenedDropdownWithControllersSecondStep()
    {
        exampleDropdown.Show();
        primaryButtonPressEffect.SetActive(false);
        exampleRay.transform.SetParent(transform);
        StartCoroutine(RemoveSortingLayerPriorityFromExampleDropdown());
        iTween.MoveTo(gameObject, iTween.Hash("position", beforeRayTransform.position + new Vector3(0, -0.5f, 0), "time", 1.5f, 
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleMoveOpenedDropdownWithControllersThirdStep", 
            "oncompletetarget", gameObject));
    }

    private IEnumerator RemoveSortingLayerPriorityFromExampleDropdown()
    {
        yield return new WaitForEndOfFrame();
        exampleDropdown.GetComponentInChildren<Canvas>().overrideSorting = false;
    }

    private void ShowExampleMoveOpenedDropdownWithControllersThirdStep()
    {
        steerStickUseEffect.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveOpenedDropdownWithControllersFourthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveOpenedDropdownWithControllersFourthStep()
    {
        steerStickDownArrow.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 1f, "to", 0f, "time", 4f, "onupdate", "ShowExampleMoveDropdownBarUpdate",
            "oncomplete", "ShowExampleMoveOpenedDropdownWithControllersFifthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveOpenedDropdownWithControllersFifthStep()
    {
        steerStickDownArrow.SetActive(false);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveOpenedDropdownWithControllersSixthStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveOpenedDropdownWithControllersSixthStep()
    {
        steerStickUpArrow.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 4f, "onupdate", "ShowExampleMoveDropdownBarUpdate",
            "oncomplete", "ShowExampleMoveOpenedDropdownWithControllersSeventhStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveOpenedDropdownWithControllersSeventhStep()
    {
        steerStickUpArrow.SetActive(false);
        steerStickUseEffect.SetActive(false);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveOpenedDropdownWithControllersFirstStep", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveDropdownBarUpdate(float value)
    {
        if (exampleDrowdownScrollbar == null)
        {
            exampleDrowdownScrollbar = exampleDropdown.GetComponentInChildren<Scrollbar>();
        }

        exampleDrowdownScrollbar.value = value;
    }

    private void EmptyUpdate()
    {

    }

    private void TurnOnExampleRay()
    {
        exampleRay.SetActive(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.75f, "onupdate", "EmptyUpdate",
            "oncomplete", "TurnOnButtonEffect", "oncompletetarget", gameObject));
    }

    private void TurnOnButtonEffect()
    {
        if (showGripAndPrimary)
        {
            primaryButtonPressEffect.SetActive(true);
            gripPressEffect.SetActive(true);
        } else if (showPrimaryOnly) 
        {
            primaryButtonPressEffect.SetActive(true);
        } else if (showGripOnly)
        {
            gripPressEffect.SetActive(true);
        }

        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate",
            "oncomplete", neededFunctionAfterTurnOnButtonEffect, "oncompletetarget", gameObject));
    }

    private void TurnOffRayAndButtonEffect(bool resetPos)
    {
        if (resetPos)
        {
            mapOfNetherlands.transform.SetParent(exampleRayParent);
            exampleGrabbebleObjectOpened.transform.SetParent(exampleRayParent);
            exampleGrabbebleObject.transform.SetParent(exampleRayParent);
            exampleRay.transform.SetParent(exampleRayParent);
            transform.SetParent(exampleRayParent);

            ownImage.enabled = true;

            exampleGrabbebleObject.transform.position = exampleGrabbebleObjectBasePos;
            exampleGrabbebleObjectOpened.transform.position = exampleGrabbebleObjectOpenedBasePos;
            exampleGrabbebleObjectOpened.transform.localScale = exampleGrabbebleObjectOpenedBaseScale;

            mapOfNetherlands.transform.position = exampleMapPos;
            mapOfNetherlands.transform.eulerAngles = exampleMapRot;
            mapOfNetherlands.transform.localScale = exampleMapScale;

            exampleRay.transform.position = exampleRayPos;
            transform.position = basePos;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        exampleRay.SetActive(false);

        transform.SetAsLastSibling();
        otherOculusController.transform.SetAsLastSibling();
    }

    public void TurnOffAllTweenObjects()
    {
        iTween.Stop(gameObject);
        iTween.Stop(mapOfNetherlands);
        iTween.Stop(exampleGrabbebleObject);
        iTween.Stop(exampleGrabbebleObjectOpened);

        for (int i = 0; i < allTweenObjects.Count; i++)
        {
            allTweenObjects[i].SetActive(false);
        }
    }
}
