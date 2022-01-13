using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.XR;
using Mapbox.Unity.Map;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    private List<TutorialHands> tutorialHands = new List<TutorialHands>();

    [SerializeField]
    private float timeBeforeTutorialStarts = 5;

    public int stepInTutorial { get; set; } = 0;

    [System.NonSerialized]
    public bool tutorialHasStarted = false, currentStepIsDone = false, checkForPOIOpen = false, checkForPOIPull = false, cantZoomMap = false;

    private ButtonCommandText buttonCommandText = null;

    [SerializeField]
    private TMP_Text tutorialTitle = null, explanationText = null;

    private TMP_Text buttonCommandTextText = null;

    [SerializeField]
    private List<string> tutorialTitles = new List<string>(), buttonCommands = new List<string>(), explanationTexts = new List<string>();

    [SerializeField]
    private List<UnityEvent> tutorialChallenges = new List<UnityEvent>(), initialStartUpForTutorial = new List<UnityEvent>();

    private InputDeviceCharacteristics controllerCharacteristics = InputDeviceCharacteristics.Controller;

    private List<InputDevice> inputDevices = new List<InputDevice>();

    [SerializeField]
    private LearnUIButton learnUIButton = null;

    [SerializeField]
    private TMP_InputField learnUIInputField = null;

    [SerializeField]
    private KeyBoard keyBoard = null;

    [SerializeField]
    private Table table = null;

    [SerializeField]
    private GameManager gameManager = null;

    [SerializeField]
    private MoveMap map = null;

    [SerializeField]
    private POIManager poiManager = null;

    [SerializeField]
    private PlayerTable playerTable = null;

    [SerializeField]
    private Notes notes = null;

    [SerializeField]
    private List<DataExplanations> dataExplanations = new List<DataExplanations>();

    [SerializeField]
    private Legenda legenda = null;

    [SerializeField]
    private List<GameObject> objectsToDeactivate = new List<GameObject>(), canvasObjects = new List<GameObject>();

    [SerializeField]
    private OculusAnimations oculusAnimations = null;

    private TutorialPOIText poi = null;

    private Vector3 neededTutorialTransform = Vector3.zero;
    private AbstractMap abstractMap = null;

    private bool primaryButtonIsDown = false, canvasObjectHasBeenOpened = false, completedCanvasStep = false;

    [SerializeField]
    private GameObject legendaImage = null, dataExplanationSet = null, notesSet = null;

    [SerializeField]
    private List<TutorialControllerArrows> grabbebleObjectArrows = new List<TutorialControllerArrows>();

    [SerializeField]
    private Transform legendaArrowBasePos = null, conclusionArrowBasePos = null, indicationArrowBasePos = null, notesArrowBasePos = null;

    [SerializeField]
    private TutorialDropdown tutorialDropdown = null;

    private List<float> buttonPressTimers = new List<float>();

    [SerializeField]
    private TutorialRemoveStickyNoteButton removeStickyNoteButton = null;

    [System.NonSerialized]
    public List<StickyNote> stickyNotes = new List<StickyNote>();

    [System.NonSerialized]
    public int stickyNoteCount = 0;

    private void Start()
    {
        SaveSytem.SaveGame();
        abstractMap = map.GetComponent<AbstractMap>();
        buttonCommandText = FindObjectOfType<ButtonCommandText>();
        buttonCommandTextText = buttonCommandText.GetComponent<TMP_Text>();
        removeStickyNoteButton.gameObject.SetActive(false);

        for (int i = 0; i < 2; i++)
        {
            buttonPressTimers.Add(0);
        }

        StartCoroutine(StartTutorial());
        InitializeControllers();
        StartCoroutine(TurnOffMapAndTable());
        StartCoroutine(gameManager.RetrieveCityData());
    }

    private IEnumerator TurnOffMapAndTable()
    {
        yield return new WaitForEndOfFrame();
        map.gameObject.SetActive(false);
        table.gameObject.SetActive(false);
        oculusAnimations.gameObject.SetActive(false);

        for (int i = 0; i < objectsToDeactivate.Count; i++)
        {
            objectsToDeactivate[i].SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            tutorialHasStarted = true;
            CelebrateCurrentStepInTurorial();
        }

        if (inputDevices.Count < 2)
        {
            InitializeControllers();
            return;
        }

        if (!currentStepIsDone)
        {
            tutorialChallenges[stepInTutorial].Invoke();
        }
    }

    private IEnumerator StartTutorial()
    {
        yield return new WaitForSeconds(timeBeforeTutorialStarts);
        if (!tutorialHasStarted)
        {
            EnablePrimaryArrows(true);
        }
    }

    public void IntroductionOfTutorial()
    {
        for (int i = 0; i < inputDevices.Count; i++)
        {
            if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton)
            {
                buttonPressTimers[i] += Time.deltaTime;
                if (buttonPressTimers[i] >= 1)
                {
                    tutorialHasStarted = true;
                    TurnOffAllHandArrows();
                    StartNextStepInTutorial();
                }
            } else
            {
                buttonPressTimers[i] = 0;
            }
        }
    }

    public void StartFirstStepOfTutorial()
    {
        learnUIButton.gameObject.SetActive(true);
        oculusAnimations.gameObject.SetActive(true);
        oculusAnimations.ShowExampleButtonAnimationFirstStep();
        EnablePrimaryArrows(false);
    }

    public void FirstStepOfTutorial()
    {
        if (learnUIButton.hasBeenClicked)
        {
            learnUIButton.gameObject.SetActive(false);
            CelebrateCurrentStepInTurorial();
        }
    }

    public void StartSecondStepOfTutorial()
    {
        learnUIInputField.gameObject.SetActive(true);
        keyBoard.gameObject.SetActive(true);
        EnablePrimaryArrows(false);
        oculusAnimations.ShowExampleInputfieldAnimationFirstStep();
    }

    public void SecondStepOfTutorial()
    {
        if (learnUIInputField.text == "Volgende!" || learnUIInputField.text == "volgende!")
        {
            learnUIInputField.gameObject.SetActive(false);
            keyBoard.gameObject.SetActive(false);
            CelebrateCurrentStepInTurorial();
        }
    }

    public void StartThirdStepOfTutorial()
    {
        oculusAnimations.ShowExampleMoveMapFirstStep();
        BaseThirdStepOfTutorial();
        EnablePrimaryArrows(false);
    }

    public void StartThirdStepOfTutorialWithControllers()
    {
        oculusAnimations.ShowExampleMoveMapFirstStepWithControllers();
        BaseThirdStepOfTutorial();
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Right).EnableSteerStickArrow(true);
    }

    private void BaseThirdStepOfTutorial()
    {
        table.gameObject.SetActive(true);
        map.gameObject.SetActive(true);
        map.SetNewMapCenter(abstractMap.CenterLatitudeLongitude);
        neededTutorialTransform = map.transform.position;
    }

    public void ThirdStepOfTutorial()
    {
        if (neededTutorialTransform != map.transform.position)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    public void StartFourthStepOfTutorial()
    {
        oculusAnimations.ShowExampleRotateMapFirstStep();
        neededTutorialTransform = map.transform.eulerAngles;
        EnablePrimaryArrows(false);
        EnableGripArrows(false);
    }

    public void StartFourthStepOfTutorialWithControllers()
    {
        oculusAnimations.ShowExampleRotateMapFirstStepWithControllers();
        neededTutorialTransform = map.transform.eulerAngles;
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Right).EnableTriggerArrow(true);
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Right).EnableGripArrow(true);
    }

    public void FourthStepOfTutorial()
    {
        if (neededTutorialTransform != map.transform.eulerAngles)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    public void StartFifthStepOfTutorial()
    {
        oculusAnimations.ShowExampleScaleMapFirstStep();
        neededTutorialTransform = map.transform.lossyScale;
        EnablePrimaryArrows(true);
    }

    public void StartFifthStepOfTutorialWithControllers()
    {
        oculusAnimations.ShowExampleScaleMapFirstStepWithControllers();
        neededTutorialTransform = map.transform.lossyScale;
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Right).EnablePrimaryButtonArrow(true);
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Right).EnableSecondaryButtonArrow(true);
    }

    public void FifthStepOfTutorial()
    {
        if (neededTutorialTransform != map.transform.lossyScale)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    public void StartSixthStepOfTutorial()
    {
        gameManager.gameObject.SetActive(true);
        gameManager.CreateTutorialLocationData("Regular", poiManager, abstractMap);

        CheckIfPrimaryButtonIsDown();
    }

    public void SixthStepOfTutorial()
    {
        if (primaryButtonIsDown)
        {
            int primaryButtonCount = 0;
            for (int i = 0; i < inputDevices.Count; i++)
            {
                if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && !primaryButton)
                {
                    primaryButtonCount++;
                }
            }

            if (primaryButtonCount == 2)
            {
                primaryButtonIsDown = false;
            }
            else
            {
                return;
            }
        }

        IntroductionOfTutorial();
    }

    public void StartSeventhStepOfTutorial()
    {
        tutorialTitle.fontSize = 27;
        poi = FindObjectOfType<TutorialPOIText>();
        oculusAnimations.ShowExampleOpenPOIInformation();
        checkForPOIOpen = true;
    }

    public void SeventhStepOfTutorial()
    {
        if (poi.hasBeenExpanded)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    public void StartEightStepOfTutorial()
    {
        oculusAnimations.ShowExamplePullPOI();
        tutorialTitle.fontSize = 22;
        EnablePrimaryArrows(false);
        checkForPOIPull = true;
    }

    public void EightStepOfTutorial()
    {
        if (poi.hasBeenPulled)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    public void StartNinthStepOfTutorial()
    {
        BaseNinthStepOfTutorial();
    }

    public void StartNinthStepOfTutorialWithControllers()
    {
        BaseNinthStepOfTutorial();
        cantZoomMap = true;
    }

    private void BaseNinthStepOfTutorial()
    {
        tutorialTitle.fontSize = 30;
        playerTable.gameObject.SetActive(true);
        CheckIfPrimaryButtonIsDown();
    }

    public void NinthStepOfTutorial()
    {
        SixthStepOfTutorial();
    }

    public void StartTenthStepOfTutorial()
    {
        legenda.gameObject.SetActive(true);
        grabbebleObjectArrows[0].SetTweenPosition(legendaArrowBasePos);
        grabbebleObjectArrows[0].connectedGameObject = legenda;
        EnableGripArrows(false);
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        oculusAnimations.ShowExampleGrabObjectFirstStep();
    }

    public void TenthStepOfTutorial()
    {
        TurnOffGrabbebleObjectArrows(legendaImage);

        if (!legendaImage.activeSelf)
        {
            if (canvasObjectHasBeenOpened)
            {
                CelebrateCurrentStepInTurorial();
            }
        }
    }

    public void StartEleventhStepOfTutorial()
    {
        oculusAnimations.ShowExampleMoveGrabbedObjectFirstStep();
        BaseStartEleventhStepOfTutorial();
        EnablePrimaryArrows(false);
    }

    public void StartEleventhStepOfTutorialWithControllers()
    {
        cantZoomMap = false;
        oculusAnimations.ShowExampleMoveGrabbedObjectFirstStepWithControllers();
        BaseStartEleventhStepOfTutorial();
        legenda.gameObject.SetActive(true);
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Left).EnableSteerStickArrow(true);
    }

    private void BaseStartEleventhStepOfTutorial()
    {
        tutorialTitle.fontSize = 24f;
        neededTutorialTransform = legendaImage.transform.position;
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        grabbebleObjectArrows[0].connectedGameObject = legenda;
        completedCanvasStep = false;
        EnableGripArrows(false);
    }

    public void EleventhStepOfTutorial()
    {
        TurnOffGrabbebleObjectArrows(legendaImage);

        if (legendaImage.activeSelf)
        {
            if (neededTutorialTransform != legendaImage.transform.position)
            {
                completedCanvasStep = true;
            }
        } else if (completedCanvasStep && !legendaImage.activeSelf)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    public void StartTwelvethStepOfTutorial()
    {
        oculusAnimations.ShowExampleScaleGrabbedObjectFirstStep();
        BaseStartTwelvethStepOfTutorial();
        EnablePrimaryArrows(true);
    }

    public void StartTwelvethStepOfTutorialWithControllers()
    {
        oculusAnimations.ShowExampleScaleGrabbedObjectFirstStepWithControllers();
        BaseStartTwelvethStepOfTutorial();
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Left).EnableSecondaryButtonArrow(true);
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Left).EnablePrimaryButtonArrow(true);
    }

    private void BaseStartTwelvethStepOfTutorial()
    {
        neededTutorialTransform = legendaImage.transform.lossyScale;
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        completedCanvasStep = false;
        EnableGripArrows(false);
    }

    public void TwelvethStepOfTutorial()
    {
        TurnOffGrabbebleObjectArrows(legendaImage);

        if (legendaImage.activeSelf)
        {
            if (neededTutorialTransform != legendaImage.transform.lossyScale)
            {
                completedCanvasStep = true;
            }
        } else if(completedCanvasStep && !legendaImage.activeSelf)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    public void StartThirthteenthStepOfTutorial()
    {
        tutorialTitle.fontSize = 30;
        for (int i = 0; i < dataExplanations.Count; i++)
        {
            dataExplanations[i].gameObject.SetActive(true);
        }

        grabbebleObjectArrows[0].gameObject.SetActive(true);
        grabbebleObjectArrows[0].SetTweenPosition(conclusionArrowBasePos);
        grabbebleObjectArrows[0].connectedGameObject = 
            dataExplanations.Find(explantion => explantion.dataSetNeeded == DataExplanations.DataSetNeeded.Conclusion);
        grabbebleObjectArrows[1].gameObject.SetActive(true);
        grabbebleObjectArrows[1].SetTweenPosition(indicationArrowBasePos);
        grabbebleObjectArrows[1].connectedGameObject =
            dataExplanations.Find(explantion => explantion.dataSetNeeded == DataExplanations.DataSetNeeded.Indication);

        CheckIfPrimaryButtonIsDown();
    }

    public void StartThirthteenthStepOfTutorialWithControllers()
    {
        oculusAnimations.ShowExampleMoveDropdownFirstStepWithControllers();
        EnablePrimaryArrows(false);
        EnableGripArrows(false);
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Left).EnableSteerStickArrow(true);
        tutorialDropdown.gameObject.SetActive(true);
    }

    public void ThirthteenthStepOfTutorial()
    {
        TurnOffGrabbebleObjectArrows(dataExplanationSet);
        NewCanvasObjectStep();
    }

    public void ThirthteenthStepOfTutorialWithControllers()
    {
        if (tutorialDropdown.scrolledWithControllerInput)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    public void StartFourteenthStepOfTutorial()
    {
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        grabbebleObjectArrows[0].SetTweenPosition(notesArrowBasePos);
        grabbebleObjectArrows[0].connectedGameObject = notes;
        grabbebleObjectArrows[1].gameObject.SetActive(false);
        notes.gameObject.SetActive(true);
        CheckIfPrimaryButtonIsDown();
    }

    public void FourteenthStepOfTutorial()
    {
        TurnOffGrabbebleObjectArrows(notesSet);
        NewCanvasObjectStep();
    }

    public void StartFifteenthStepOfTutorial()
    {
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        EnablePrimaryArrows(false);
        EnableGripArrows(false);
        oculusAnimations.ShowExampleCreateAndMoveNotesFirstStep();
        completedCanvasStep = false;
        tutorialTitle.fontSize = 28;
    }

    public void FifteenthStepOfTutorial()
    {
        TurnOffGrabbebleObjectArrows(notesSet);

        if (notesSet.activeSelf)
        {
            if (stickyNotes.Count > 0 && !completedCanvasStep)
            {
                for (int i = 0; i < stickyNotes.Count; i++)
                {
                    if (stickyNotes[i].transform.localPosition != Vector3.zero)
                    {
                        completedCanvasStep = true;
                    }
                }
            }
        }
        else if (completedCanvasStep && !notesSet.activeSelf)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    public void StartSixteenthStepOfTutorial()
    {
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        EnablePrimaryArrows(false);
        EnableGripArrows(false);
        oculusAnimations.ShowExampleStartAndStopEditNotesFirstStep();
        completedCanvasStep = false;
        tutorialTitle.fontSize = 25;
    }

    public void SixteenthStepOfTutorial()
    {
        TurnOffGrabbebleObjectArrows(notesSet);

        if (notesSet.activeSelf)
        {
            if (!completedCanvasStep)
            {
                for (int i = 0; i < stickyNotes.Count; i++)
                {
                    bool textsHaveBeenEdited = false;
                    int amountOfTexts = 0;
                    int textsHaveBeenEditedCounter = 0;
                    foreach(NotesText text in stickyNotes[i].GetComponentsInChildren<NotesText>())
                    {
                        amountOfTexts++;
                        if (!text.firstTimeEdit && text.text.text != "")
                        {
                            textsHaveBeenEditedCounter++;
                        }
                    }

                    if (textsHaveBeenEditedCounter == amountOfTexts)
                    {
                        textsHaveBeenEdited = true;
                    }

                    if (textsHaveBeenEdited && !stickyNotes[i].GetStickyNoteBeingEdited())
                    {
                        completedCanvasStep = true;
                    }
                }
            }
        }
        else if (completedCanvasStep && !notesSet.activeSelf)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    public void StartSeventeenthStepOfTutorial()
    {
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        EnablePrimaryArrows(false);
        EnableGripArrows(false);
        oculusAnimations.ShowExampleCreateAndMoveNotesFirstStep();
        completedCanvasStep = false;
        removeStickyNoteButton.gameObject.SetActive(true);
        tutorialTitle.fontSize = 30;
    }

    public void SeventeenthStepOfTutorial()
    {
        TurnOffGrabbebleObjectArrows(notesSet);

        if (notesSet.activeSelf)
        {
            if (!completedCanvasStep)
            {
                if (stickyNotes.Count < stickyNoteCount)
                {
                    completedCanvasStep = true;
                }
            }
        }
        else if (completedCanvasStep && !notesSet.activeSelf)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    private void NewCanvasObjectStep()
    {
        for (int i = 0; i < canvasObjects.Count; i++)
        {
            if (canvasObjects[i].activeSelf)
            {
                return;
            }
        }
        SixthStepOfTutorial();
    }

    private void CheckIfPrimaryButtonIsDown()
    {
        ResetButtonPressTimers();
        oculusAnimations.gameObject.SetActive(false);
        EnablePrimaryArrows(true);

        for (int i = 0; i < inputDevices.Count; i++)
        {
            if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton)
            {
                primaryButtonIsDown = true;
            }
        }
    }

    private void EnablePrimaryArrows(bool bothHands)
    {
        if (bothHands)
        {
            for (int i = 0; i < tutorialHands.Count; i++)
            {
                tutorialHands[i].EnablePrimaryButtonArrow(true);
            }
        }
        else
        {
            SearchForCorrectHand(TutorialHands.HandCharacteristic.Right).EnablePrimaryButtonArrow(true);
        }
    }

    private void EnableGripArrows(bool bothHands)
    {
        if (bothHands)
        {
            for (int i = 0; i < tutorialHands.Count; i++)
            {
                tutorialHands[i].EnableGripArrow(true);
            }
        }
        else
        {
            SearchForCorrectHand(TutorialHands.HandCharacteristic.Right).EnableGripArrow(true);
        }
    }

    private void CelebrateCurrentStepInTurorial()
    {
        TurnOffAllHandArrows();
        oculusAnimations.TurnOffAllTweenObjects();
        oculusAnimations.gameObject.SetActive(false);
        currentStepIsDone = true;
        buttonCommandText.StartCelebrateStepDone();
    }

    private void TurnOffAllHandArrows()
    {
        for (int i = 0; i < tutorialHands.Count; i++)
        {
            tutorialHands[i].ClearAllArrows();
        }
    }

    private void TurnOffGrabbebleObjectArrows(GameObject canvasObject)
    {
        if (canvasObject.activeSelf)
        {
            if (!canvasObjectHasBeenOpened)
            {
                canvasObjectHasBeenOpened = true;
            }
        }

        for (int i = 0; i < grabbebleObjectArrows.Count; i++)
        {
            if (!grabbebleObjectArrows[i].gameObject.activeSelf) { continue; }
            if (grabbebleObjectArrows[i].connectedGameObject.isGrabbed)
            {
                grabbebleObjectArrows[i].gameObject.SetActive(false);
            }
        }
    }

    public void StartNextStepInTutorial()
    {
        stepInTutorial++;
        if (stepInTutorial > tutorialChallenges.Count - 1)
        {
            SceneManager.LoadScene(TutorialSceneManager.sceneToSwitchTo);
            return;
        }

        tutorialTitle.text = tutorialTitles[stepInTutorial];
        buttonCommandTextText.text = buttonCommands[stepInTutorial];
        explanationText.text = explanationTexts[stepInTutorial];
        currentStepIsDone = false;
        canvasObjectHasBeenOpened = false;
        oculusAnimations.gameObject.SetActive(true);

        if (initialStartUpForTutorial[stepInTutorial] != null)
        {
            initialStartUpForTutorial[stepInTutorial].Invoke();
        }
    }

    private void InitializeControllers()
    {
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, inputDevices);
        for (int i = 0; i < inputDevices.Count; i++)
        {
            if (!inputDevices[i].isValid)
            {
                inputDevices.Clear();
                break;
            }
        }
    }

    private void ResetButtonPressTimers()
    {
        for (int i = 0; i < buttonPressTimers.Count; i++)
        {
            buttonPressTimers[i] = 0;
        }
    }

    private TutorialHands SearchForCorrectHand(TutorialHands.HandCharacteristic handCharacteristic)
    {
        return tutorialHands.Find(hand => hand.handCharacteristic == handCharacteristic);
    }

    public void AddTutorialHandToList(TutorialHands tutorialHand)
    {
        tutorialHands.Add(tutorialHand);
    }
}
