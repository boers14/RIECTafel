using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SwitchSceneButton : MonoBehaviour
{
    public string sceneToSwitchTo = "";

    [System.NonSerialized]
    public Button button = null;

    /// <summary>
    /// Switch scene to given scene, set to virtual to perform extra needed functions
    /// </summary>

    public virtual void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SwitchScene);
    }

    public virtual void SwitchScene()
    {
        SceneManager.LoadScene(sceneToSwitchTo);
    }
}
