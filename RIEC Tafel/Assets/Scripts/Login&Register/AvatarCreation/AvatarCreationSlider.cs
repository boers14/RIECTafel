using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarCreationSlider : MonoBehaviour
{
    private AvatarCreationManager avatarCreationManager = null;

    public AvatarCreationManager.TargetedBodyType targetedBodyType = 0;

    /// <summary>
    /// Change the avatar body size based on the value of the slider
    /// </summary>

    private void Start()
    {
        avatarCreationManager = FindObjectOfType<AvatarCreationManager>();
        GetComponent<Slider>().onValueChanged.AddListener(ChangeAvatarBodyType);
    }

    private void ChangeAvatarBodyType(float newValue)
    {
        avatarCreationManager.ChangeBodySize(newValue, targetedBodyType);
    }
}
