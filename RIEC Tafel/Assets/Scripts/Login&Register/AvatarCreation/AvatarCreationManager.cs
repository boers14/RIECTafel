using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarCreationManager : MonoBehaviour
{
    [SerializeField]
    private AvatarChangebleBodypart avatarHead = null, avatarBody = null;

    [SerializeField]
    private Transform ground = null;

    public enum TargetedBodyType
    {
        Head,
        Body
    }

    /// <summary>
    /// Change the model of the targeted object based on its standard size
    /// </summary>

    public void ChangeBodyType(Mesh newModel, TargetedBodyType targetedBodyType, Vector3 newBodySize)
    {
        switch (targetedBodyType)
        {
            case TargetedBodyType.Head:
                SetNewModel(avatarHead, targetedBodyType, newModel, newBodySize);
                break;
            case TargetedBodyType.Body:
                SetNewModel(avatarBody, targetedBodyType, newModel, newBodySize);
                break;
        }
    }

    /// <summary>
    /// Set new model and reset its variables. Set its position correctly.
    /// </summary>

    private void SetNewModel(AvatarChangebleBodypart bodypart, TargetedBodyType targetedBodyType, Mesh newModel, Vector3 newBodySize)
    {
        bodypart.GetComponent<MeshFilter>().mesh = newModel;
        bodypart.transform.localScale = newBodySize;
        bodypart.Start();
        ChangeBodySize(bodypart.newSizeFactor, targetedBodyType);
    }

    /// <summary>
    /// Change the size of the body and if its of the body type change the y position accordingly
    /// </summary>

    public void ChangeBodySize(float newSizeFactor, TargetedBodyType targetedBodyType)
    {
        switch(targetedBodyType)
        {
            case TargetedBodyType.Head:
                avatarHead.transform.localScale = ReturnNewShapeSize(newSizeFactor, avatarHead);
                break;
            case TargetedBodyType.Body:
                // Parent head to body so that its moves accordingly upward as the y scale changes, remember the old scales
                Vector3 currentHeadScale = avatarHead.transform.localScale;
                Vector3 currentHeadPos = avatarHead.transform.position;
                float oldSizeFactor = avatarBody.newSizeFactor;
                avatarHead.transform.SetParent(avatarBody.transform);

                avatarBody.transform.localScale = ReturnNewShapeSize(newSizeFactor, avatarBody);

                // Move body based on its body type and new scale
                Vector3 newBodyPos = avatarBody.transform.position;
                if (avatarBody.standardScale.y > avatarBody.standardScale.x)
                {
                    newBodyPos.y = ground.position.y + avatarBody.transform.localScale.y / 2;
                }
                else
                {
                    newBodyPos.y = ground.position.y + avatarBody.transform.localScale.y;
                }
                avatarBody.transform.position = newBodyPos;

                // Set old scale of head
                avatarHead.transform.SetParent(avatarBody.transform.parent);
                avatarHead.transform.localScale = currentHeadScale;

                // Make sure head doesnt move if the scale stays the same
                if (oldSizeFactor == newSizeFactor)
                {
                    avatarHead.transform.position = currentHeadPos;
                }
                break;
        }
    }

    /// <summary>
    /// Return the size of the new model based on the value of the standard size and percentage of the changed size
    /// </summary>

    private Vector3 ReturnNewShapeSize(float newSizeFactor, AvatarChangebleBodypart bodypart)
    {
        bodypart.newSizeFactor = newSizeFactor;

        Vector3 newSize = bodypart.transform.localScale;
        for (int i = 0; i < bodypart.changeDirections.Count; i++)
        {
            switch (bodypart.changeDirections[i])
            {
                case AvatarChangebleBodypart.SizeChangeDirection.X:
                    newSize.x = ReturnAddedSize(newSizeFactor, bodypart.sizeChangePercentage, bodypart.standardSizes[i]);
                    break;
                case AvatarChangebleBodypart.SizeChangeDirection.Y:
                    newSize.y = ReturnAddedSize(newSizeFactor, bodypart.sizeChangePercentage, bodypart.standardSizes[i]);
                    break;
                case AvatarChangebleBodypart.SizeChangeDirection.Z:
                    newSize.z = ReturnAddedSize(newSizeFactor, bodypart.sizeChangePercentage, bodypart.standardSizes[i]);
                    break;
            }
        }

        return newSize;
    }

    /// <summary>
    /// Return new size based standard size + the min size + the max size * percentage
    /// </summary>

    private float ReturnAddedSize(float newSizeFactor, float sizeChangePercentage, float standardSize)
    {
        float sizeChangeRange = standardSize * sizeChangePercentage;
        return standardSize + (-sizeChangeRange + (sizeChangeRange * 2 * newSizeFactor));
    }
}
