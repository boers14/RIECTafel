using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoveStickyNoteButton : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(RemoveStickyNote);
    }

    private void RemoveStickyNote()
    {
        StickyNote stickyNoteToRemove = NotesSaver.instance.allStickyNotes.Find(stickyNote => stickyNote.GetStickyNoteBeingEdited());
        if (stickyNoteToRemove != null)
        {
            NotesSaver.instance.allStickyNotes.Remove(stickyNoteToRemove);
            Destroy(stickyNoteToRemove.gameObject);
        }
    }
}
