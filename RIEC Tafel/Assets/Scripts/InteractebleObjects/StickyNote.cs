using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class StickyNote : MonoBehaviour
{
    [SerializeField]
    private InputDeviceCharacteristics leftCharacteristics = 0, rightCharacteristics = 0;

    [System.NonSerialized]
    public List<InputDevice> inputDevices = new List<InputDevice>();

    [System.NonSerialized]
    public List<PlayerHandRays> handRays = new List<PlayerHandRays>();

    private List<PlayerHandsRayInteractor> interactors = new List<PlayerHandsRayInteractor>();

    private List<Vector3> lastHandPosses = new List<Vector3>();

    [System.NonSerialized]
    public List<NotesText> texts = new List<NotesText>();

    private bool isBeingHovered = false, moveStickyNote = false;

    private RectTransform rectTransform = null;

    private List<StickyNote> allStickyNotes = new List<StickyNote>();

    /// <summary>
    /// Initialize all variables not set throught serialize field
    /// </summary>

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        inputDevices = InitializeControllers.InitializeControllersBasedOnHandRays(inputDevices, handRays, rightCharacteristics, leftCharacteristics);

        handRays.AddRange(FindObjectsOfType<PlayerHandRays>());
        for (int i = 0; i < handRays.Count; i++)
        {
            lastHandPosses.Add(handRays[i].transform.localPosition);
        }

        interactors.AddRange(GetComponentsInChildren<PlayerHandsRayInteractor>());
        for (int i = 0; i < interactors.Count; i++)
        {
            interactors[i].objectHoverEnteredEvent.AddListener(SetObjectToBeingHovered);
            interactors[i].objectHoverExitedEvent.AddListener(SetObjectToBeingUnHovered);
        }

        texts.AddRange(GetComponentsInChildren<NotesText>());
    }

    /// <summary>
    /// Check if the stickynote is being hovered. Move the stickynote if the primary button is down.
    /// </summary>

    private void Update()
    {
        if (inputDevices.Count < 2)
        {
            inputDevices.Clear();
            inputDevices = InitializeControllers.InitializeControllersBasedOnHandRays(inputDevices, handRays, rightCharacteristics, 
                leftCharacteristics);
            return;
        }

        if (isBeingHovered)
        {
            // Check for both the title and the body
            for (int i = 0; i < interactors.Count; i++)
            {
                for (int j = 0; j < inputDevices.Count; j++)
                {
                    // Check for input of the primary button
                    if (inputDevices[j].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton &&
                        handRays[j].hoveredObjects.Contains(interactors[i]))
                    {
                        if (!moveStickyNote)
                        {
                            moveStickyNote = true;
                            SetAsLastChild();
                            // Update the sticky notes list
                            allStickyNotes.Clear();
                            allStickyNotes.AddRange(FindObjectsOfType<StickyNote>());
                            EnableStickyNotesColliders(false);
                        }

                        // Movement calculation
                        Vector3 currentPos = handRays[j].transform.localPosition;
                        Vector3 movement = Vector3.zero;
                        movement.y = currentPos.y - lastHandPosses[j].y;
                        movement.x = currentPos.x - lastHandPosses[j].x;
                        rectTransform.position += movement * 13f;
                    }
                }
            }
        } else if (!isBeingHovered && moveStickyNote)
        {
            // Set everything to normal after moving the stickynote
            moveStickyNote = false;
            EnableStickyNotesColliders(true);
        }

        // Update last posses as to correctly calculate movement
        for (int i = 0; i < lastHandPosses.Count; i++)
        {
            lastHandPosses[i] = handRays[i].transform.localPosition;
        }
    }

    /// <summary>
    /// Turn all colliders of other stickynotes on/ off (turn off so that the user cant accidentally grab another stickynote to move.)
    /// </summary>

    private void EnableStickyNotesColliders(bool enabled)
    {
        for (int i = 0; i < allStickyNotes.Count; i++)
        {
            if (allStickyNotes[i] == this) { continue; }

            if (allStickyNotes[i])
            {
                foreach (BoxCollider collider in allStickyNotes[i].GetComponentsInChildren<BoxCollider>())
                {
                    collider.enabled = enabled;
                }
            }
        }
    }

    /// <summary>
    /// Check is the stickynote is currently being edited
    /// </summary>

    public bool GetStickyNoteBeingEdited()
    {
        for (int i = 0; i < texts.Count; i++)
        {
            if (texts[i].isCurrentlyEdited)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Set object to being hovered if player aims at the stickynote
    /// </summary>

    private void SetObjectToBeingHovered()
    {
        isBeingHovered = true;
    }

    /// <summary>
    /// Check if the title and body of sticky note are not hovered, if so set object to not being hovered
    /// </summary>

    private void SetObjectToBeingUnHovered()
    {
        int notHoveredCounter = 0;

        for (int i = 0; i < interactors.Count; i++)
        {
            for (int j = 0; j < handRays.Count; j++)
            {
                if (!handRays[j].hoveredObjects.Contains(interactors[i]))
                {
                    notHoveredCounter++;
                }
            }
        }

        if (notHoveredCounter == interactors.Count * handRays.Count)
        {
            isBeingHovered = false;
        }
    }

    /// <summary>
    /// Set as last child to appear in front of all other stickynotes
    /// </summary>

    public void SetAsLastChild()
    {
        transform.SetAsLastSibling();
    }
}
