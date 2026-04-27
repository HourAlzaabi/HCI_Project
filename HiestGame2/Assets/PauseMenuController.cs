using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public Button resumeButton;
    public Button restartButton;
    public Button lobbyButton;
    public Button quitButton;

    [Header("Warning Popup")]
    public GameObject lobbyWarningPopup;

    [Header("Feedback")]
    public TMP_Text feedbackText;

    void Start()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
       
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (lobbyWarningPopup != null)
            lobbyWarningPopup.SetActive(false);

        SetAllButtonsInteractable(true);
    }

    void SetAllButtonsInteractable(bool state)
    {
        if (resumeButton != null) resumeButton.interactable = state;
        if (restartButton != null) restartButton.interactable = state;
        if (lobbyButton != null) lobbyButton.interactable = state;
    }

    public void ResumeGame()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.ResumeGame();
    }

    public void RestartLevel()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.RestartLevel();
    }

    public void OnLobbyButtonClicked()
    {
        // Show warning popup instead of going directly
        if (lobbyWarningPopup != null)
            lobbyWarningPopup.SetActive(true);
        else
            ConfirmReturnToLobby(); // no popup assigned, go directly
    }

    public void ConfirmReturnToLobby()
    {
        if (lobbyWarningPopup != null)
            lobbyWarningPopup.SetActive(false);

        if (LevelManager.Instance != null)
            LevelManager.Instance.GoToLobby();
    }

    public void CancelReturnToLobby()
    {
        if (lobbyWarningPopup != null)
            lobbyWarningPopup.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Called by LevelManager when pause state changes
    public void OnPauseStateChanged(bool isPaused)
    {
        if (isPaused)
        {
            // Re-enable everything every time pause opens
            SetAllButtonsInteractable(true);

            // Only the player who paused can resume
            bool iAmThePauser = !Photon.Pun.PhotonNetwork.IsConnected ||
                (LevelManager.Instance != null &&
                 Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber ==
                 LevelManager.Instance.pausedByActor);

            if (resumeButton != null)
                resumeButton.interactable = iAmThePauser;

            if (feedbackText != null)
                feedbackText.text = iAmThePauser ? "" : "Other player paused the game";
        }
        else
        {
            // Game resumed - re-enable everything
            SetAllButtonsInteractable(true);
            if (feedbackText != null) feedbackText.text = "";
            if (lobbyWarningPopup != null) lobbyWarningPopup.SetActive(false);
        }
    }
}