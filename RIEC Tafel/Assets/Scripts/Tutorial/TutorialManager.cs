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
    public bool tutorialHasStarted = false, currentStepIsDone = false, checkForPOIOpen = false, checkForPOIPull = false, 
        cantZoomMap = false;

    private ButtonCommandText buttonCommandText = null;

    [SerializeField]
    private TMP_Text tutorialTitle = null, explanationText = null;

    private TMP_Text buttonCommandTextText = null;

    [SerializeField]
    private List<string> tutorialTitles = new List<string>(), buttonCommands = new List<string>(), 
        explanationTexts = new List<string>();

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
    private Transform legendaArrowBasePos = null, conclusionArrowBasePos = null, indicationArrowBasePos = null, 
        notesArrowBasePos = null;

    [SerializeField]
    private TutorialDropdown tutorialDropdown = null;

    private List<float> buttonPressTimers = new List<float>();

    [SerializeField]
    private TutorialRemoveStickyNoteButton removeStickyNoteButton = null;

    [System.NonSerialized]
    public List<StickyNote> stickyNotes = new List<StickyNote>();

    [System.NonSerialized]
    public int stickyNoteCount = 0;

    /// <summary>
    /// Initialize variables, turn off non required objects and retrieve data from the game manager
    /// </summary>

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

    /// <summary>
    /// Turn off objects not neccesary to the player at the start of the tutorial
    /// </summary>

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

    /// <summary>
    /// Use Q to skip steps in the tutorial. Perform the current step in the tutorial.
    /// The tutorial challenges list is filled with functions from this script to perform based on where in the tutorial the player is
    /// </summary>

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

    /// <summary>
    /// After a few second show which buttons the player has to hold if the player didnt start the tutorial yet
    /// </summary>

    private IEnumerator StartTutorial()
    {
        yield return new WaitForSeconds(timeBeforeTutorialStarts);
        if (!tutorialHasStarted)
        {
            EnablePrimaryArrows(true);
        }
    }

    /// <summary>
    /// Hold a primary button for a second to start the next step in the tutorial
    /// </summary>

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

    /// <summary>
    /// Activate learn UI button, to learn the player how to use buttons on the UI
    /// </summary>

    public void StartFirstStepOfTutorial()
    {
        learnUIButton.gameObject.SetActive(true);
        oculusAnimations.gameObject.SetActive(true);
        oculusAnimations.ShowExampleButtonAnimationFirstStep();
        EnablePrimaryArrows(false);
    }

    /// <summary>
    /// If the learn UI buttons has been pressed go to the next step
    /// </summary>

    public void FirstStepOfTutorial()
    {
        if (learnUIButton.hasBeenClicked)
        {
            learnUIButton.gameObject.SetActive(false);
            CelebrateCurrentStepInTurorial();
        }
    }

    /// <summary>
    /// Activate keyboard to learn the player how to use the keyboard and inputfield on the UI
    /// </summary>

    public void StartSecondStepOfTutorial()
    {
        learnUIInputField.gameObject.SetActive(true);
        keyBoard.gameObject.SetActive(true);
        EnablePrimaryArrows(false);
        oculusAnimations.ShowExampleInputfieldAnimationFirstStep();
    }

    /// <summary>
    /// If the player typed the correct sentence go to next step
    /// </summary>

    public void SecondStepOfTutorial()
    {
        if (learnUIInputField.text == "Volgende!" || learnUIInputField.text == "volgende!")
        {
            learnUIInputField.gameObject.SetActive(false);
            keyBoard.gameObject.SetActive(false);
            CelebrateCurrentStepInTurorial();
        }
    }

    /// <summary>
    /// For hands tutorial. Activates map and learns the player how to move the map.
    /// </summary>

    public void StartThirdStepOfTutorial()
    {
        oculusAnimations.ShowExampleMoveMapFirstStep();
        BaseThirdStepOfTutorial();
        EnablePrimaryArrows(false);
    }

    /// <summary>
    /// For controller tutorial. Activates map and learns the player how to move the map.
    /// </summary>

    public void StartThirdStepOfTutorialWithControllers()
    {
        oculusAnimations.ShowExampleMoveMapFirstStepWithControllers();
        BaseThirdStepOfTutorial();
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Right).EnableSteerStickArrow(true);
    }

    /// <summary>
    /// Base steps that always need to be done for the third step
    /// </summary>

    private void BaseThirdStepOfTutorial()
    {
        table.gameObject.SetActive(true);
        map.gameObject.SetActive(true);
        // makes sure map is visible for player
        map.SetNewMapCenter(abstractMap.CenterLatitudeLongitude);
        neededTutorialTransform = map.transform.position;
    }

    /// <summary>
    /// If the player changed the position of the map start the next step
    /// </summary>

    public void ThirdStepOfTutorial()
    {
        if (neededTutorialTransform != map.transform.position)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    /// <summary>
    /// For hands tutorial. Learns the player how to rotate the map.
    /// </summary>

    public void StartFourthStepOfTutorial()
    {
        oculusAnimations.ShowExampleRotateMapFirstStep();
        neededTutorialTransform = map.transform.eulerAngles;
        EnablePrimaryArrows(false);
        EnableGripArrows(false);
    }

    /// <summary>
    /// For controller tutorial. Learns the player how to rotate the map.
    /// </summary>

    public void StartFourthStepOfTutorialWithControllers()
    {
        oculusAnimations.ShowExampleRotateMapFirstStepWithControllers();
        neededTutorialTransform = map.transform.eulerAngles;
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Right).EnableTriggerArrow(true);
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Right).EnableGripArrow(true);
    }

    /// <summary>
    /// Starts next step if player changed rotation of map
    /// </summary>

    public void FourthStepOfTutorial()
    {
        if (neededTutorialTransform != map.transform.eulerAngles)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    /// <summary>
    /// For hands tutorial. Learns the player how to scale the map.
    /// </summary>

    public void StartFifthStepOfTutorial()
    {
        oculusAnimations.ShowExampleScaleMapFirstStep();
        neededTutorialTransform = map.transform.lossyScale;
        EnablePrimaryArrows(true);
    }

    /// <summary>
    /// For controller tutorial. Learns the player how to scale the map.
    /// </summary>

    public void StartFifthStepOfTutorialWithControllers()
    {
        oculusAnimations.ShowExampleScaleMapFirstStepWithControllers();
        neededTutorialTransform = map.transform.lossyScale;
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Right).EnablePrimaryButtonArrow(true);
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Right).EnableSecondaryButtonArrow(true);
    }

    /// <summary>
    /// When player changes scale of map go to next step
    /// </summary>

    public void FifthStepOfTutorial()
    {
        if (neededTutorialTransform != map.transform.lossyScale)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    /// <summary>
    /// Create a POI in the center of the map. Show explanation text of what it is. Checks if the player has a primary button down,
    /// so that player cannot accidentally skip this step of the tutorial by having the a button held from the previous step.
    /// </summary>

    public void StartSixthStepOfTutorial()
    {
        gameManager.gameObject.SetActive(true);
        gameManager.CreateTutorialLocationData("Regular", poiManager, abstractMap);

        CheckIfPrimaryButtonIsDown();
    }

    /// <summary>
    /// If the primary button was previously down, check if none of the primary buttons are down. If that is the case perform 
    /// introduction of tutorial step.
    /// </summary>

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

    /// <summary>
    /// Learn the player that if the player hovers the POI that shows extra information
    /// </summary>

    public void StartSeventhStepOfTutorial()
    {
        tutorialTitle.fontSize = 27;
        poi = FindObjectOfType<TutorialPOIText>();
        oculusAnimations.ShowExampleOpenPOIInformation();
        checkForPOIOpen = true;
    }

    /// <summary>
    /// If the POI has expanded its text go to next step of tutorial
    /// </summary>

    public void SeventhStepOfTutorial()
    {
        if (poi.hasBeenExpanded)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    /// <summary>
    /// Learn the player how to pull the POI in front of the player
    /// </summary>

    public void StartEightStepOfTutorial()
    {
        oculusAnimations.ShowExamplePullPOI();
        tutorialTitle.fontSize = 22;
        EnablePrimaryArrows(false);
        checkForPOIPull = true;
    }

    /// <summary>
    /// If the POI has performed its pull function go to next step of tutorial
    /// </summary>

    public void EightStepOfTutorial()
    {
        if (poi.hasBeenPulled)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    /// <summary>
    /// Explains grabbeble objects to the player (another explanation step, the player will have to hold a primary button to go to 
    /// next step)
    /// </summary>

    public void StartNinthStepOfTutorial()
    {
        BaseNinthStepOfTutorial();
    }

    /// <summary>
    /// Make sure the player cant scale the map when holding a primary button to go to the next step
    /// </summary>

    public void StartNinthStepOfTutorialWithControllers()
    {
        BaseNinthStepOfTutorial();
        cantZoomMap = true;
    }

    /// <summary>
    /// Activate the playertable to show where the grabbeble objects will be
    /// </summary>

    private void BaseNinthStepOfTutorial()
    {
        tutorialTitle.fontSize = 30;
        playerTable.gameObject.SetActive(true);
        CheckIfPrimaryButtonIsDown();
    }

    /// <summary>
    /// Hold primary button to go to next step
    /// </summary>

    public void NinthStepOfTutorial()
    {
        SixthStepOfTutorial();
    }

    /// <summary>
    /// Turn on the legenda. Learn the player how to grab objects
    /// </summary>

    public void StartTenthStepOfTutorial()
    {
        legenda.gameObject.SetActive(true);
        // Is the arrow that points at the legenda
        grabbebleObjectArrows[0].SetTweenPosition(legendaArrowBasePos);
        grabbebleObjectArrows[0].connectedGameObject = legenda;
        EnableGripArrows(false);
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        oculusAnimations.ShowExampleGrabObjectFirstStep();
    }

    /// <summary>
    /// If legenda image has been turned on and then is deactivated again, go to next step
    /// </summary>

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

    /// <summary>
    /// Learn player how to move grabbeble objects in hands tutorial
    /// </summary>

    public void StartEleventhStepOfTutorial()
    {
        oculusAnimations.ShowExampleMoveGrabbedObjectFirstStep();
        BaseStartEleventhStepOfTutorial();
        EnablePrimaryArrows(false);
    }

    /// <summary>
    /// Learn player how to move grabbeble objects in controller tutorial
    /// </summary>

    public void StartEleventhStepOfTutorialWithControllers()
    {
        cantZoomMap = false;
        oculusAnimations.ShowExampleMoveGrabbedObjectFirstStepWithControllers();
        BaseStartEleventhStepOfTutorial();
        legenda.gameObject.SetActive(true);
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Left).EnableSteerStickArrow(true);
    }

    /// <summary>
    /// Initialize data for 11th step of tutorial
    /// </summary>

    private void BaseStartEleventhStepOfTutorial()
    {
        tutorialTitle.fontSize = 24f;
        neededTutorialTransform = legendaImage.transform.position;
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        grabbebleObjectArrows[0].connectedGameObject = legenda;
        completedCanvasStep = false;
        EnableGripArrows(false);
    }

    /// <summary>
    /// If the player changed the position of the legenda and turned the legenda off again, go to next step
    /// </summary>

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

    /// <summary>
    /// Learn the player how to scale grabbeble objects hand tutorial.
    /// </summary>

    public void StartTwelvethStepOfTutorial()
    {
        oculusAnimations.ShowExampleScaleGrabbedObjectFirstStep();
        BaseStartTwelvethStepOfTutorial();
        EnablePrimaryArrows(true);
    }

    /// <summary>
    /// Learn the player how to scale grabbeble objects controller tutorial.
    /// </summary>

    public void StartTwelvethStepOfTutorialWithControllers()
    {
        oculusAnimations.ShowExampleScaleGrabbedObjectFirstStepWithControllers();
        BaseStartTwelvethStepOfTutorial();
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Left).EnableSecondaryButtonArrow(true);
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Left).EnablePrimaryButtonArrow(true);
    }

    /// <summary>
    /// Initialize correct data for 12th step of tutorial
    /// </summary>

    private void BaseStartTwelvethStepOfTutorial()
    {
        neededTutorialTransform = legendaImage.transform.lossyScale;
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        completedCanvasStep = false;
        EnableGripArrows(false);
    }

    /// <summary>
    /// If the player changed the scale of the legenda and turned the legenda off again, go to next step
    /// </summary>

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

    /// <summary>
    /// Explain the use of conclusions and indications in this step in hands tutorial
    /// </summary>

    public void StartThirthteenthStepOfTutorial()
    {
        tutorialTitle.fontSize = 30;
        for (int i = 0; i < dataExplanations.Count; i++)
        {
            dataExplanations[i].gameObject.SetActive(true);
        }

        // Point the arrows at conclusions and indications instead of legenda
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

    /// <summary>
    /// Learn the user how to scroll throught dropdowns in the controller tutorial
    /// </summary>

    public void StartThirthteenthStepOfTutorialWithControllers()
    {
        oculusAnimations.ShowExampleMoveDropdownFirstStepWithControllers();
        EnablePrimaryArrows(false);
        EnableGripArrows(false);
        SearchForCorrectHand(TutorialHands.HandCharacteristic.Left).EnableSteerStickArrow(true);
        tutorialDropdown.gameObject.SetActive(true);
    }

    /// <summary>
    /// Standard explanation text step like step 6 & 9. Cant go to next step when an canvasobject is grabbed.
    /// </summary>

    public void ThirthteenthStepOfTutorial()
    {
        TurnOffGrabbebleObjectArrows(dataExplanationSet);
        NewCanvasObjectStep();
    }

    /// <summary>
    /// If the player scrolled throught dropdown using the controller inputs finish the tutorial
    /// </summary>

    public void ThirthteenthStepOfTutorialWithControllers()
    {
        if (tutorialDropdown.scrolledWithControllerInput)
        {
            CelebrateCurrentStepInTurorial();
        }
    }

    /// <summary>
    /// Explain the use of notes in this step of the tutorial
    /// </summary>

    public void StartFourteenthStepOfTutorial()
    {
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        grabbebleObjectArrows[0].SetTweenPosition(notesArrowBasePos);
        grabbebleObjectArrows[0].connectedGameObject = notes;
        grabbebleObjectArrows[1].gameObject.SetActive(false);
        notes.gameObject.SetActive(true);
        CheckIfPrimaryButtonIsDown();
    }

    /// <summary>
    /// Standard explanation text step like step 6 & 9. Cant go to next step when an canvasobject is grabbed.
    /// </summary>

    public void FourteenthStepOfTutorial()
    {
        TurnOffGrabbebleObjectArrows(notesSet);
        NewCanvasObjectStep();
    }

    /// <summary>
    /// Learns the player how to create a new stickynote and move it
    /// </summary>

    public void StartFifteenthStepOfTutorial()
    {
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        EnablePrimaryArrows(false);
        EnableGripArrows(false);
        oculusAnimations.ShowExampleCreateAndMoveNotesFirstStep();
        completedCanvasStep = false;
        tutorialTitle.fontSize = 28;
    }

    /// <summary>
    /// If the player created a stickynote and moved its position and then turned off the notes UI, go to next step
    /// </summary>

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

    /// <summary>
    /// Learns the player how to edit a sticky note and how to stop editing a sticky note
    /// </summary>

    public void StartSixteenthStepOfTutorial()
    {
        grabbebleObjectArrows[0].gameObject.SetActive(true);
        EnablePrimaryArrows(false);
        EnableGripArrows(false);
        oculusAnimations.ShowExampleStartAndStopEditNotesFirstStep();
        completedCanvasStep = false;
        tutorialTitle.fontSize = 25;
    }

    /// <summary>
    /// Double tap a primary button to start editing a sticky note, stop editing by double tapping beside the sticky note
    /// If the UI is closed and the title and body of a stickynote have been edited and the the sticky note was not being edited
    /// anymore, go to next step
    /// </summary>

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

    /// <summary>
    /// 
    /// </summary>

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
