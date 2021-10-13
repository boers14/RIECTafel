using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class KeyBoardKey : MonoBehaviour
{
    [System.NonSerialized]
    public string textForKey = "";

    public int keySize = 1;

    [SerializeField]
    private bool removeText = false;

    [System.NonSerialized]
    public EditebleText currentText = null;

    [System.NonSerialized]
    public KeyBoard keyBoard = null;

    private new MeshRenderer renderer = null;

    private Color32 white = Color.white, gray = Color.gray;

    [System.NonSerialized]
    public bool isHovered = false;

    [System.NonSerialized]
    public List<XRNode> targetedNodes = new List<XRNode>();

    public virtual void Start()
    {
        GetComponentInChildren<TMP_Text>().text = textForKey;
        GetComponent<XRGrabInteractable>().hoverEntered.AddListener(OnHoverEnter);
        GetComponent<XRGrabInteractable>().hoverExited.AddListener(OnHoverLeave);
        keyBoard = GetComponentInParent<KeyBoard>();
        renderer = GetComponent<MeshRenderer>();
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        targetedNodes.Add(args.interactor.GetComponent<XRController>().controllerNode);
        renderer.material.color = gray;
        isHovered = true;
    }

    private void OnHoverLeave(HoverExitEventArgs args)
    {
        targetedNodes.Remove(args.interactor.GetComponent<XRController>().controllerNode);
        if (targetedNodes.Count == 0)
        {
            isHovered = false;
            renderer.material.color = white;
        }
    }

    public void OnKeySelect()
    {
        if (!currentText)
        {
            FetchCurrentlyEditedText();
        }

        if (!currentText.isCurrentlyEdited)
        {
            FetchCurrentlyEditedText();
        }

        KeyFunction();
    }

    public virtual void KeyFunction()
    {
        currentText.EditText(textForKey, removeText, keyBoard);
    }

    private void FetchCurrentlyEditedText()
    {
        EditebleText[] editebleTexts = FindObjectsOfType<EditebleText>();
        for (int i = 0; i < editebleTexts.Length; i++)
        {
            if (editebleTexts[i].isCurrentlyEdited)
            {
                currentText = editebleTexts[i];
            }
        }
    }
}
