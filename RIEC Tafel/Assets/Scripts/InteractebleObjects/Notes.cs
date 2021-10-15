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

    [SerializeField]
    private EditebleText notesEditText = null;

    [SerializeField]
    private TMP_Text notesTitle = null;

    private TMP_Text notesText = null;

    private Vector3 notesTitlePos = Vector3.zero, notesCenterPos = Vector3.zero;

    private float originalYPosOfExplanationText = 0, oldYSize = 0, changeFontSizeTimer = 0;

    public override void Start()
    {
        base.Start();
        notesText = notesEditText.GetComponent<TMP_Text>();
        notesTitlePos = notesTitle.rectTransform.position;
        notesCenterPos = notesText.rectTransform.localPosition;
    }

    public override void Update()
    {
        changeFontSizeTimer -= Time.deltaTime;
        base.Update();
    }

    public override void MoveImage(Vector2 steerStickInput, GameObject image, Vector3 originalPosition, float extraYMovement, bool nullifyMovement)
    {
        if (steerStickInput.x < 0.1f && steerStickInput.y < 0.1f && steerStickInput.x > -0.1f && steerStickInput.y > -0.1f || 
            notesText.textInfo == null) { return; }

        if (nullifyMovement)
        {
            steerStickInput = Vector2.zero;
        }

        Vector3 newPos = notesText.rectTransform.localPosition;
        float ySize = (float)notesText.textInfo.lineCount * (notesText.fontSize * 1.15f);

        newPos.y += -steerStickInput.y * movementPower * notesText.rectTransform.localScale.x;
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

    public override void ChangeImageScale(float scalePower, GameObject image, Vector3 vector3, float extraYMovement)
    {
        if (scalePower > 0)
        {
            scalePower = 1;
        }
        else
        {
            scalePower = -1;
        }

        if (changeFontSizeTimer > 0 || notesText.fontSize == maximumScale && scalePower > 0 ||
            notesText.fontSize == minimumScale && scalePower < 0 || keyBoard.keyBoardIsHovered) { return; }
        changeFontSizeTimer = 0.3f;
        oldYSize = (float)notesText.textInfo.lineCount * (notesText.fontSize * 1.15f);
        notesText.fontSize += (int)scalePower;
        MoveImage(Vector3.one, notesText.gameObject, notesCenterPos, 0, true);
    }

    public override void OnGrabEnter(SelectEnterEventArgs selectEnterEventArgs, bool setOriginalVectors)
    {
        base.OnGrabEnter(selectEnterEventArgs, setOriginalVectors);
        keyBoard.EnableKeyBoard(true);
        notesSet.SetActive(true);
        notesEditText.SetAsEditeble();
        notesSet.transform.SetAsLastSibling();
    }

    public override void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
    {
        base.OnSelectExit(selectExitEventArgs);
        keyBoard.EnableKeyBoard(false);
        notesSet.SetActive(false);
        notesEditText.isCurrentlyEdited = false;
    }
}
