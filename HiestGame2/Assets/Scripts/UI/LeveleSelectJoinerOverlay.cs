using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectJoinerOverlay : MonoBehaviour
{
    [Header("Joiner Overlay")]
    [Tooltip("The whole overlay GameObject — shown only to joiner")]
    public GameObject joinerOverlay;
    public TMP_Text joinerOverlayText;

    [Header("Level Limits")]
    public int totalLevelsAvailable = 3; // levels 1..3 are built, anything past goes to LevelSelect

    [Header("Level Buttons")]
    [Tooltip("All LevelButton instances in the scene — disabled for joiner")]
    public LevelButton[] levelButtons;

    [Header("Other Joiner-Disabled Buttons (optional)")]
    [Tooltip("Any extra buttons to disable for joiner (back button, etc.)")]
    public Button[] additionalJoinerDisabledButtons;

    void Start()
    {
        bool isHost = !PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient;

        // Show overlay only to joiner
        if (joinerOverlay != null)
            joinerOverlay.SetActive(!isHost);

        if (joinerOverlayText != null)
            joinerOverlayText.text = "Host is choosing the level...";

        // Disable level buttons for joiner
        if (!isHost)
        {
            foreach (LevelButton lb in levelButtons)
            {
                if (lb != null && lb.levelButton != null)
                    lb.levelButton.interactable = false;
            }

            foreach (Button b in additionalJoinerDisabledButtons)
            {
                if (b != null) b.interactable = false;
            }
        }
    }
}