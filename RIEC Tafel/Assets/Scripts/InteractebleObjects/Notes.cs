using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class Notes : GrabbebleObjects
{
    [SerializeField]
    private KeyBoard keyBoard = null;

    [SerializeField]
    private GameObject notesSet = null;

    public TMP_Text notesText = null;

    [System.NonSerialized]
    public Vector3 notesCenterPos = Vector3.zero;

    private float changeFontSizeTimer = 0;

    [System.NonSerialized]
    public bool cantMoveText = false;

    /// <summary>
    /// Set notes center pos if possible
    /// </summary>

    public override void Start()
    {
        base.Start();
        if (notesText)
        {
            notesCenterPos = notesText.rectTransform.localPosition;
        }
    }

    /// <summary>
    /// Perform standard update and count change font size timer down
    /// </summary>

    public override void Update()
    {
        if (!notesText) { return; }
        changeFontSizeTimer -= Time.deltaTime;
        base.Update();
    }

    /// <summary>
    /// Move the notesText over the y axis based on input.
    /// </summary>

    public override void MoveImage(Vector2 steerStickInput, GameObject image, Vector3 originalPosition, float extraYMovement, bool nullifyMovement)
    {
        if (steerStickInput.x < 0.1f && steerStickInput.y < 0.1f && steerStickInput.x > -0.1f && steerStickInput.y > -0.1f || 
            notesText.textInfo == null || cantMoveText) { return; }

        if (nullifyMovement)
        {
            steerStickInput = Vector2.zero;
        }

        steerStickInput *= SettingsManager.moveGrabbedObjectSpeedFactor;

        Vector3 newPos = notesText.rectTransform.localPosition;
        float ySize = (float)notesText.textInfo.lineCount * (notesText.fontSize * 1.15f);

        newPos.y += -steerStickInput.y * movementPower * notesText.rectTransform.localScale.x;
        // Make sure notesText doesnt move past boundries
        if (newPos.y > notesCenterPos.y + ySize)
        {
            newPos.y = notesCenterPos.y + ySize;
        }
        else if (newPos.y < -notesCenterPos.y)
        {
            newPos.y = -notesCenterPos.y;
        }

        notesText.rectTransform.localPosition = newPos;
    }

    /// <summary>
    /// Change fontsize of notesText, then check if pos didnt move past the bounderies
    /// </summary>

    public override void ChangeImageScale(float scalePower, GameObject image, Vector3 vector3, float extraYMovement)
    {
        if (keyBoard)
        {
            if (keyBoard.keyBoardIsHovered)
            {
                return;
            }
        }

        if (scalePower > 0)
        {
            scalePower = 1;
        }
        else
        {
            scalePower = -1;
        }

        if (changeFontSizeTimer > 0 || notesText.fontSize == maximumScale && scalePower > 0 ||
            notesText.fontSize == minimumScale && scalePower < 0 || cantMoveText) { return; }
        changeFontSizeTimer = 0.3f;
        notesText.fontSize += (int)scalePower;
        MoveImage(Vector3.one, notesText.gameObject, notesCenterPos, 0, true);
    }

    /// <summary>
    /// Enable all required objects for creating notes
    /// </summary>

    public override void OnGrabEnter(SelectEnterEventArgs selectEnterEventArgs, bool setOriginalVectors)
    {
        base.OnGrabEnter(selectEnterEventArgs, setOriginalVectors);
        EnableNotesObjects(true);
        notesSet.transform.SetAsLastSibling();
    }

    /// <summary>
    /// Disable required object for creating notes
    /// </summary>

    public override void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
    {
        base.OnSelectExit(selectExitEventArgs);
        EnableNotesObjects(false);
    }

    /// <summary>
    /// Enable notes objects
    /// </summary>

    private void EnableNotesObjects(bool enabled)
    {
        keyBoard.EnableKeyBoard(enabled);
        notesSet.SetActive(enabled);
    }
}
