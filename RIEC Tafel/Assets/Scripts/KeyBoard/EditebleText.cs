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
    public bool isCurrentlyEdited = true;

    private TMP_Text text = null;

    private float blinkTimer = 0, blinkSwitch = 0.5f;

    private float prevYSize = 0;

    private void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    private void FixedUpdate()
    {
        if (text.rectTransform.sizeDelta.y != prevYSize)
        {
            float difInSize = text.rectTransform.sizeDelta.y - prevYSize;
            Vector3 newPos = text.rectTransform.localPosition;
            if (difInSize > 0)
            {
                newPos.y -= difInSize / 2;
            } else
            {
                newPos.y += difInSize / 2 * -1;
            }
            text.rectTransform.localPosition = newPos;
            prevYSize = text.rectTransform.sizeDelta.y;
        } 

        if (!isCurrentlyEdited || !showEditPlace) { return; }

        blinkTimer += Time.fixedDeltaTime;
        if (blinkTimer >= blinkSwitch)
        {
            blinkTimer = 0;
            if (text.text.EndsWith("|"))
            {
                text.text = text.text.Remove(text.text.Length - 1);
            } else
            {
                text.text += "|";
            }
        }
    }

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

    public void EditText(string addedText, bool removeText, KeyBoard keyBoard)
    {
        if (!removeText)
        {
            if (text.text.EndsWith("|"))
            {
                text.text = text.text.Remove(text.text.Length - 1);
                text.text += addedText + "|";
            }
            else
            {
                text.text += addedText;
            }
        }
        else
        {
            if (text.text.EndsWith("|"))
            {
                if (text.text.Length > 1)
                {
                    text.text = text.text.Remove(text.text.Length - 2);
                }
            } else
            {
                if (text.text.Length > 0)
                {
                    text.text = text.text.Remove(text.text.Length - 1);
                }
            }
        }

        if (keyBoard.shiftState)
        {
            keyBoard.SwapKeyBoardState();
        }
    }
}
