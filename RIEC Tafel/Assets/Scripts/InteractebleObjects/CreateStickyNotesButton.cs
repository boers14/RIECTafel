using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateStickyNotesButton : MonoBehaviour
{
    [SerializeField]
    private Transform bgImage = null;

    [SerializeField]
    private StickyNote stickyNote = null;

    private Vector3 stickyNoteScale = Vector3.zero;

    public virtual void Start()
    {
        stickyNoteScale = stickyNote.transform.localScale;
        GetComponent<Button>().onClick.AddListener(StartCreateNewStickyNote);
    }

    public virtual void StartCreateNewStickyNote()
    {
        StickyNote newStickyNote = CreateNewStickyNote();

        if (NotesSaver.instance)
        {
            NotesSaver.instance.allStickyNotes.Add(newStickyNote);
        }
    }

    public StickyNote CreateNewStickyNote()
    {
        StickyNote newStickyNote = Instantiate(stickyNote);
        newStickyNote.transform.SetParent(bgImage);
        newStickyNote.transform.localPosition = Vector3.zero;
        newStickyNote.transform.localScale = stickyNoteScale;
        newStickyNote.transform.localEulerAngles = Vector3.zero;

        return newStickyNote;
    }
}
