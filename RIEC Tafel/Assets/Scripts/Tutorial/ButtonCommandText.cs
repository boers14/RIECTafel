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

    /// <summary>
    /// Initialize variables and start tweening
    /// </summary>

    private void Start()
    {
        tutorialManager = FindObjectOfType<TutorialManager>();
        text = GetComponent<TMP_Text>();

        startTweenColor = normalEndTweenColor;
        endTweenColor = normalStartTweenColor;

        StartNormalTween();
    }

    /// <summary>
    /// Tweens from a given start color to a given end color in the given time.
    /// Normally this is from a gold yellow color with an alpha 100% to the same color with 0% alpha
    /// </summary>

    private void StartNormalTween()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", textAlphaTweenTime, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateColor", "oncomplete", "SwitchTweenColor", "oncompletetarget", gameObject));
    }

    /// <summary>
    /// Is the update function of the tween
    /// Calculates the new color value based on the progress of the tween
    /// </summary>

    private void UpdateColor(float val)
    {
        Color32 newColor = text.color;
        newColor.r = (byte)(((1f - val) * startTweenColor.r) + (val * endTweenColor.r));
        newColor.g = (byte)(((1f - val) * startTweenColor.g) + (val * endTweenColor.g));
        newColor.b = (byte)(((1f - val) * startTweenColor.b) + (val * endTweenColor.b));
        newColor.a = (byte)(((1f - val) * startTweenColor.a) + (val * endTweenColor.a));
        text.color = newColor;
    }

    /// <summary>
    /// Is the on complete funtion of the tween
    /// If the tween coloring was different then the regular tween color, set it to the regular tween color
    /// </summary>

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
        // Loop the tween infinetly
        StartNormalTween();
    }

    /// <summary>
    /// Stop the current tween on the object, start the regular tween where the text colors to green but have the on complete function
    /// be different
    /// </summary>

    public void StartCelebrateStepDone()
    {
        iTween.Stop(gameObject);
        text.text = "Stap gehaald!";
        startTweenColor = text.color;
        endTweenColor = stepDoneColor;

        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", textAlphaTweenTime, "easetype", iTween.EaseType.easeInOutSine,
            "onupdate", "UpdateColor", "oncomplete", "CelebrateStepDone", "oncompletetarget", gameObject));
    }

    /// <summary>
    /// Start the coroutine of the next step
    /// </summary>

    private void CelebrateStepDone()
    {
        StartCoroutine(StartNextStep());
    }

    /// <summary>
    /// Play all the particle systems and start the normal tweens again
    /// </summary>

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
        // Start next step in tutorial at end celebration
        tutorialManager.StartNextStepInTutorial();
    }
}
