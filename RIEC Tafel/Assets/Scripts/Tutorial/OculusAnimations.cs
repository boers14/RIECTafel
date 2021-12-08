using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OculusAnimations : MonoBehaviour
{
    private Vector3 basePos = Vector3.zero, baseRot = Vector3.zero, baseRayPos = Vector3.zero, exampleLegendaBasePos = Vector3.zero,
        exampleLegendaBaseRot = Vector3.zero, standardTweenStartPos = Vector3.zero;

    private List<GameObject> allTweenObjects = new List<GameObject>();

    [SerializeField]
    private GameObject exampleButton = null, exampleInputField = null, table = null, exampleLegenda = null, exampleDropdown = null;

    [SerializeField]
    private OculusAnimations otherController = null;

    [SerializeField]
    private Transform keyBoard = null, canvas = null;

    [SerializeField]
    private List<string> requiredKeysFromKeyBoard = new List<string>();

    private List<Transform> keyBoardKeysToBeSelectedInExample = new List<Transform>();

    private LineRenderer exampleRay = null;

    private string animationAfterButtonPressAnimation = "", startAnimationAfterEndDelay = "", animationAfterObjectGrab = "";

    [SerializeField]
    private TutorialControllerArrows primaryButtonArrow = null, secondaryButtonArrow = null, steerStickArrow = null, triggerArrow = null,
        gripArrow = null, steerStickArrowUp = null, steerStickArrowDown = null, steerStickArrowLeft = null, steerStickArrowRight = null;

    private GameObject rayEndPosGameObject = null, examplePOI = null;

    private int keyBoardKeysCounter = 0;

    [SerializeField]
    private bool isControlledByOtherController = false;

    private bool showPrimaryOnly = true, showGripOnly = false, showPrimaryAndGrip = false;

    private float amountOfMovement = 0.4f;

    private void Start()
    {
        basePos = transform.position;
        baseRot = transform.eulerAngles;
        standardTweenStartPos = basePos + new Vector3(0, 0.2f, 0);

        exampleLegendaBasePos = exampleLegenda.transform.position;
        exampleLegendaBaseRot = exampleLegenda.transform.eulerAngles;

        exampleRay = GetComponent<LineRenderer>();
        exampleRay.enabled = false;

        allTweenObjects.AddRange(new GameObject[] { primaryButtonArrow.gameObject, secondaryButtonArrow.gameObject, steerStickArrow.gameObject, 
            triggerArrow.gameObject, gripArrow.gameObject, otherController.gameObject, exampleLegenda, steerStickArrowDown.gameObject,
            steerStickArrowLeft.gameObject, steerStickArrowRight.gameObject, steerStickArrowUp.gameObject });

        foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>())
        {
            allTweenObjects.Add(text.gameObject);
        }

        if (isControlledByOtherController)
        {
            gameObject.SetActive(false);
        }
    }

    public void ShowExampleButtonAnimationFirstStep()
    {
        animationAfterButtonPressAnimation = "EndDelayAnimation";
        startAnimationAfterEndDelay = "ShowExampleButtonAnimationFirstStep";
        TurnOffRayAndButtonEffect(true);
        exampleButton.SetActive(true);
        rayEndPosGameObject = exampleButton;
        BaseStartAnimation();
    }

    public void ShowExampleInputfieldAnimationFirstStep()
    {
        TurnOffRayAndButtonEffect(true);
        exampleInputField.SetActive(true);

        if (keyBoardKeysToBeSelectedInExample.Count == 0)
        {
            StartCoroutine(SetNeccesaryKeyBoardKeys());
        }

        keyBoardKeysCounter = 0;
        animationAfterButtonPressAnimation = "ShowExampleInputfieldAnimationSecondStep";
        startAnimationAfterEndDelay = "ShowExampleInputfieldAnimationFirstStep";
        rayEndPosGameObject = exampleInputField;
        BaseStartAnimation();
    }

    private void ShowExampleInputfieldAnimationSecondStep()
    {
        TurnOffRayAndButtonEffect(false);
        iTween.MoveTo(gameObject, iTween.Hash("position", keyBoard.position + keyBoard.up * 0.35f + keyBoard.forward * 0.2f, "time", 2f, 
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "ShowExampleInputfieldAnimationThirdStep", "oncompletetarget", gameObject));
    }

    private IEnumerator SetNeccesaryKeyBoardKeys()
    {
        yield return new WaitForSeconds(0.02f);
        KeyBoardKey[] keyBoardKeysArray = FindObjectsOfType<KeyBoardKey>();
        List<KeyBoardKey> keyBoardKeys = new List<KeyBoardKey>();
        keyBoardKeys.AddRange(keyBoardKeysArray);
        for (int i = 0; i < requiredKeysFromKeyBoard.Count; i++)
        {
            keyBoardKeysToBeSelectedInExample.Add(keyBoardKeys.Find
                (key => key.GetComponentInChildren<TMP_Text>().text == requiredKeysFromKeyBoard[i]).transform);
        }
    }

    private void ShowExampleInputfieldAnimationThirdStep()
    {
        TurnOffRayAndButtonEffect(false);
        iTween.LookTo(gameObject, iTween.Hash("looktarget", keyBoardKeysToBeSelectedInExample[keyBoardKeysCounter], "time", 1f,
            "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
        rayEndPosGameObject = keyBoardKeysToBeSelectedInExample[keyBoardKeysCounter].gameObject;
        keyBoardKeysCounter++;
        if (keyBoardKeysCounter < keyBoardKeysToBeSelectedInExample.Count)
        {
            animationAfterButtonPressAnimation = "ShowExampleInputfieldAnimationThirdStep";
        } else
        {
            animationAfterButtonPressAnimation = "EndDelayAnimation";
        }
    }

    public void ShowExampleMoveMapFirstStep()
    {
        animationAfterButtonPressAnimation = "ShowExampleMoveMapSecondStep";
        startAnimationAfterEndDelay = "ShowExampleMoveMapFirstStep";
        TurnOffRayAndButtonEffect(true);
        rayEndPosGameObject = table;
        BaseStartAnimation();
    }

    private void ShowExampleMoveMapSecondStep()
    {
        Vector3 newPos = transform.position - transform.right * amountOfMovement;
        if (isControlledByOtherController)
        {
            newPos = transform.position + transform.right * amountOfMovement;
        }

        iTween.MoveTo(gameObject, iTween.Hash("position", newPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateRayPos", "oncomplete", "ShowExampleMoveMapThirdStep", "oncompletetarget", gameObject, "delay", 0.5f));
    }

    private void ShowExampleMoveMapThirdStep()
    {
        Vector3 newPos = transform.position + transform.right * 2 * amountOfMovement;
        if (isControlledByOtherController)
        {
            newPos = transform.position - transform.right * 2 * amountOfMovement;
        }

        iTween.MoveTo(gameObject, iTween.Hash("position", newPos, "time", 4f, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateRayPos", "oncomplete", "ShowExampleMoveMapFourthStep", "oncompletetarget", gameObject, "delay", 0.5f));
    }

    private void ShowExampleMoveMapFourthStep()
    {
        Vector3 newPos = transform.position - transform.right * amountOfMovement;
        if (isControlledByOtherController)
        {
            newPos = transform.position + transform.right * amountOfMovement;
        }

        iTween.MoveTo(gameObject, iTween.Hash("position", newPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateRayPos", "oncomplete", "EndDelayAnimation", "oncompletetarget", gameObject, "delay", 0.5f));
    }

    public void ShowExampleRotateMapFirstStep()
    {
        showPrimaryAndGrip = true;
        animationAfterButtonPressAnimation = "ShowExampleMoveMapSecondStep";
        startAnimationAfterEndDelay = "ShowExampleRotateMapFirstStep";
        TurnOffRayAndButtonEffect(true);
        rayEndPosGameObject = table;
        BaseStartAnimation();
    }

    public void ShowExampleScaleMapFirstStep()
    {
        if (!otherController.gameObject.activeSelf)
        {
            otherController.gameObject.SetActive(true);
            if (otherController.exampleRay == null)
            {
                otherController.Start();
            }
            otherController.ShowExampleScaleMapFirstStep();
        }

        amountOfMovement = 0.15f;
        rayEndPosGameObject = table;
        showPrimaryAndGrip = false;
        animationAfterButtonPressAnimation = "ShowExampleMoveMapSecondStep";
        startAnimationAfterEndDelay = "ShowExampleScaleMapFirstStep";
        TurnOffRayAndButtonEffect(true);
        BaseStartAnimation();
    }

    public void ShowExampleOpenPOIInformation()
    {
        if (examplePOI == null)
        {
            examplePOI = FindObjectOfType<POIText>().gameObject;
        }

        showPrimaryOnly = false;
        animationAfterButtonPressAnimation = "ShowExampleOpenPOIInformation";
        startAnimationAfterEndDelay = "ShowExampleOpenPOIInformation";
        TurnOffRayAndButtonEffect(true);
        rayEndPosGameObject = examplePOI;
        BaseStartAnimation();
    }

    public void ShowExamplePullPOI()
    {
        showPrimaryOnly = true;
        animationAfterButtonPressAnimation = "EndDelayAnimation";
        startAnimationAfterEndDelay = "ShowExamplePullPOI";
        TurnOffRayAndButtonEffect(true);
        rayEndPosGameObject = examplePOI;
        BaseStartAnimation();
    }

    public void ShowExampleGrabObjectFirstStep()
    {
        showPrimaryOnly = false;
        showGripOnly = true;
        TurnOffRayAndButtonEffect(true);
        animationAfterButtonPressAnimation = "ShowExampleGrabObjectThirdStep";
        animationAfterObjectGrab = "EndDelayAnimation";
        exampleLegenda.SetActive(true);
        rayEndPosGameObject = exampleLegenda;
        iTween.MoveTo(gameObject, iTween.Hash("position", standardTweenStartPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleGrabObjectSecondStep", "oncompletetarget", gameObject, "delay", 1f));
    }

    private void ShowExampleGrabObjectSecondStep()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", exampleLegenda.transform.position + exampleLegenda.transform.up * 0.5f + 
            exampleLegenda.transform.right * -0.35f, "time", 2f, "easetype", iTween.EaseType.easeInOutSine, "oncomplete", "RotateToLookAtObject", 
            "oncompletetarget", gameObject));
    }

    private void ShowExampleGrabObjectThirdStep()
    {
        animationAfterButtonPressAnimation = "ShowExampleGrabObjectFourthStep";
        startAnimationAfterEndDelay = "ShowPrimaryButtonPressAnimation";
        TurnOffRayAndButtonEffect(false);
        iTween.MoveTo(exampleLegenda, iTween.Hash("position", transform.position, "time", 0.35f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", animationAfterObjectGrab, "oncompletetarget", gameObject));
    }

    private void ShowExampleGrabObjectFourthStep()
    {
        TurnOffRayAndButtonEffect(false);
        startAnimationAfterEndDelay = "ShowExampleGrabObjectFirstStep";
        iTween.MoveTo(exampleLegenda, iTween.Hash("position", exampleLegendaBasePos, "time", 0.35f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "EndDelayAnimation", "oncompletetarget", gameObject));
    }

    public void ShowExampleMoveGrabbedObjectFirstStep()
    {
        showPrimaryOnly = false;
        showGripOnly = true;
        TurnOffRayAndButtonEffect(true);
        animationAfterButtonPressAnimation = "ShowExampleGrabObjectThirdStep";
        animationAfterObjectGrab = "ShowExampleMoveGrabbedObjectSecondStep";
        exampleLegenda.SetActive(true);
        rayEndPosGameObject = exampleLegenda;
        iTween.MoveTo(gameObject, iTween.Hash("position", standardTweenStartPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleGrabObjectSecondStep", "oncompletetarget", gameObject, "delay", 1f));
    }

    private void ShowExampleMoveGrabbedObjectSecondStep()
    {
        exampleLegenda.transform.SetParent(transform);
        rayEndPosGameObject = canvas.gameObject;
        animationAfterButtonPressAnimation = "ShowExampleMoveGrabbedObjectThirdStep";
        showPrimaryOnly = true;
        showGripOnly = false;
        iTween.RotateTo(gameObject, iTween.Hash("rotation", baseRot, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
             "oncomplete", "BaseStartAnimation", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveGrabbedObjectThirdStep()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", basePos + new Vector3(0, 0.4f, 0), "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateRayPos", "oncomplete", "ShowExampleMoveGrabbedObjectFourthStep", "oncompletetarget", gameObject, "delay", 0.5f));
    }

    private void ShowExampleMoveGrabbedObjectFourthStep()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", basePos, "time", 3f, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateRayPos", "oncomplete", "ShowExampleMoveGrabbedObjectFifthStep", "oncompletetarget", gameObject, "delay", 0.5f));
    }

    private void ShowExampleMoveGrabbedObjectFifthStep()
    {
        startAnimationAfterEndDelay = "ShowExampleMoveGrabbedObjectFirstStep";
        iTween.MoveTo(gameObject, iTween.Hash("position", standardTweenStartPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateRayPos", "oncomplete", "PrepareFinalStepMoveGrabbedObject", "oncompletetarget", gameObject, "delay", 0.5f));
    }

    private void PrepareFinalStepMoveGrabbedObject()
    {
        exampleLegenda.transform.SetParent(null);
        animationAfterButtonPressAnimation = "ShowExampleMoveGrabbedObjectSixthStep";
        TurnOffRayAndButtonEffect(false);
        showPrimaryOnly = false;
        showGripOnly = true;
        ShowPrimaryButtonPressAnimation();
    }

    private void ShowExampleMoveGrabbedObjectSixthStep()
    {
        TurnOffRayAndButtonEffect(false);
        iTween.MoveTo(exampleLegenda, iTween.Hash("position", exampleLegendaBasePos, "time", 0.35f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "EndDelayAnimation", "oncompletetarget", gameObject));
        iTween.RotateTo(exampleLegenda, iTween.Hash("rotation", exampleLegendaBaseRot, "time", 0.35f, "easetype", iTween.EaseType.easeInOutSine));
    }

    public void ShowExampleScaleGrabbedObjectFirstStep()
    {
        showPrimaryOnly = false;
        showGripOnly = true;
        TurnOffRayAndButtonEffect(true);
        animationAfterButtonPressAnimation = "ShowExampleGrabObjectThirdStep";
        animationAfterObjectGrab = "ShowExampleScaleGrabbedObjectSecondStep";
        exampleLegenda.SetActive(true);
        rayEndPosGameObject = exampleLegenda;
        iTween.MoveTo(gameObject, iTween.Hash("position", standardTweenStartPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleGrabObjectSecondStep", "oncompletetarget", gameObject, "delay", 1f));
    }

    private void ShowExampleScaleGrabbedObjectSecondStep()
    {
        rayEndPosGameObject = canvas.gameObject;
        animationAfterButtonPressAnimation = "ShowExampleScaleGrabbedObjectThirdStep";
        showPrimaryOnly = true;
        showGripOnly = false;
        amountOfMovement = 0.15f;
        iTween.RotateTo(gameObject, iTween.Hash("rotation", baseRot, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "BaseStartAnimation", "oncompletetarget", gameObject));

        if (!isControlledByOtherController)
        {
            exampleLegenda.transform.SetParent(transform);
            otherController.gameObject.SetActive(true);
            otherController.ShowExampleScaleGrabbedObjectSecondStep();
        }
    }

    private void ShowExampleScaleGrabbedObjectThirdStep()
    {
        Vector3 newPos = transform.position - transform.right * amountOfMovement;
        if (isControlledByOtherController)
        {
            newPos = transform.position + transform.right * amountOfMovement;
        }

        iTween.MoveTo(gameObject, iTween.Hash("position", newPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateRayPos", "oncomplete", "ShowExampleScaleGrabbedObjectFourthStep", "oncompletetarget", gameObject, "delay", 0.5f));
    }

    private void ShowExampleScaleGrabbedObjectFourthStep()
    {
        Vector3 newPos = transform.position + transform.right * 2 * amountOfMovement;
        if (isControlledByOtherController)
        {
            newPos = transform.position - transform.right * 2 * amountOfMovement;
        }

        iTween.MoveTo(gameObject, iTween.Hash("position", newPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateRayPos", "oncomplete", "ShowExampleScaleGrabbedObjectFifthStep", "oncompletetarget", gameObject, "delay", 0.5f));
    }

    private void ShowExampleScaleGrabbedObjectFifthStep()
    {
        Vector3 newPos = transform.position - transform.right * amountOfMovement;
        if (isControlledByOtherController)
        {
            newPos = transform.position + transform.right * amountOfMovement;
        }

        iTween.MoveTo(gameObject, iTween.Hash("position", newPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateRayPos", "oncomplete", "PrepareFinalStepScaleGrabbedObject", "oncompletetarget", gameObject, "delay", 0.5f));
    }

    private void PrepareFinalStepScaleGrabbedObject()
    {
        if (isControlledByOtherController)
        {
            TurnOffRayAndButtonEffect(false);
            transform.position = basePos;
            transform.eulerAngles = baseRot;
            gameObject.SetActive(false);
        }
        else
        {
            exampleLegenda.transform.SetParent(null);
            startAnimationAfterEndDelay = "ShowExampleScaleGrabbedObjectFirstStep";
            animationAfterButtonPressAnimation = "ShowExampleMoveGrabbedObjectSixthStep";
            TurnOffRayAndButtonEffect(false);
            showPrimaryOnly = false;
            showGripOnly = true;
            ShowPrimaryButtonPressAnimation();
        }
    }

    public void ShowExampleMoveMapFirstStepWithControllers()
    {
        startAnimationAfterEndDelay = "ShowExampleMoveMapFirstStepWithControllers";
        if (!isControlledByOtherController)
        {
            TurnOffRayAndButtonEffect(true);
        }
        iTween.MoveTo(gameObject, iTween.Hash("position", standardTweenStartPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleMoveMapSecondStepWithControllers", "oncompletetarget", gameObject, "delay", 1f));
    }

    private void ShowExampleMoveMapSecondStepWithControllers()
    {
        steerStickArrow.gameObject.SetActive(true);
        steerStickArrow.DisplayTween();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", steerStickArrow.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveMapThirdStepWithControllers", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapThirdStepWithControllers()
    {
        steerStickArrowUp.gameObject.SetActive(true);
        steerStickArrowUp.DisplayTween();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", steerStickArrow.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveMapFourthStepWithControllers", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapFourthStepWithControllers()
    {
        steerStickArrowUp.gameObject.SetActive(false);
        steerStickArrowLeft.gameObject.SetActive(true);
        steerStickArrowLeft.DisplayTween();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", steerStickArrow.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveMapFifthStepWithControllers", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapFifthStepWithControllers()
    {
        steerStickArrowLeft.gameObject.SetActive(false);
        steerStickArrowDown.gameObject.SetActive(true);
        steerStickArrowDown.DisplayTween();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", steerStickArrow.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveMapSixthStepWithControllers", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapSixthStepWithControllers()
    {
        steerStickArrowDown.gameObject.SetActive(false);
        steerStickArrowRight.gameObject.SetActive(true);
        steerStickArrowRight.DisplayTween();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", steerStickArrow.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveMapSeventhStepWithControllers", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapSeventhStepWithControllers()
    {
        if (isControlledByOtherController)
        {
            otherController.startAnimationAfterEndDelay = "ShowExampleMoveGrabbedObjectFirstStepWithControllers";
            otherController.PrepareFinalStepMoveGrabbedObject();
            gameObject.SetActive(false);
        }
        else
        {
            steerStickArrowRight.gameObject.SetActive(false);
            EndDelayAnimation();
        }
    }

    public void ShowExampleRotateMapFirstStepWithControllers()
    {
        TurnOffRayAndButtonEffect(true);
        iTween.MoveTo(gameObject, iTween.Hash("position", standardTweenStartPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleRotateMapSecondStepWithControllers", "oncompletetarget", gameObject, "delay", 1f));
    }
    private void ShowExampleRotateMapSecondStepWithControllers()
    {
        startAnimationAfterEndDelay = "ShowExampleRotateMapThirdStepWithControllers";
        triggerArrow.gameObject.SetActive(true);
        triggerArrow.DisplayTween();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", triggerArrow.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", "EndDelayAnimation", "oncompletetarget", gameObject));
    }

    private void ShowExampleRotateMapThirdStepWithControllers()
    {
        startAnimationAfterEndDelay = "ShowExampleMoveMapRotateFourthStepWithControllers";
        TurnOffRayAndButtonEffect(false);
        gripArrow.gameObject.SetActive(true);
        gripArrow.DisplayTween();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", gripArrow.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", "EndDelayAnimation", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveMapRotateFourthStepWithControllers()
    {
        TurnOffRayAndButtonEffect(false);
        startAnimationAfterEndDelay = "ShowExampleRotateMapFirstStepWithControllers";
        EndDelayAnimation();
    }

    public void ShowExampleScaleMapFirstStepWithControllers()
    {
        if (!isControlledByOtherController)
        {
            TurnOffRayAndButtonEffect(true);
        }
        iTween.MoveTo(gameObject, iTween.Hash("position", standardTweenStartPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleScaleMapSecondStepWithControllers", "oncompletetarget", gameObject, "delay", 1f));
    }
    private void ShowExampleScaleMapSecondStepWithControllers()
    {
        startAnimationAfterEndDelay = "ShowExampleScaleMapThirdStepWithControllers";
        primaryButtonArrow.gameObject.SetActive(true);
        primaryButtonArrow.DisplayTween();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", primaryButtonArrow.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", "EndDelayAnimation", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleMapThirdStepWithControllers()
    {
        startAnimationAfterEndDelay = "ShowExampleScaleMapFourthStepWithControllers";
        TurnOffRayAndButtonEffect(false);
        secondaryButtonArrow.gameObject.SetActive(true);
        secondaryButtonArrow.DisplayTween();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", secondaryButtonArrow.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", "EndDelayAnimation", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleMapFourthStepWithControllers()
    {
        if (isControlledByOtherController)
        {
            otherController.startAnimationAfterEndDelay = "ShowExampleScaleGrabbedObjectFirstStepWithControllers";
            otherController.PrepareFinalStepMoveGrabbedObject();
            gameObject.SetActive(false);
        }
        else
        {
            TurnOffRayAndButtonEffect(false);
            startAnimationAfterEndDelay = "ShowExampleScaleMapFirstStepWithControllers";
            EndDelayAnimation();
        }
    }

    public void ShowExampleMoveGrabbedObjectFirstStepWithControllers()
    {
        showPrimaryOnly = false;
        showGripOnly = true;
        TurnOffRayAndButtonEffect(true);
        otherController.TurnOffRayAndButtonEffect(true);
        animationAfterButtonPressAnimation = "ShowExampleGrabObjectThirdStep";
        animationAfterObjectGrab = "ShowExampleMoveGrabbedObjectSecondStepWithControllers";
        exampleLegenda.SetActive(true);
        rayEndPosGameObject = exampleLegenda;
        iTween.MoveTo(gameObject, iTween.Hash("position", standardTweenStartPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleGrabObjectSecondStep", "oncompletetarget", gameObject, "delay", 1f));
    }

    private void ShowExampleMoveGrabbedObjectSecondStepWithControllers()
    {
        exampleLegenda.transform.SetParent(transform);
        iTween.RotateTo(gameObject, iTween.Hash("rotation", baseRot, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleMoveGrabbedObjectThirdStepWithControllers", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveGrabbedObjectThirdStepWithControllers()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", basePos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleMoveGrabbedObjectFourthStepWithControllers", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveGrabbedObjectFourthStepWithControllers()
    {
        exampleLegenda.transform.SetParent(null);

        otherController.gameObject.SetActive(true);
        if (otherController.exampleRay == null)
        {
            otherController.Start();
        }
        otherController.ShowExampleMoveMapFirstStepWithControllers();
    }

    public void ShowExampleScaleGrabbedObjectFirstStepWithControllers()
    {
        TurnOffRayAndButtonEffect(true);
        otherController.TurnOffRayAndButtonEffect(true);
        animationAfterButtonPressAnimation = "ShowExampleGrabObjectThirdStep";
        animationAfterObjectGrab = "ShowExampleScaleGrabbedObjectSecondStepWithControllers";
        exampleLegenda.SetActive(true);
        rayEndPosGameObject = exampleLegenda;
        iTween.MoveTo(gameObject, iTween.Hash("position", standardTweenStartPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleGrabObjectSecondStep", "oncompletetarget", gameObject, "delay", 1f));
    }

    private void ShowExampleScaleGrabbedObjectSecondStepWithControllers()
    {
        exampleLegenda.transform.SetParent(transform);
        iTween.RotateTo(gameObject, iTween.Hash("rotation", baseRot, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleScaleGrabbedObjectThirdStepWithControllers", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleGrabbedObjectThirdStepWithControllers()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", basePos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "ShowExampleScaleGrabbedObjectFourthStepWithControllers", "oncompletetarget", gameObject));
    }

    private void ShowExampleScaleGrabbedObjectFourthStepWithControllers()
    {
        exampleLegenda.transform.SetParent(null);
        otherController.gameObject.SetActive(true);
        otherController.ShowExampleScaleMapFirstStepWithControllers();
    }

    public void ShowExampleMoveDropdownFirstStepWithControllers()
    {
        if (isControlledByOtherController)
        {
            animationAfterButtonPressAnimation = "ShowExampleMoveDropdownSecondStepWithControllers";
            startAnimationAfterEndDelay = "ShowExampleMoveDropdownFirstStepWithControllers";
            TurnOffRayAndButtonEffect(true);
            rayEndPosGameObject = exampleDropdown;
            BaseStartAnimation();
        } else
        {
            TurnOffRayAndButtonEffect(true);
            otherController.gameObject.SetActive(true);
            otherController.ShowExampleMoveDropdownFirstStepWithControllers();
        }
    }

    private void ShowExampleMoveDropdownSecondStepWithControllers()
    {
        primaryButtonArrow.gameObject.SetActive(false);
        foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>())
        {
            text.gameObject.SetActive(false);
        }
        iTween.MoveTo(gameObject, iTween.Hash("position", basePos - new Vector3(0, 0.1f, 0), "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateRayPos", "oncomplete", "ShowExampleMoveDropdownThirdStepWithControllers", "oncompletetarget", gameObject, "delay", 0.5f));
    }

    private void ShowExampleMoveDropdownThirdStepWithControllers()
    {
        steerStickArrow.gameObject.SetActive(true);
        steerStickArrow.DisplayTween();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", steerStickArrow.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveDropdownFourthStepWithControllers", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveDropdownFourthStepWithControllers()
    {
        steerStickArrowDown.gameObject.SetActive(true);
        steerStickArrowDown.DisplayTween();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", steerStickArrowDown.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveDropdownFifthStepWithControllers", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveDropdownFifthStepWithControllers()
    {
        steerStickArrowDown.gameObject.SetActive(false);
        steerStickArrowUp.gameObject.SetActive(true);
        steerStickArrowUp.DisplayTween();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", steerStickArrowUp.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", "ShowExampleMoveDropdownSixthStepWithControllers", "oncompletetarget", gameObject));
    }

    private void ShowExampleMoveDropdownSixthStepWithControllers()
    {
        steerStickArrowUp.gameObject.SetActive(false);
        EndDelayAnimation();
    }

    private void BaseStartAnimation()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", standardTweenStartPos, "time", 2f, "easetype", iTween.EaseType.easeInOutSine,
            "oncomplete", "RotateToLookAtObject", "oncompletetarget", gameObject, "delay", 1f));
    }

    private void RotateToLookAtObject()
    {
        if (rayEndPosGameObject.activeSelf)
        {
            iTween.LookTo(gameObject, iTween.Hash("looktarget", rayEndPosGameObject.transform.position, "time", 0.5f, "easetype", iTween.EaseType.easeInOutSine,
                "oncomplete", "TurnOnExampleRay", "oncompletetarget", gameObject));
        } else
        {
            EndDelayAnimation();
        }
    }

    private void EndDelayAnimation()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 1f, "onupdate", "EmptyUpdate",
            "oncomplete", startAnimationAfterEndDelay, "oncompletetarget", gameObject));
    }

    private void EmptyUpdate()
    {
    }

    private void UpdateRayPos()
    {
        Vector3 newStartExamplePos = transform.position - transform.up * 0.08f - transform.forward * 0.05f;
        Vector3 diffInPos = baseRayPos - newStartExamplePos;
        exampleRay.SetPositions(new Vector3[] { newStartExamplePos, rayEndPosGameObject.transform.position - diffInPos });
    }

    private void TurnOnExampleRay()
    {
        exampleRay.enabled = true;
        baseRayPos = transform.position - transform.up * 0.08f - transform.forward * 0.05f;
        exampleRay.SetPositions(new Vector3[] { baseRayPos, rayEndPosGameObject.transform.position });
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "onupdate", "EmptyUpdate", 
            "oncomplete", "ShowPrimaryButtonPressAnimation", "oncompletetarget", gameObject));
    }

    private void ShowPrimaryButtonPressAnimation()
    {
        if (showPrimaryAndGrip)
        {
            primaryButtonArrow.gameObject.SetActive(true);
            primaryButtonArrow.DisplayTween();
            gripArrow.gameObject.SetActive(true);
            gripArrow.DisplayTween();
        } else if (showPrimaryOnly)
        {
            primaryButtonArrow.gameObject.SetActive(true);
            primaryButtonArrow.DisplayTween();
        } else if (showGripOnly)
        {
            gripArrow.gameObject.SetActive(true);
            gripArrow.DisplayTween();
        }

        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", primaryButtonArrow.arrowMoveTweenTime, "onupdate", "EmptyUpdate",
            "oncomplete", animationAfterButtonPressAnimation, "oncompletetarget", gameObject));
    }

    private void TurnOffRayAndButtonEffect(bool resetPos)
    {
        if (resetPos)
        {
            transform.position = basePos;
            transform.eulerAngles = baseRot;

            exampleLegenda.transform.SetParent(null);
            exampleLegenda.transform.position = exampleLegendaBasePos;
            exampleLegenda.transform.eulerAngles = exampleLegendaBaseRot;
        }

        for (int i = 1; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        exampleRay.enabled = false;
    }

    public void TurnOffAllTweenObjects()
    {
        iTween.Stop(gameObject);

        for (int i = 0; i < allTweenObjects.Count; i++)
        {
            iTween.Stop(allTweenObjects[i]);
            allTweenObjects[i].SetActive(false);
        }

        if (!isControlledByOtherController)
        {
            otherController.TurnOffAllTweenObjects();
        }
    }
}
