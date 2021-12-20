using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuNotes : Notes
{
    private bool notesSelectionDropdownIsHovered = false;

    [SerializeField]
    private TMP_Dropdown notesSelectionDropdown = null;

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
