using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarCreationButton : MonoBehaviour
{
    public Mesh bodyPartShape = null;

    private AvatarCreationManager avatarCreationManager = null;

    public AvatarCreationManager.TargetedBodyType targetedBodyType = 0;

    public Vector3 newBodyScale = Vector3.one;

    private void Start()
    {
        avatarCreationManager = FindObjectOfType<AvatarCreationManager>();
        GetComponent<Button>().onClick.AddListener(ChangeAvatarBodyType);
    }

    private void ChangeAvatarBodyType()
    {
        avatarCreationManager.ChangeBodyType(bodyPartShape, targetedBodyType, newBodyScale);
    }
}
