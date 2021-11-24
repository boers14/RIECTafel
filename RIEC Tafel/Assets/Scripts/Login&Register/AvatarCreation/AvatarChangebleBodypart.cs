using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarChangebleBodypart : MonoBehaviour
{
    public float sizeChangePercentage = 0.5f;

    [System.NonSerialized]
    public List<float> standardSizes = new List<float>();

    [System.NonSerialized]
    public Vector3 standardScale = Vector3.zero;

    [System.NonSerialized]
    public float newSizeFactor = 0.5f;

    public enum SizeChangeDirection
    {
        X,
        Y,
        Z
    }

    public List<SizeChangeDirection> changeDirections = new List<SizeChangeDirection>();

    public AvatarCreationManager.TargetedBodyType bodyType = 0;

    private AvatarCreationManager avatarCreationManager = null;

    private Mesh meshAtStartOfCreation = null;

    private float newSizeFactorAtStartOfCreation = 0;

    private List<AvatarCreationButton> avatarCreationButtons = new List<AvatarCreationButton>();

    private Slider connectedAvatarCreationSlider = null;

    public void Start()
    {
        if (avatarCreationButtons.Count == 0)
        {
            avatarCreationButtons.AddRange(FindObjectsOfType<AvatarCreationButton>());
            avatarCreationManager = FindObjectOfType<AvatarCreationManager>();

            List<AvatarCreationSlider> avatarCreationSliders = new List<AvatarCreationSlider>();
            avatarCreationSliders.AddRange(FindObjectsOfType<AvatarCreationSlider>());
            connectedAvatarCreationSlider = avatarCreationSliders.Find(slider => slider.targetedBodyType == bodyType).GetComponent<Slider>();
        }

        standardScale = transform.localScale;
        standardSizes.Clear();
        for (int i = 0; i < changeDirections.Count; i++)
        {
            switch (changeDirections[i])
            {
                case SizeChangeDirection.X:
                    standardSizes.Add(transform.localScale.x);
                    break;
                case SizeChangeDirection.Y:
                    standardSizes.Add(transform.localScale.y);
                    break;
                case SizeChangeDirection.Z:
                    standardSizes.Add(transform.localScale.z);
                    break;
            }
        }
    }

    private void OnEnable()
    {
        meshAtStartOfCreation = GetComponent<MeshFilter>().mesh;
        newSizeFactorAtStartOfCreation = newSizeFactor;
    }

    public void CancelAvatarCreation()
    {
        string modelName = meshAtStartOfCreation.name.Split(' ')[0];
        Vector3 neededStandardScale = Vector3.one;
        neededStandardScale = avatarCreationButtons.Find(button => button.targetedBodyType == bodyType && 
                button.bodyPartShape.name == modelName).newBodyScale;

        connectedAvatarCreationSlider.value = newSizeFactorAtStartOfCreation;
        avatarCreationManager.ChangeBodySize(newSizeFactorAtStartOfCreation, bodyType);
        avatarCreationManager.ChangeBodyType(meshAtStartOfCreation, bodyType, neededStandardScale);
    }
}
