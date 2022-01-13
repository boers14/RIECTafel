using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRemoveStickyNoteButton : RemoveStickyNoteButton
{
    private TutorialManager tutorialManager = null;

    public override void Start()
    {
        base.Start();
        tutorialManager = FindObjectOfType<TutorialManager>();
    }

    public override void RemoveStickyNote()
    {
        StickyNote stickyNoteToRemove = tutorialManager.stickyNotes.Find(stickyNote => stickyNote.GetStickyNoteBeingEdited());
        if (stickyNoteToRemove != null)
        {
            tutorialManager.stickyNotes.Remove(stickyNoteToRemove);
            Destroy(stickyNoteToRemove.gameObject);
        }
    }
}
