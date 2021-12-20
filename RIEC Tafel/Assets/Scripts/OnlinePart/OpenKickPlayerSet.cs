using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class OpenKickPlayerSet : MonoBehaviour
{
    [SerializeField]
    private bool enableSet = false;

    [SerializeField]
    private GameObject kickPlayerSet = null;

    [SerializeField]
    private TMP_Text nameText = null;

    private bool isBeingHovered = false;

    [SerializeField]
    private InputDeviceCharacteristics leftCharacteristics = 0, rightCharacteristics = 0;

    private List<InputDevice> inputDevices = new List<InputDevice>();

    public PlayerConnection player = null;

    private PlayerHandsRayInteractor interactor = null;

    private List<PlayerHandRays> handRays = new List<PlayerHandRays>();

    private void Start()
    {
        InitializeControllers.InitializeControllersBasedOnHandRays(inputDevices, handRays, rightCharacteristics, leftCharacteristics);

        handRays.AddRange(FindObjectsOfType<PlayerHandRays>());

        interactor = GetComponent<PlayerHandsRayInteractor>();
        if (interactor != null)
        {
            interactor.objectHoverEnteredEvent.AddListener(SetToBeingHovered);
            interactor.objectHoverExitedEvent.AddListener(SetToNotBeingHovered);
        }
    }

    private void Update()
    {
        if (inputDevices.Count < 2)
        {
            inputDevices.Clear();
            InitializeControllers.InitializeControllersBasedOnHandRays(inputDevices, handRays, rightCharacteristics, leftCharacteristics);
            return;
        }

        if (!isBeingHovered || !player.isServer)
        {
            return;
        }

        for (int i = 0; i < inputDevices.Count; i++)
        {
            if (inputDevices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton && 
                handRays[i].hoveredObjects.Contains(interactor))
            {
                OnButtonClickAction();
            }
        }
    }

    public virtual void OnButtonClickAction()
    {
        if (kickPlayerSet.activeSelf == enableSet) { return; }

        kickPlayerSet.SetActive(enableSet);
        if (enableSet)
        {
            SetNameTextStats("Wilt u dit persoon verwijderen?", VerticalAlignmentOptions.Middle);
        }
        else
        {
            SetNameTextStats(player.playerName, VerticalAlignmentOptions.Bottom);
        }
    }

    private void SetNameTextStats(string text, VerticalAlignmentOptions alignmentOption)
    {
        nameText.text = text;
        nameText.verticalAlignment = alignmentOption;
    }

    private void SetToBeingHovered()
    {
        isBeingHovered = true;
        EnablePlayerHandRayVisuals(1, true);
    }

    private void SetToNotBeingHovered()
    {
        isBeingHovered = false;
        EnablePlayerHandRayVisuals(0, false);
    }

    private void EnablePlayerHandRayVisuals(float alpha, bool contains)
    {
        for (int i = 0; i < handRays.Count; i++)
        {
            if (handRays[i].hoveredObjects.Contains(interactor) == contains)
            {
                if (!contains && handRays[i].hoveredObjects.Count == 0)
                {
                    handRays[i].ChangeInvalidColorGradientOfLineRenderer(alpha);
                } else if (contains)
                {
                    handRays[i].ChangeInvalidColorGradientOfLineRenderer(alpha);
                }
            }
        }
    }
}
