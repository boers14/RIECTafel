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

    private List<Collider> hands = new List<Collider>();

    /// <summary>
    /// Set required variables
    /// </summary>

    public virtual void Start()
    {
        GetComponentInChildren<TMP_Text>().text = textForKey;
        GetComponent<XRGrabInteractable>().hoverEntered.AddListener(OnHoverEnter);
        GetComponent<XRGrabInteractable>().hoverExited.AddListener(OnHoverLeave);
        keyBoard = GetComponentInParent<KeyBoard>();
        renderer = GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// If the keyboard is touchkeyboard the keyboard key is being pressed
    /// </summary>

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Hand" || hands.Contains(other) || keyBoard.keyBoardIsHovered || SettingsManager.rayKeyBoard) { return; }

        keyBoard.keyBoardIsHovered = true;
        hands.Add(other);
        renderer.material.color = gray;
        OnKeySelect();
    }

    /// <summary>
    /// If the keyboard is touchkeyboard the keyboard key is unpressed
    /// </summary>

    private void OnTriggerExit(Collider other)
    {
        if (!hands.Contains(other) || SettingsManager.rayKeyBoard) { return; }

        hands.Remove(other);
        if (hands.Count <= 0)
        {
            keyBoard.keyBoardIsHovered = false;
            renderer.material.color = white;
        }
    }

    /// <summary>
    /// Set the key to targeted if the keyboard is a ray keyboard
    /// </summary>

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (!SettingsManager.rayKeyBoard) { return; }

        // For knowing which hand is targeting the key
        targetedNodes.Add(args.interactor.GetComponent<XRController>().controllerNode);
        renderer.material.color = gray;
        isHovered = true;
    }

    /// <summary>
    /// Untarged the keyboard if the keyboard is a rayboard
    /// </summary>

    private void OnHoverLeave(HoverExitEventArgs args)
    {
        if (!SettingsManager.rayKeyBoard) { return; }

        targetedNodes.Remove(args.interactor.GetComponent<XRController>().controllerNode);
        if (targetedNodes.Count == 0)
        {
            isHovered = false;
            renderer.material.color = white;
        }
    }

    /// <summary>
    /// For when the key is selected, fetch the text that is edited and perform the keyfunction
    /// </summary>

    public void OnKeySelect()
    {
        if (!currentText || !currentText.gameObject.activeSelf)
        {
            currentText = FetchCurrentlyEditedText();
        }

        if (currentText)
        {
            if (!currentText.isCurrentlyEdited)
            {
                currentText = FetchCurrentlyEditedText();
            }

            if (currentText)
            {
                KeyFunction();
            }
        }
    }

    /// <summary>
    /// Reset key if its disabled
    /// </summary>

    private void OnDisable()
    {
        hands.Clear();
        if (renderer)
        {
            renderer.material.color = white;
        }
    }

    /// <summary>
    /// Edit the currently edited text. Set to virtual if key does something different.
    /// </summary>

    public virtual void KeyFunction()
    {
        currentText.EditText(textForKey, removeText, keyBoard);
    }

    /// <summary>
    /// Grab the text that is currently being edited if there is one
    /// </summary>

    private EditebleText FetchCurrentlyEditedText()
    {
        EditebleText[] editebleTexts = FindObjectsOfType<EditebleText>();

        for (int i = 0; i < editebleTexts.Length; i++)
        {
            if (editebleTexts[i].isCurrentlyEdited)
            {
                return editebleTexts[i];
            }
        }

        return null;
    }
}
