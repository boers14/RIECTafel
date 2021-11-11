using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OpenTutorialOptions : MonoBehaviour
{
    [SerializeField]
    private GameObject tutorialButtonSet = null;

    private void Start()
    {
        TutorialSceneManager.sceneToSwitchTo = SceneManager.GetActiveScene().name;
        GetComponent<Button>().onClick.AddListener(EnableButtonSet);
    }

    private void EnableButtonSet()
    {
        tutorialButtonSet.SetActive(!tutorialButtonSet.activeSelf);
    }
}
