using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditebleText : MonoBehaviour
{
    [SerializeField]
    private bool showEditPlace = true;

    [System.NonSerialized]
    public bool isCurrentlyEdited = false;

    private float blinkTimer = 0, blinkSwitch = 0.5f;

    private float prevYSize = 0;

    private RectTransform rectTransform = null;

    private bool isInputField = false, changeYPos = true;

    /// <summary>
    /// Check if the edited text is an input field or regular text
    /// </summary>

    public virtual void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (GetComponent<TMP_InputField>())
        {
            isInputField = true;
            changeYPos = false;
        }
    }

    /// <summary>
    /// Edit y position if textblock changes in size and show the place where the text is edited
    /// </summary>

    private void FixedUpdate()
    {
        if (rectTransform.sizeDelta.y != prevYSize && changeYPos)
        {
            float difInSize = rectTransform.sizeDelta.y - prevYSize;
            Vector3 newPos = rectTransform.localPosition;
            if (difInSize > 0)
            {
                newPos.y -= difInSize / 2;
            }
            else
            {
                newPos.y += difInSize / 2 * -1;
            }
            rectTransform.localPosition = newPos;
            prevYSize = rectTransform.sizeDelta.y;
        }

        // only show where the edit place if showEditPlace
        if (!isCurrentlyEdited || !showEditPlace) { return; }

        blinkTimer += Time.fixedDeltaTime;
        if (blinkTimer >= blinkSwitch)
        {
            blinkTimer = 0;
            string text = GetText();

            if (text.EndsWith("|"))
            {
                text = text.Remove(text.Length - 1);
            } else
            {
                text += "|";
            }

            SetText(text);
        }
    }

    /// <summary>
    /// Set all texts to not being edited and turn it on for this
    /// </summary>

    public void SetAsEditeble()
    {
        EditebleText[] editebleTexts = FindObjectsOfType<EditebleText>();
        for (int i = 0; i < editebleTexts.Length; i++)
        {
            editebleTexts[i].isCurrentlyEdited = false;
            editebleTexts[i].blinkTimer = 0;
        }

        isCurrentlyEdited = true;
    }

    /// <summary>
    /// Add the addedText to the current text, if remove text, remove the last symbol instead
    /// </summary>

    public void EditText(string addedText, bool removeText, KeyBoard keyBoard)
    {
        string text = GetText();

        if (!removeText)
        {
            if (text.EndsWith("|"))
            {
                text = text.Remove(text.Length - 1);
                text += addedText + "|";
            }
            else
            {
                text += addedText;
            }
        }
        else
        {
            if (text.EndsWith("|"))
            {
                if (text.Length > 1)
                {
                    text = text.Remove(text.Length - 2);
                }
            } else
            {
                if (text.Length > 0)
                {
                    text = text.Remove(text.Length - 1);
                }
            }
        }

        SetText(text);
        // Swap the keyboard to the normal state if it was in capslock
        if (keyBoard.shiftState)
        {
            keyBoard.SwapKeyBoardState();
        }
    }

    /// <summary>
    /// Get the text component depending on what type of editeble text it is
    /// </summary>

    private string GetText()
    {
        if (isInputField)
        {
            return GetComponent<TMP_InputField>().text;
        } else
        {
            return GetComponent<TMP_Text>().text;
        }
    }

    /// <summary>
    /// Set the text depending on what type of editeble text it is
    /// </summary>

    private void SetText(string text)
    {
        if (isInputField)
        {
            GetComponent<TMP_InputField>().text = text;
        }
        else
        {
            GetComponent<TMP_Text>().text = text;
        }
    }
}
