using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class KeyBoard : MonoBehaviour
{
    [SerializeField]
    private KeyBoardKey key = null;

    [SerializeField]
    private List<string> keyBoardKeysInString = new List<string>(), shiftKeyBoardKeysInString = new List<string>(), specialKeys = new List<string>();

    [SerializeField]
    private List<KeyBoardKey> specialKeyBoardKeys = new List<KeyBoardKey>();

    private List<KeyBoardKey> normalKeyBoardKeys = new List<KeyBoardKey>(), shiftKeyBoardKeys = new List<KeyBoardKey>();

    [SerializeField]
    private List<int> keysPerRow = new List<int>();

    [System.NonSerialized]
    public bool shiftState = true, keyBoardIsHovered = false;

    public bool pushKeyBoard = false;

    private List<InputDevice> inputDevices = new List<InputDevice>();

    private List<bool> usedInputDevices = new List<bool>();

    private InputDeviceCharacteristics rightCharacteristics = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right,
        leftCharacteristics = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;

    private InputDevice inputDevice;

    private Vector3 pushKeyboardPos = new Vector3(0, 0.8f, 0.1f), originalPosition = Vector3.zero, originalScale = Vector3.zero;

    private void Start()
    {
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;

        float xScale = key.transform.localScale.x * 10;
        float zScale = key.transform.localScale.z * 10;

        CreateKeyBoard(xScale, zScale, keyBoardKeysInString, normalKeyBoardKeys);
        CreateKeyBoard(xScale, zScale, shiftKeyBoardKeysInString, shiftKeyBoardKeys);
        SwapKeyBoardState();

        AddInputDevices();
        for (int i = 0; i < 2; i++)
        {
            usedInputDevices.Add(false);
        }

        if (pushKeyBoard)
        {
            transform.localScale *= 0.5f;
            pushKeyboardPos.x = transform.localPosition.x / 2;
            transform.localPosition = pushKeyboardPos;
        }
    }

    private void FixedUpdate()
    {
        if (pushKeyBoard) { return; }

        if (inputDevices.Count < 2)
        {
            AddInputDevices();
        }

        if (shiftState)
        {
            CheckIfKeyboardIsHovered(shiftKeyBoardKeys);
        } else
        {
            CheckIfKeyboardIsHovered(normalKeyBoardKeys);
        }

        for (int i = 0; i < inputDevices.Count; i++)
        {
            if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool button) && button && !usedInputDevices[i])
            {
                usedInputDevices[i] = true;
                KeyBoardKey selectedKey = null;

                if (shiftState)
                {
                    selectedKey = GetSelectedKey(selectedKey, shiftKeyBoardKeys, i);
                } else
                {
                    selectedKey = GetSelectedKey(selectedKey, normalKeyBoardKeys, i);
                }

                if (selectedKey)
                {
                    selectedKey.OnKeySelect();
                }
            }
            else if (!button)
            {
                usedInputDevices[i] = false;
            }
        }
    }

    private void CheckIfKeyboardIsHovered(List<KeyBoardKey> keyBoardKeys)
    {
        bool isHovered = false;
        for (int i = 0; i < keyBoardKeys.Count; i++)
        {
            if (keyBoardKeys[i].isHovered)
            {
                isHovered = true;
            }
        }

        keyBoardIsHovered = isHovered;
    }

    private KeyBoardKey GetSelectedKey(KeyBoardKey selectedKey, List<KeyBoardKey> keyBoardKeys, int index)
    {
        if (index == 0)
        {
            selectedKey = keyBoardKeys.Find(i => i.isHovered == true && i.targetedNodes.Contains(XRNode.RightHand));
        }
        else
        {
            selectedKey = keyBoardKeys.Find(i => i.isHovered == true && i.targetedNodes.Contains(XRNode.LeftHand));
        }

        return selectedKey;
    }

    private void AddInputDevices()
    {
        inputDevices.Clear();
        AddInputDevice(rightCharacteristics);
        AddInputDevice(leftCharacteristics);
    }

    private void AddInputDevice(InputDeviceCharacteristics characteristics)
    {
        inputDevice = InitializeControllers.ReturnInputDeviceBasedOnCharacteristics(characteristics, inputDevice);

        if (inputDevice.isValid)
        {
            inputDevices.Add(inputDevice);
        }
    }

    private void CreateKeyBoard(float xScale, float zScale, List<string> keyBoardKeysInString, List<KeyBoardKey> keyBoardKeys)
    {
        float startXPos = keysPerRow[0] / 2 * xScale - xScale / 2;
        float startZPos = Mathf.Ceil(keyBoardKeysInString.Count / keysPerRow.Count) / 2 * zScale + zScale / 2;

        int rowCounter = 0;
        int placeInRow = 0;

        for (int i = 1; i < keyBoardKeysInString.Count + 1; i++)
        {
            if (rowCounter >= keysPerRow.Count) { break; }
            int actualIndex = i - 1;

            KeyBoardKey newKey = null;
            if (specialKeys.Contains(keyBoardKeysInString[actualIndex]))
            {
                newKey = Instantiate(specialKeyBoardKeys[specialKeys.IndexOf(keyBoardKeysInString[actualIndex])], transform.position, 
                    transform.rotation);
            }
            else
            {
                newKey = Instantiate(key, transform.position, transform.rotation);
            }

            newKey.transform.SetParent(transform);

            Vector3 pos = Vector3.zero;
            pos.x = -startXPos + xScale * placeInRow + (xScale * (((float)newKey.keySize - 1) / 2));
            pos.z = startZPos - zScale * rowCounter;
            newKey.transform.localPosition = pos;

            newKey.textForKey = keyBoardKeysInString[actualIndex];
            placeInRow += newKey.keySize;
            keyBoardKeys.Add(newKey);

            if (placeInRow >= keysPerRow[rowCounter])
            {
                rowCounter++;
                if (rowCounter < keysPerRow.Count)
                {
                    startXPos = keysPerRow[rowCounter] / 2 * xScale - xScale / 2;
                }
                placeInRow = 0;
            }
        }
    }

    public void SwapKeyBoardState()
    {
        shiftState = !shiftState;
        if (shiftState)
        {
            EnableKeyBoardState(shiftKeyBoardKeys, normalKeyBoardKeys);
        } else
        {
            EnableKeyBoardState(normalKeyBoardKeys, shiftKeyBoardKeys);
        }

        if (pushKeyBoard)
        {
            StartCoroutine(SwapHoverState());
        }
    }

    private IEnumerator SwapHoverState()
    {
        yield return new WaitForSeconds(0.05f);
        keyBoardIsHovered = false;
    }

    private void EnableKeyBoardState(List<KeyBoardKey> keyBoardKeysToEnable, List<KeyBoardKey> keyBoardKeysToDisable)
    {
        for (int  i = 0; i < keyBoardKeysToEnable.Count; i++)
        {
            keyBoardKeysToEnable[i].gameObject.SetActive(true);
            keyBoardKeysToDisable[i].gameObject.SetActive(false);
        }
    }

    public void EnableKeyBoard(bool enabled)
    {
        gameObject.SetActive(enabled);
    }
}
