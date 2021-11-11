using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonCommandText : MonoBehaviour
{
    [SerializeField]
    private float textAlphaTweenTime = 1, stepCelebrationTime = 2;

    [SerializeField]
    private Color32 normalStartTweenColor = Color.yellow, normalEndTweenColor = Color.clear, stepDoneColor = Color.green;

    private Color32 startTweenColor = Color.yellow, endTweenColor = Color.clear;

    private TMP_Text text = null;

    private TutorialManager tutorialManager = null;

    [SerializeField]
    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    private bool setRegularTweenColor = false;

    private void Start()
    {
        tutorialManager = FindObjectOfType<TutorialManager>();
        text = GetComponent<TMP_Text>();

        startTweenColor = normalEndTweenColor;
        endTweenColor = normalStartTweenColor;

        StartNormalTween();
    }

    private void StartNormalTween()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", textAlphaTweenTime, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateColor", "oncomplete", "SwitchTweenColor", "oncompletetarget", gameObject));
    }

    private void UpdateColor(float val)
    {
        Color32 newColor = text.color;
        newColor.r = (byte)(((1f - val) * startTweenColor.r) + (val * endTweenColor.r));
        newColor.g = (byte)(((1f - val) * startTweenColor.g) + (val * endTweenColor.g));
        newColor.b = (byte)(((1f - val) * startTweenColor.b) + (val * endTweenColor.b));
        newColor.a = (byte)(((1f - val) * startTweenColor.a) + (val * endTweenColor.a));
        text.color = newColor;
    }

    private void SwitchTweenColor()
    {
        Color32 currentStartColor = startTweenColor;
        if (setRegularTweenColor)
        {
            currentStartColor = normalStartTweenColor;
            setRegularTweenColor = false;
        }
        startTweenColor = endTweenColor;
        endTweenColor = currentStartColor;
        StartNormalTween();
    }

    public void StartCelebrateStepDone()
    {
        iTween.Stop(gameObject);
        text.text = "Stap gehaald!";
        startTweenColor = text.color;
        endTweenColor = stepDoneColor;

        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", textAlphaTweenTime, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateColor", "oncomplete", "CelebrateStepDone", "oncompletetarget", gameObject));
    }

    private void CelebrateStepDone()
    {
        StartCoroutine(StartNextStep());
    }

    private IEnumerator StartNextStep()
    {
        for (int i = 0; i < particleSystems.Count; i++)
        {
            particleSystems[i].Play();
        }

        yield return new WaitForSeconds(stepCelebrationTime);

        startTweenColor = stepDoneColor;
        endTweenColor = normalEndTweenColor;
        setRegularTweenColor = true;
        StartNormalTween();
        tutorialManager.StartNextStepInTutorial();
    }
}
