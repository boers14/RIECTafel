using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR;
using UnityEngine.Events;
using TextSpeech;

public class NotesText : MonoBehaviour
{
    private Notes notes = null;

    [System.NonSerialized]
    public TMP_Text text = null;

    private StickyNote stickyNote = null;

    private List<InputDevice> inputDevices = new List<InputDevice>();

    private List<PlayerHandRays> handRays = new List<PlayerHandRays>();

    private List<int> buttonPressCount = new List<int>();

    private List<float> buttonPressIntervals = new List<float>();

    private PlayerHandsRayInteractor interactor = null;

    private KeyBoard keyBoard = null;

    private bool isBeingHovered = false, firstTimeEdit = true;

    [SerializeField]
    private bool cantMoveText = false;

    [System.NonSerialized]
    public bool isCurrentlyEdited = false;

    private Vector3 originalPos = Vector3.zero;

    private void Start()
    {
        stickyNote = GetComponentInParent<StickyNote>();
        text = GetComponent<TMP_Text>();
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
        }
    }

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

    private void BaseUpdate(UnityAction doubleTapAction, bool containsInteractor)
    {
        for (int i = 0; i < inputDevices.Count; i++)
        {
            if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton &&
                handRays[i].hoveredObjects.Contains(interactor) == containsInteractor)
            {
                if (buttonPressCount[i] == 0)
                {
                    buttonPressCount[i]++;
                    buttonPressIntervals[i] = 0.5f;
                }
                else if (buttonPressCount[i] == 2)
                {
                    doubleTapAction.Invoke();
                }
            }
            else if (!primaryButton && handRays[i].hoveredObjects.Contains(interactor) == containsInteractor && buttonPressCount[i] == 1)
            {
                buttonPressCount[i]++;
            }

            if (handRays[i].hoveredObjects.Contains(interactor) != containsInteractor || buttonPressIntervals[i] <= 0)
            {
                buttonPressCount[i] = 0;
            }

            buttonPressIntervals[i] -= Time.deltaTime;
        }
    }

    private void SelectNotesText()
    {
        if (!isCurrentlyEdited)
        {
            if (firstTimeEdit)
            {
                firstTimeEdit = false;
                text.text = "";
            }

            for (int i = 0; i < buttonPressCount.Count; i++)
            {
                buttonPressCount[i] = 0;
            }

            isCurrentlyEdited = true;
            notes.notesText = text;
            notes.cantMoveText = cantMoveText;
            notes.notesCenterPos = originalPos;
            SpeechToText.instance.StartRecording();
            stickyNote.SetAsLastChild();
        }
    }

    private void OnDisable()
    {
        if (notes.notesText == text)
        {
            notes.notesText = null;
        }

        isCurrentlyEdited = false;
        SpeechToText.instance.StopRecording();

        if (text.text.EndsWith("|"))
        {
            text.text = text.text.Remove(text.text.Length - 1);
        }
    }

    private void SetObjectToBeingHovered()
    {
        isBeingHovered = true;
    }

    private void SetObjectToBeingUnHovered()
    {
        isBeingHovered = false;
    }
}
