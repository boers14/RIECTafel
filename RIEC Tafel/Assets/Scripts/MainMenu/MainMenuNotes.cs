using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Android;

public class MainMenuNotes : Notes
{
    private bool notesSelectionDropdownIsHovered = false;

    [SerializeField]
    private TMP_Dropdown notesSelectionDropdown = null;

    public override void Start()
    {
        base.Start();

        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
    }

    public override void Update()
    {
        if (notesSelectionDropdown.transform.childCount == 4 || notesSelectionDropdownIsHovered) { return; }
        base.Update();
    }

    public void SetNoteDropdownToBeingHovered()
    {
        notesSelectionDropdownIsHovered = true;
    }

    public void SetNoteDropdownToNotBeingHovered()
    {
        notesSelectionDropdownIsHovered = false;
    }
}
