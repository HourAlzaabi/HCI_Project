using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Roles Data")]
    public PlayerRolesData rolesData;

    [Header("Code & Status")]
    public TMP_Text joinCodeText;
    public TMP_Text waitingText;
    public TMP_Text playerCountText;

    [Header("Ready")]
    public Button readyButton;
    public TMP_Text readyButtonText;
    public BackgroundPlayer backgroundPlayer;

    [Header("Player 1 Panel (Host)")]
    public GameObject player1Panel;
    public Image player1PreviewImage;
    public TMP_Text player1TagText;
    public TMP_Text player1StatusText;

    [Header("Player 2 Panel (Joiner)")]
    public GameObject player2Panel;
    public Image player2PreviewImage;
    public TMP_Text player2TagText;
    public TMP_Text player2StatusText;

    [Header("UI to hide when ready")]
    public GameObject readyButton_GO;
    public GameObject codeText_GO;
    public GameObject backButton_GO;
    public GameObject playerCount_GO;

    [Header("Back Warning Popup")]
    public GameObject backWarningPopup;
    public TMP_Text warningText;

    [Header("Scene Title")]
    public TMP_Text sceneTitleText;

    [Header("Main lobby content to delay")]
    public GameObject mainLobbyContent;
    public float lobbyContentDelay = 2f;

    [Header("Player section loads last")]
    public GameObject playerSection;
    public float playerSectionDelay = 1f;

    [Header("Notification")]
    public GameObject notificationPanel;
    public TMP_Text notificationText;
    public float notificationDuration = 2f;

    private bool isLoadingLevel = false;
    private Coroutine notificationCoroutine;

    void Start()
    {
        isLoadingLevel = false;

        if (notificationPanel != null) notificationPanel.SetActive(false);

        if (sceneTitleText != null)
            sceneTitleText.text = "Entering Lobby...";

        if (mainLobbyContent != null) mainLobbyContent.SetActive(false);
        if (playerSection != null) playerSection.SetActive(false);

        StartCoroutine(ShowLobbyAfterDelay());

        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(OnReadyClicked);
        readyButton.interactable = false; // disabled until 2nd player joins

        if (backWarningPopup != null) backWarningPopup.SetActive(false);

        // Initialize panels to default empty state
        ResetPlayer1Panel();
        ResetPlayer2Panel();

        if (PhotonNetwork.CurrentRoom != null)
            joinCodeText.text = "Code: " + PhotonNetwork.CurrentRoom.Name;
        else
            joinCodeText.text = "Code: Error";

        // CRITICAL: if we joined the room in a previous scene, OnJoinedRoom won't fire here.
        if (PhotonNetwork.InRoom)
            InitializeForRoom();
    }

    IEnumerator ShowLobbyAfterDelay()
    {
        yield return new WaitForSeconds(lobbyContentDelay);

        if (sceneTitleText != null)
            sceneTitleText.text = "Lobby";

        if (mainLobbyContent != null)
            mainLobbyContent.SetActive(true);

        yield return new WaitForSeconds(playerSectionDelay);

        if (playerSection != null)
            playerSection.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[LobbyManager] OnJoinedRoom");
        InitializeForRoom();
    }

    void InitializeForRoom()
    {
        ResetReadyState();

        if (PhotonNetwork.CurrentRoom != null)
            joinCodeText.text = "Code: " + PhotonNetwork.CurrentRoom.Name;

        RefreshAllPanels();
    }

    // ======================================================================
    // ROLE-BASED PANEL ASSIGNMENT
    // Player 1 panel = master client, Player 2 panel = joiner.
    // Same on both clients.
    // ======================================================================

    void RefreshAllPanels()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        Player p1 = null;
        Player p2 = null;

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.IsMasterClient) p1 = p;
            else p2 = p;
        }

        if (p1 != null) ApplyPlayerToPanel1(p1);
        else ResetPlayer1Panel();

        if (p2 != null) ApplyPlayerToPanel2(p2);
        else ResetPlayer2Panel();

        RefreshPlayerCount();
        UpdateReadyButtonInteractable();
        UpdateStatus();
    }

    void ApplyPlayerToPanel1(Player p)
    {
        if (player1PreviewImage != null && rolesData != null)
        {
            player1PreviewImage.gameObject.SetActive(true);
            player1PreviewImage.sprite = rolesData.player1Sprite;
            player1PreviewImage.color = Color.white;
        }
        if (player1TagText != null && rolesData != null)
            player1TagText.text = rolesData.player1Tag;

        UpdatePanel1ReadyVisuals(GetPlayerReady(p));
    }

    void ApplyPlayerToPanel2(Player p)
    {
        if (player2PreviewImage != null && rolesData != null)
        {
            player2PreviewImage.gameObject.SetActive(true);
            player2PreviewImage.sprite = rolesData.player2Sprite;
            player2PreviewImage.color = Color.white;
        }
        if (player2TagText != null && rolesData != null)
            player2TagText.text = rolesData.player2Tag;

        UpdatePanel2ReadyVisuals(GetPlayerReady(p));
    }

    void ResetPlayer1Panel()
    {
        if (player1PreviewImage != null && rolesData != null)
        {
            player1PreviewImage.gameObject.SetActive(true);
            player1PreviewImage.sprite = rolesData.player1Sprite;
            player1PreviewImage.color = new Color(1f, 1f, 1f, 0.3f); // dim if absent
        }
        if (player1TagText != null && rolesData != null)
            player1TagText.text = rolesData.player1Tag;
        if (player1StatusText != null)
        {
            player1StatusText.text = "Waiting...";
            player1StatusText.color = Color.gray;
        }
    }

    void ResetPlayer2Panel()
    {
        if (player2PreviewImage != null && rolesData != null)
        {
            player2PreviewImage.gameObject.SetActive(true);
            player2PreviewImage.sprite = rolesData.player2Sprite;
            player2PreviewImage.color = new Color(1f, 1f, 1f, 0.3f); // dim if absent
        }
        if (player2TagText != null && rolesData != null)
            player2TagText.text = rolesData.player2Tag;
        if (player2StatusText != null)
        {
            player2StatusText.text = "Waiting...";
            player2StatusText.color = Color.gray;
        }
    }

    void UpdatePanel1ReadyVisuals(bool isReady)
    {
        if (player1StatusText != null)
        {
            player1StatusText.text = isReady ? "Ready" : "Not Ready";
            player1StatusText.color = isReady ? Color.green : Color.red;
        }
    }

    void UpdatePanel2ReadyVisuals(bool isReady)
    {
        if (player2StatusText != null)
        {
            player2StatusText.text = isReady ? "Ready" : "Not Ready";
            player2StatusText.color = isReady ? Color.green : Color.red;
        }
    }

    bool GetPlayerReady(Player p)
    {
        if (p == null) return false;
        if (!p.CustomProperties.ContainsKey("ready")) return false;
        return (bool)p.CustomProperties["ready"];
    }

    void RefreshPlayerCount()
    {
        if (playerCountText == null) return;
        int count = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
        playerCountText.text = count + "/2";
    }

    void UpdateReadyButtonInteractable()
    {
        if (readyButton == null) return;
        bool bothPresent = PhotonNetwork.CurrentRoom != null
            && PhotonNetwork.CurrentRoom.PlayerCount >= 2;
        readyButton.interactable = bothPresent;
    }

    // ======================================================================
    // BACK BUTTON
    // ======================================================================

    public void OnBackClicked()
    {
        if (backWarningPopup != null)
        {
            backWarningPopup.SetActive(true);
            if (warningText != null)
                warningText.text =
                    "Leaving the lobby will end the session for both players. Are you sure?";
        }
    }

    public void OnBackConfirmed()
    {
        HideAllUI();

        if (backWarningPopup != null)
            backWarningPopup.SetActive(false);

        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        else
            GoToSessionCreation();
    }

    public void OnBackCancelled()
    {
        if (backWarningPopup != null)
            backWarningPopup.SetActive(false);
    }

    public override void OnLeftRoom()
    {
        GoToSessionCreation();
    }

    void GoToSessionCreation()
    {
        if (backgroundPlayer != null)
            backgroundPlayer.PlayExitThen(() =>
                SceneManager.LoadScene("SessionCreation"));
        else
            SceneManager.LoadScene("SessionCreation");
    }

    // ======================================================================
    // READY STATE
    // ======================================================================

    void ResetReadyState()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[LobbyManager] ResetReadyState skipped: Not in room yet.");
            return;
        }

        Hashtable props = new Hashtable { { "ready", false } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void OnReadyClicked()
    {
        if (!PhotonNetwork.InRoom) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2) return;

        bool currentReady = false;
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("ready"))
            currentReady = (bool)PhotonNetwork.LocalPlayer.CustomProperties["ready"];

        bool newReady = !currentReady;

        // Update local button text immediately for responsiveness
        if (readyButtonText != null)
            readyButtonText.text = newReady ? "Unready" : "Ready";

        // Authoritative update — OnPlayerPropertiesUpdate refreshes panels for both clients
        Hashtable props = new Hashtable { { "ready", newReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    // ======================================================================
    // PHOTON CALLBACKS
    // ======================================================================

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(">>> PLAYER JOINED: " + newPlayer.NickName);

        ShowNotification(GetPlayerLabel(newPlayer) + " joined the lobby");

        RefreshAllPanels();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(">>> PLAYER LEFT: " + otherPlayer.NickName);
        string leaverLabel = GetPlayerLabel(otherPlayer);

        if (otherPlayer.IsMasterClient)
        {
            // Host left — joiner gets routed to session creation
            if (!PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(NotifySessionEndedAndLeave());
                return;
            }
        }
        else
        {
            ShowNotification(leaverLabel + " left the lobby");
        }

        RefreshAllPanels();
    }

    IEnumerator NotifySessionEndedAndLeave()
    {
        if (backWarningPopup != null)
        {
            backWarningPopup.SetActive(true);
            if (warningText != null)
                warningText.text = "Host left — session ended. Returning to session creation...";
        }

        yield return new WaitForSeconds(2.5f);

        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        else
            GoToSessionCreation();
    }

    void UpdateStatus()
    {
        if (PhotonNetwork.CurrentRoom == null || waitingText == null) return;
        int count = PhotonNetwork.CurrentRoom.PlayerCount;

        if (count < 2)
            waitingText.text = "Waiting for other player to join...";
        else
            waitingText.text = "Both connected! Press Ready when you are.";
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!changedProps.ContainsKey("ready")) return;

        bool isReady = (bool)changedProps["ready"];

        // Use IsMasterClient (role) NOT IsLocal — same panel on both clients
        if (targetPlayer.IsMasterClient)
            UpdatePanel1ReadyVisuals(isReady);
        else
            UpdatePanel2ReadyVisuals(isReady);

        // If it's me, also update my ready button text
        if (targetPlayer.IsLocal && readyButtonText != null)
            readyButtonText.text = isReady ? "Unready" : "Ready";

        // Update waiting text based on ready states
        if (isReady)
            waitingText.text = "Waiting for other player to ready up...";
        else
            UpdateStatus();

        CheckAllReady();
    }

    void CheckAllReady()
    {
        if (!PhotonNetwork.InRoom) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2) return;

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.ContainsKey("ready") ||
                !(bool)p.CustomProperties["ready"])
            {
                waitingText.text = "Waiting for other player to ready up...";
                return;
            }
        }

        if (isLoadingLevel) return;
        isLoadingLevel = true;

        waitingText.text = "Both ready! Loading...";
        HideAllUI();

        if (PhotonNetwork.IsMasterClient)
        {
            if (backgroundPlayer != null)
                backgroundPlayer.PlayExitThen(() =>
                {
                    if (PhotonNetwork.IsMasterClient)
                        PhotonNetwork.LoadLevel("LevelSelect");
                });
            else
                PhotonNetwork.LoadLevel("LevelSelect");
        }
        else
        {
            if (backgroundPlayer != null)
                backgroundPlayer.PlayExitThen(null);

            Debug.Log("Client waiting for host to load level...");
        }
    }

    void HideAllUI()
    {
        if (readyButton_GO != null) readyButton_GO.SetActive(false);
        if (codeText_GO != null) codeText_GO.SetActive(false);
        if (backButton_GO != null) backButton_GO.SetActive(false);
        if (playerCount_GO != null) playerCount_GO.SetActive(false);
        if (player1Panel != null) player1Panel.SetActive(false);
        if (player2Panel != null) player2Panel.SetActive(false);
        if (waitingText != null) waitingText.gameObject.SetActive(false);
        if (joinCodeText != null) joinCodeText.gameObject.SetActive(false);
        if (mainLobbyContent != null) mainLobbyContent.SetActive(false);
        if (playerSection != null) playerSection.SetActive(false);
        if (sceneTitleText != null) sceneTitleText.gameObject.SetActive(false);
    }

    // ======================================================================
    // NOTIFICATION
    // ======================================================================

    public void ShowNotification(string message)
    {
        if (notificationPanel == null || notificationText == null) return;

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(ShowNotificationRoutine(message));
    }

    IEnumerator ShowNotificationRoutine(string message)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);
        yield return new WaitForSeconds(notificationDuration);
        notificationPanel.SetActive(false);
        notificationCoroutine = null;
    }

    string GetPlayerLabel(Player p)
    {
        if (rolesData == null) return "Player";
        return p.IsMasterClient ? rolesData.player1Tag : rolesData.player2Tag;
    }
}