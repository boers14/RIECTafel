using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OpenTutorialOptions : MonoBehaviour
{
    [SerializeField]
    private GameObject tutorialButtonSet = null;

    /// <summary>
    /// Make sure after tutorial that player returns to main menu
    /// </summary>

    private void Start()
    {
        TutorialSceneManager.sceneToSwitchTo = SceneManager.GetActiveScene().name;
        GetComponent<Button>().onClick.AddListener(EnableButtonSet);
    }

    /// <summary>
    /// Set active states of tutorial buttons
    /// </summary>

    private void EnableButtonSet()
    {
        tutorialButtonSet.SetActive(!tutorialButtonSet.activeSelf);
    }
}
