using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoveStickyNoteButton : MeetingButton
{
    [SerializeField]
    private Button yesButton = null;

    public override void Start()
    {
        base.Start();
        yesButton.onClick.AddListener(RemoveStickyNote);
    }

    public virtual void RemoveStickyNote()
    {
        StickyNote stickyNoteToRemove = NotesSaver.instance.allStickyNotes.Find(stickyNote => stickyNote.GetStickyNoteBeingEdited());
        if (stickyNoteToRemove != null)
        {
            NotesSaver.instance.allStickyNotes.Remove(stickyNoteToRemove);
            Destroy(stickyNoteToRemove.gameObject);
        }
    }
}
