using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SessionCreationManager : MonoBehaviourPunCallbacks
{
    [Header("Input")]
    public TMP_InputField joinCodeInput;

    [Header("Status")]
    public TMP_Text statusText;

    [Header("Buttons")]
    public Button hostButton;
    public Button joinButton;
    public Button backButton;

    [Header("Panels")]
    public GameObject hostPanel;
    public GameObject joinPanel;

    [Header("Descriptions")]
    public TMP_Text hostDescriptionText;
    public TMP_Text joinDescriptionText;

   

    [Header("Failure Popup")]
    public GameObject failurePopup;
    public TMP_Text failurePopupText;
    public float failurePopupDuration = 2f;

    [Header("Background")]
    public BackgroundPlayer backgroundPlayer;

    private bool isProcessing = false;
    private Coroutine failurePopupCoroutine;

    void Start()
    {
        ResetSceneState();

        if (PhotonNetwork.IsConnected)
        {
            statusText.text = "Reconnecting...";
            PhotonNetwork.AutomaticallySyncScene = true;

            if (PhotonNetwork.InLobby)
                OnJoinedLobby();
            else
                PhotonNetwork.JoinLobby();
        }
        else
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void ResetSceneState()
    {
        isProcessing = false;

        
        if (joinCodeInput != null)
            joinCodeInput.text = "";

        if (statusText != null)
            statusText.text = "Connecting...";

        if (failurePopup != null)
            failurePopup.SetActive(false);

        if (hostDescriptionText != null)
            hostDescriptionText.text =
                "Start a new session and invite a friend to join with your code.";

        if (joinDescriptionText != null)
            joinDescriptionText.text =
                "Enter the code shared by your host to join their session.";

        // Buttons disabled until connected
        SetButtonsInteractable(false);
    }

    void SetButtonsInteractable(bool state)
    {
        if (hostButton != null) hostButton.interactable = state;
        if (joinButton != null) joinButton.interactable = state;
        if (backButton != null) backButton.interactable = state;
        if (joinCodeInput != null) joinCodeInput.interactable = state;
    }

    public void OnBackClicked()
    {
        if (isProcessing) return;
        isProcessing = true;

        SetButtonsInteractable(false);
        statusText.text = "Returning to main menu...";

        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();

        if (backgroundPlayer != null)
            backgroundPlayer.PlayExitThen(() =>
                SceneManager.LoadScene("MainMenu"));
        else
            SceneManager.LoadScene("MainMenu");
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected!";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        statusText.text = "Ready! Host or join a session.";
        SetButtonsInteractable(true);
    }

    public void OnHostClicked()
    {
        if (isProcessing) return;
        isProcessing = true;

        SetButtonsInteractable(false);
        statusText.text = "Creating session...";

        string code = Random.Range(100000, 999999).ToString();
        GameData.JoinCode = code;
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(code, options);
    }

    public void OnJoinClicked()
    {
        if (isProcessing) return;

        string code = joinCodeInput.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(code))
        {
            statusText.text = "Please enter a code.";
            return;
        }

        isProcessing = true;
        SetButtonsInteractable(false);
        statusText.text = "Searching for session...";

        PhotonNetwork.JoinRoom(code);
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Connected! Entering lobby...";

        if (backgroundPlayer != null)
            backgroundPlayer.PlayExitThen(() =>
                PhotonNetwork.LoadLevel("Lobby"));
        else
            PhotonNetwork.LoadLevel("Lobby");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        ShowFailurePopup("Could not create session. Please try again.");
        statusText.text = "Ready! Host or join a session.";
        isProcessing = false;
        SetButtonsInteractable(true);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        ShowFailurePopup("No session exists with that code.");
        statusText.text = "Ready! Host or join a session.";
        isProcessing = false;
        SetButtonsInteractable(true);
    }

    void ShowFailurePopup(string message)
    {
        if (failurePopup == null) return;

        if (failurePopupCoroutine != null)
            StopCoroutine(failurePopupCoroutine);

        failurePopupCoroutine = StartCoroutine(FailurePopupRoutine(message));
    }

    IEnumerator FailurePopupRoutine(string message)
    {
        if (failurePopupText != null)
            failurePopupText.text = message;

        failurePopup.SetActive(true);
        yield return new WaitForSeconds(failurePopupDuration);
        failurePopup.SetActive(false);
        failurePopupCoroutine = null;
    }
}