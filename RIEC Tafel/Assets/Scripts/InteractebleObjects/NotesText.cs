using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR;
using UnityEngine.Events;

public class NotesText : MonoBehaviour
{
    private Notes notes = null;

    [System.NonSerialized]
    public TMP_Text text = null;

    private EditebleText editebleText = null;

    private StickyNote stickyNote = null;

    private List<InputDevice> inputDevices = new List<InputDevice>();

    private List<PlayerHandRays> handRays = new List<PlayerHandRays>();

    private List<int> buttonPressCount = new List<int>();

    private List<float> buttonPressIntervals = new List<float>();

    private List<bool> skippedControllers = new List<bool>();

    private PlayerHandsRayInteractor interactor = null;

    private KeyBoard keyBoard = null;

    private bool isBeingHovered = false;

    [SerializeField]
    private bool cantMoveText = false;

    [System.NonSerialized]
    public bool isCurrentlyEdited = false, firstTimeEdit = true;

    private Vector3 originalPos = Vector3.zero;

    /// <summary>
    /// Initialize all required variables
    /// </summary>

    private void Start()
    {
        stickyNote = GetComponentInParent<StickyNote>();
        text = GetComponent<TMP_Text>();
        editebleText = GetComponent<EditebleText>();
        notes = FindObjectOfType<Notes>();
        keyBoard = FindObjectOfType<KeyBoard>();
        interactor = GetComponentInParent<PlayerHandsRayInteractor>();
        originalPos = text.rectTransform.localPosition;

        interactor.objectHoverEnteredEvent.AddListener(SetObjectToBeingHovered);
        interactor.objectHoverExitedEvent.AddListener(SetObjectToBeingUnHovered);

        for (int i = 0; i < 2; i++)
        {
            buttonPressCount.Add(0);
            buttonPressIntervals.Add(0);
            skippedControllers.Add(false);
        }
    }

    /// <summary>
    /// Check if players hovers the keyboard and if the player should no longer be editing the stickynote if the stickynote is being
    /// edited. Else check if the player is going to edit the stickynote.
    /// </summary>

    private void Update()
    {
        if (inputDevices.Count < 2)
        {
            inputDevices = stickyNote.inputDevices;
            handRays = stickyNote.handRays;
            return;
        }

        if (isCurrentlyEdited)
        {
            // Check if the keyboard is being hovered (else the player could accidentally stop edting the sticky note by typing)
            for (int i = 0; i < skippedControllers.Count; i++)
            {
                skippedControllers[i] = false;
            }

            if (keyBoard.targetedControllerNodes.Contains(XRNode.LeftHand))
            {
                skippedControllers[handRays.IndexOf(handRays.Find(ray => ray.hand == PlayerHandRays.Hand.Left))] = true;
            }

            if (keyBoard.targetedControllerNodes.Contains(XRNode.RightHand))
            {
                skippedControllers[handRays.IndexOf(handRays.Find(ray => ray.hand == PlayerHandRays.Hand.Right))] = true;
            }

            BaseUpdate(() => OnDisable(), false);

            if (notes.notesText != text)
            {
                OnDisable();
            }
        }

        if (!isBeingHovered)
        {
            return;
        }

        if (!isCurrentlyEdited)
        {
            BaseUpdate(() => SelectNotesText(), true);
        }
    }

    /// <summary>
    /// Checks for dubble tap of the primary button, if the player dubbel taps perform the dubble tap action.
    /// </summary>

    private void BaseUpdate(UnityAction doubleTapAction, bool containsInteractor)
    {
        for (int i = 0; i < inputDevices.Count; i++)
        {
            // Skip controller if hovering keyboard
            if (skippedControllers[i]) { continue; }

            if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton &&
                handRays[i].hoveredObjects.Contains(interactor) == containsInteractor)
            {
                // Check for the first button press and set the button press interval
                if (buttonPressCount[i] == 0)
                {
                    buttonPressCount[i]++;
                    buttonPressIntervals[i] = 0.5f;
                }
                // Perform function if player tapped the button twice 
                else if (buttonPressCount[i] == 2)
                {
                    doubleTapAction.Invoke();
                }
            }
            // Add to button press count when player releases button
            else if (!primaryButton && handRays[i].hoveredObjects.Contains(interactor) == containsInteractor && 
                buttonPressCount[i] == 1)
            {
                buttonPressCount[i]++;
            }

            // Reset buttonpress count if the interval gets to big or if player does the opposite of contains interactor
            if (handRays[i].hoveredObjects.Contains(interactor) != containsInteractor || buttonPressIntervals[i] <= 0)
            {
                buttonPressCount[i] = 0;
            }

            buttonPressIntervals[i] -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Set the text to being edited.
    /// </summary>

    private void SelectNotesText()
    {
        if (!isCurrentlyEdited)
        {
            // Remove all the baseline text if this is the first edit
            if (firstTimeEdit)
            {
                firstTimeEdit = false;
                text.text = "";
            }

            // Reset all variables that are required for selecting text
            for (int i = 0; i < buttonPressCount.Count; i++)
            {
                buttonPressCount[i] = 0;
                skippedControllers[i] = false;
            }

            isCurrentlyEdited = true;
            notes.notesText = text;
            notes.cantMoveText = cantMoveText;
            notes.notesCenterPos = originalPos;
            editebleText.SetAsEditeble();
            stickyNote.SetAsLastChild();
        }
    }

    /// <summary>
    /// Set text to no longer being edited.
    /// </summary>

    private void OnDisable()
    {
        if (notes.notesText == text)
        {
            notes.notesText = null;
        }

        isCurrentlyEdited = false;
        editebleText.isCurrentlyEdited = false;

        // Text can end with this sign that shows where the user is editing, remove it if it ends with this sign
        if (text.text.EndsWith("|"))
        {
            text.text = text.text.Remove(text.text.Length - 1);
        }
    }

    /// <summary>
    /// Set to being hovered if player aims at object
    /// </summary>

    private void SetObjectToBeingHovered()
    {
        isBeingHovered = true;
    }

    /// <summary>
    /// Set to being not hovered if player no longer aims at object
    /// </summary>

    private void SetObjectToBeingUnHovered()
    {
        isBeingHovered = false;
    }
}
