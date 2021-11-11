using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHands : MonoBehaviour
{
    [SerializeField]
    private TutorialControllerArrows primaryButtonArrow = null, secondaryButtonArrow = null, steerStickArrow = null, triggerArrow = null,
        gripArrow = null;

    private List<TutorialControllerArrows> allArrows = new List<TutorialControllerArrows>();

    private TutorialManager tutorialManager = null;

    public enum HandCharacteristic
    {
        Left,
        Right
    }

    public HandCharacteristic handCharacteristic = 0;

    private void Start()
    {
        tutorialManager = FindObjectOfType<TutorialManager>();
        tutorialManager.AddTutorialHandToList(this);
        allArrows.AddRange(new TutorialControllerArrows[] { primaryButtonArrow, secondaryButtonArrow, steerStickArrow, triggerArrow, gripArrow });
    }

    public void ClearAllArrows()
    {
        for (int i = 0; i < allArrows.Count; i++)
        {
            allArrows[i].gameObject.SetActive(false);
        }
    }

    public void EnablePrimaryButtonArrow(bool enabled)
    {
        primaryButtonArrow.gameObject.SetActive(enabled);
    }

    public void EnableSecondaryButtonArrow(bool enabled)
    {
        secondaryButtonArrow.gameObject.SetActive(enabled);
    }

    public void EnableSteerStickArrow(bool enabled)
    {
        steerStickArrow.gameObject.SetActive(enabled);
    }

    public void EnableTriggerArrow(bool enabled)
    {
        triggerArrow.gameObject.SetActive(enabled);
    }

    public void EnableGripArrow(bool enabled)
    {
        gripArrow.gameObject.SetActive(enabled);
    }
}
