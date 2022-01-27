using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCreateStickyNoteButton : CreateStickyNotesButton
{
    private TutorialManager tutorialManager = null;

    /// <summary>
    /// Updates the stickynotes count and adds it to the list in the tutorial manager
    /// </summary>

    public override void Start()
    {
        base.Start();
        tutorialManager = FindObjectOfType<TutorialManager>();
    }

    public override void StartCreateNewStickyNote()
    {
        StickyNote newStickyNote = CreateNewStickyNote();
        tutorialManager.stickyNotes.Add(newStickyNote);
        tutorialManager.stickyNoteCount = tutorialManager.stickyNotes.Count;
    }
}
