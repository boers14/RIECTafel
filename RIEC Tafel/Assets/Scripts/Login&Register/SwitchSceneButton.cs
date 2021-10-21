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
