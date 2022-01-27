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

    /// <summary>
    /// Permission of microphone is needed if this program wants to use the microphone (does not yet use it)
    /// </summary>

    public override void Start()
    {
        base.Start();

        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
    }

    /// <summary>
    /// Dont update if the user is planning to use the notes dropdown
    /// </summary>

    public override void Update()
    {
        if (notesSelectionDropdown.transform.childCount == 4 || notesSelectionDropdownIsHovered) { return; }
        base.Update();
    }

    /// <summary>
    /// Set the hovered state of the notes dropdown (set in editor on player hand rays interactor)
    /// </summary>

    public void SetNoteDropdownToBeingHovered()
    {
        notesSelectionDropdownIsHovered = true;
    }

    public void SetNoteDropdownToNotBeingHovered()
    {
        notesSelectionDropdownIsHovered = false;
    }
}
