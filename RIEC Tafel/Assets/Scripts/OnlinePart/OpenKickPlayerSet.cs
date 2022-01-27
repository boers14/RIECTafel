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

    [SerializeField]
    private int maskAfterClickingButton = 0;

    private List<InputDevice> inputDevices = new List<InputDevice>();

    public PlayerConnection player = null;

    private PlayerHandsRayInteractor interactor = null;

    private List<PlayerHandRays> handRays = new List<PlayerHandRays>();

    private List<bool> buttonIsDown = new List<bool>();

    /// <summary>
    /// Initialize variables
    /// </summary>

    private void Start()
    {
        InitializeControllers.InitializeControllersBasedOnHandRays(inputDevices, handRays, rightCharacteristics, leftCharacteristics);

        handRays.AddRange(FindObjectsOfType<PlayerHandRays>());
        for (int i = 0; i < handRays.Count; i++)
        {
            buttonIsDown.Add(false);
        }

        interactor = GetComponent<PlayerHandsRayInteractor>();
        if (interactor != null)
        {
            interactor.objectHoverEnteredEvent.AddListener(SetToBeingHovered);
            interactor.objectHoverExitedEvent.AddListener(SetToNotBeingHovered);
        }
    }

    /// <summary>
    /// Only perform the update if its the server and the player is hovered. 
    /// If the primary button is down then perform the on button down action
    /// </summary>

    private void Update()
    {
        if (inputDevices.Count < 2)
        {
            inputDevices.Clear();
            InitializeControllers.InitializeControllersBasedOnHandRays(inputDevices, handRays, rightCharacteristics, 
                leftCharacteristics);
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
                // Make sure event only fires once
                if (!buttonIsDown[i])
                {
                    buttonIsDown[i] = true;
                    OnButtonClickAction();
                }
            } else if (!primaryButton)
            {
                buttonIsDown[i] = false;
            }
        }
    }

    /// <summary>
    /// Virtual so it can be used for kick player script. Set correct variables for what enable set is set to.
    /// </summary>

    public virtual void OnButtonClickAction()
    {
        if (kickPlayerSet.activeSelf == enableSet) { return; }

        // Change player layer to a layer that wont block ray for the UI of kick player to be selected
        // Change it back when the UI is turned off
        player.gameObject.layer = maskAfterClickingButton;

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

    /// <summary>
    /// Set name text and alignment to selected option
    /// </summary>

    private void SetNameTextStats(string text, VerticalAlignmentOptions alignmentOption)
    {
        nameText.text = text;
        nameText.verticalAlignment = alignmentOption;
    }

    /// <summary>
    /// Set object to being hovered/ unhovered. Change the alpha of the line so that it is visible when the player is
    /// hovering the object. Set it to inviseble when not hovering the object.
    /// </summary>

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
