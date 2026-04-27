using Photon.Pun;
using Photon.Realtime;
using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviourPun
{
    public static LevelManager Instance;

    public event System.Action OnCollectiblesChanged;

    private bool isReturningToLobby = false;
    private bool isRestarting = false;

    [Header("Collectibles")]
    public int coinsCollected;
    public int cashCollected;
    public int gemsCollected;

    [Header("Values")]
    public int coinValue = 1;
    public int cashValue = 5;
    public int gemValue = 10;

    [Header("Time")]
    public float levelTimer;
    public float maxLevelTime = 300f;
    public bool timerRunning = true;
    public float failedPanelDelay = 0.35f;

    [Header("State")]
    public bool isPaused;
    public bool missionEnded;
    public bool missionSuccess;

    [Header("Stars")]
    public int earnedStars;
    public int oneStarScore = 12;
    public int twoStarScore = 22;
    public int threeStarScore = 32;

    [Header("UI")]
    public GameObject pauseBlurPanel;
    public GameObject pauseOverlay;
    public GameObject pauseMenu;
    public GameObject missionCompletePanel;
    public GameObject missionFailedPanel;
    public FailOverlayFX failOverlayFX;

    [Header("Decision Notification")]
    public GameObject notificationPanel;
    public TMP_Text notificationText;
    public float notificationDuration = 2f;

    [Header("Player Control")]
    public MonoBehaviour[] playerMovementScripts;

    [Header("Level Limits")]
    public int totalLevelsAvailable = 3;

    private List<string> collectedIds = new List<string>();

    public int pausedByActor = -1;

    private bool isLoadingNextLevel = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        Time.timeScale = 1f;

        // Ensure scene sync is on for all level transitions
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        PhotonNetwork.AddCallbackTarget(this);
        isRestarting = false;
        isLoadingNextLevel = false;
        levelTimer = 0f;
        timerRunning = true;
        missionEnded = false;
        missionSuccess = false;
        isPaused = false;
        isReturningToLobby = false;
        pausedByActor = -1;
        collectedIds.Clear();
        Time.timeScale = 1f;

        if (pauseBlurPanel != null) pauseBlurPanel.SetActive(false);
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (missionCompletePanel != null) missionCompletePanel.SetActive(false);
        if (missionFailedPanel != null) missionFailedPanel.SetActive(false);
        if (notificationPanel != null) notificationPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Update()
    {
        if (!missionEnded && !isPaused && timerRunning)
        {
            levelTimer += Time.deltaTime;
            if (levelTimer >= maxLevelTime)
                FailMission("Time ran out");
        }

        if (!missionEnded && Keyboard.current != null
            && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // ==================== COLLECTIBLES ====================

    public void CollectAndDestroy(string collectibleId, int type)
    {
        if (missionEnded) return;
        if (collectedIds.Contains(collectibleId)) return;

        if (PhotonNetwork.IsConnected && photonView != null)
            photonView.RPC("RPC_CollectAndDestroy", RpcTarget.All, collectibleId, type);
        else
            RPC_CollectAndDestroy(collectibleId, type);
    }

    [PunRPC]
    void RPC_CollectAndDestroy(string collectibleId, int type)
    {
        if (collectedIds.Contains(collectibleId)) return;
        collectedIds.Add(collectibleId);

        switch (type)
        {
            case 0: coinsCollected++; Debug.Log("Coin! Total: " + coinsCollected); break;
            case 1: cashCollected++; Debug.Log("Cash! Total: " + cashCollected); break;
            case 2: gemsCollected++; Debug.Log("Gem! Total: " + gemsCollected); break;
        }

        OnCollectiblesChanged?.Invoke();

        Collectible[] all = FindObjectsByType<Collectible>();
        foreach (Collectible c in all)
        {
            if (c.GetUniqueId() == collectibleId)
            {
                c.DestroyMe();
                return;
            }
        }
    }

    // ==================== SCORING ====================

    public int GetCollectibleScore()
    {
        return (coinsCollected * coinValue)
             + (cashCollected * cashValue)
             + (gemsCollected * gemValue);
    }

    public int GetTimeBonus()
    {
        if (levelTimer <= 120f) return 10;
        if (levelTimer <= 180f) return 8;
        if (levelTimer <= 240f) return 6;
        if (levelTimer <= 300f) return 4;
        return 0;
    }

    public int GetTotalScore()
    {
        return GetCollectibleScore() + GetTimeBonus();
    }

    public int CalculateStars()
    {
        int score = GetTotalScore();
        if (score >= threeStarScore) return 3;
        if (score >= twoStarScore) return 2;
        if (score >= oneStarScore) return 1;
        return 0;
    }

    // ==================== MISSION COMPLETE / FAIL ====================

    public void CompleteMission()
    {
        if (missionEnded) return;
        if (PhotonNetwork.IsConnected && photonView != null)
            photonView.RPC("RPC_CompleteMission", RpcTarget.All);
        else
            RPC_CompleteMission();
    }

    [PunRPC]
    void RPC_CompleteMission()
    {
        if (missionEnded) return;
        missionEnded = true;
        missionSuccess = true;
        timerRunning = false;
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseBlurPanel != null) pauseBlurPanel.SetActive(false);
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);

        earnedStars = CalculateStars();
        SetPlayerControl(false);

        if (missionCompletePanel != null)
            missionCompletePanel.SetActive(true);

        SaveLevelProgress();
        Debug.Log("Mission Complete!");
    }

    public void FailMission(string reason)
    {
        if (missionEnded) return;
        if (PhotonNetwork.IsConnected && photonView != null)
            photonView.RPC("RPC_FailMission", RpcTarget.All, reason);
        else
            RPC_FailMission(reason);
    }

    [PunRPC]
    void RPC_FailMission(string reason)
    {
        if (missionEnded) return;
        missionEnded = true;
        missionSuccess = false;
        timerRunning = false;
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseBlurPanel != null) pauseBlurPanel.SetActive(false);
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);

        SetPlayerControl(false);

        if (failOverlayFX != null)
            failOverlayFX.PlayFailOverlay();

        StartCoroutine(ShowFailedPanelAfterDelay());
        Debug.Log("Mission Failed: " + reason);
    }

    // ==================== PAUSE ====================

    public void PauseGame()
    {
        if (missionEnded) return;

        if (photonView == null)
        {
            Debug.LogError("[LevelManager] photonView NULL - cannot pause!");
            return;
        }

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("RPC_PauseGame", RpcTarget.All,
                PhotonNetwork.LocalPlayer.ActorNumber);
            PhotonNetwork.SendAllOutgoingCommands();
        }
        else
            RPC_PauseGame(-1);
    }

    [PunRPC]
    void RPC_PauseGame(int actorWhoPaused)
    {
        if (missionEnded) return;
        Debug.Log("[LevelManager] RPC_PauseGame on Actor: "
            + PhotonNetwork.LocalPlayer.ActorNumber
            + " | Paused by: " + actorWhoPaused);

        isPaused = true;
        timerRunning = false;
        pausedByActor = actorWhoPaused;

        SetPlayerControl(false);

        if (pauseBlurPanel != null) pauseBlurPanel.SetActive(true);
        if (pauseOverlay != null) pauseOverlay.SetActive(true);
        if (pauseMenu != null) pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        if (missionEnded) return;
        if (!isPaused) return;

        if (PhotonNetwork.IsConnected &&
            PhotonNetwork.LocalPlayer.ActorNumber != pausedByActor)
        {
            Debug.Log("[LevelManager] Only Actor " + pausedByActor + " can resume.");
            return;
        }

        if (photonView == null) { RPC_ResumeGame(); return; }

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("RPC_ResumeGame", RpcTarget.All);
            PhotonNetwork.SendAllOutgoingCommands();
        }
        else
            RPC_ResumeGame();
    }

    [PunRPC]
    void RPC_ResumeGame()
    {
        if (missionEnded) return;
        Debug.Log("[LevelManager] RPC_ResumeGame RECEIVED on Actor: "
            + PhotonNetwork.LocalPlayer.ActorNumber);

        isPaused = false;
        timerRunning = true;
        pausedByActor = -1;

        if (pauseBlurPanel != null) pauseBlurPanel.SetActive(false);
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (notificationPanel != null) notificationPanel.SetActive(false);

        SetPlayerControl(true);
        Debug.Log("[LevelManager] Game resumed!");
    }

    // ==================== RESTART ====================

    public void RestartLevel()
    {
        if (isRestarting)
        {
            Debug.LogWarning("[LevelManager] RestartLevel blocked - already restarting.");
            return;
        }
        isRestarting = true;

        Debug.Log("[LevelManager] RestartLevel called by Actor: "
            + PhotonNetwork.LocalPlayer.ActorNumber);

        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (photonView == null) return;

        string callerName = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("playerName")
            ? (string)PhotonNetwork.LocalPlayer.CustomProperties["playerName"]
            : "A player";

        photonView.RPC("RPC_ShowRestartNotification", RpcTarget.All, callerName);
        PhotonNetwork.SendAllOutgoingCommands();

        photonView.RPC("RPC_MasterDoRestart", RpcTarget.MasterClient);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    [PunRPC]
    void RPC_ShowRestartNotification(string callerName)
    {
        isPaused = false;
        pausedByActor = -1;

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
        if (pauseBlurPanel != null) pauseBlurPanel.SetActive(false);
        if (missionCompletePanel != null) missionCompletePanel.SetActive(false);
        if (missionFailedPanel != null) missionFailedPanel.SetActive(false);

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
            if (notificationText != null)
                notificationText.text = callerName + " is restarting...";
        }

        Debug.Log("[LevelManager] RPC_ShowRestartNotification on Actor: "
            + PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void RPC_MasterDoRestart()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        Debug.Log("[LevelManager] Master doing restart.");

        string levelName = SceneManager.GetActiveScene().name;

        if (SceneReloader.Instance == null)
        {
            GameObject go = new GameObject("SceneReloader");
            go.AddComponent<SceneReloader>();
        }

        SceneReloader.Instance.ReloadViaIntermediate(levelName);
    }

    // ==================== NEXT LEVEL ====================

    public void LoadNextLevel()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (isLoadingNextLevel) return;

        Debug.Log("[LevelManager] LoadNextLevel called. AutomaticallySyncScene: "
            + PhotonNetwork.AutomaticallySyncScene);

        int currentLevel = GetCurrentLevelNumber();
        int nextLevel = currentLevel + 1;

        if (nextLevel > totalLevelsAvailable)
        {
            Debug.Log("[LevelManager] No more levels available. Going to LevelSelect.");
            GoToLevelSelect();
            return;
        }

        string nextScene = "Level" + nextLevel;
        isLoadingNextLevel = true;

        // Ensure scene sync is on before loading
        PhotonNetwork.AutomaticallySyncScene = true;

        photonView.RPC(nameof(RPC_PrepareNextLevel), RpcTarget.All, nextScene);
        photonView.RPC(nameof(RPC_MasterLoadNextLevel), RpcTarget.MasterClient, nextScene);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    [PunRPC]
    void RPC_PrepareNextLevel(string nextScene)
    {
        Debug.Log("[LevelManager] RPC_PrepareNextLevel on Actor: "
            + PhotonNetwork.LocalPlayer.ActorNumber + " | next: " + nextScene);

        // Ensure both clients have scene sync enabled
        PhotonNetwork.AutomaticallySyncScene = true;

        if (missionCompletePanel) missionCompletePanel.SetActive(false);
        if (pauseOverlay) pauseOverlay.SetActive(false);
        if (pauseBlurPanel) pauseBlurPanel.SetActive(false);
        if (pauseMenu) pauseMenu.SetActive(false);

        SetPlayerControl(false);
        ShowNotification("Loading " + nextScene + "...");
    }

    [PunRPC]
    void RPC_MasterLoadNextLevel(string nextScene)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        Debug.Log("[LevelManager] Master loading next level: " + nextScene);

        // Make sure SceneReloader exists (this was the missing fix)
        if (SceneReloader.Instance == null)
        {
            GameObject go = new GameObject("SceneReloader");
            go.AddComponent<SceneReloader>();
        }

        SceneReloader.Instance.ReloadViaIntermediate(nextScene);
    }

    public void GoToLevelSelect()
    {
        Time.timeScale = 1f;
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("LevelSelect");
        else if (!PhotonNetwork.IsConnected)
            SceneManager.LoadScene("LevelSelect");
    }

    // ==================== GO TO LOBBY ====================

    public void GoToLobby()
    {
        Debug.Log($"[GoToLobby] Called. Connected={PhotonNetwork.IsConnected}, InRoom={PhotonNetwork.InRoom}, IsMaster={PhotonNetwork.IsMasterClient}");
        if (isReturningToLobby) return;
        isReturningToLobby = true;
        Time.timeScale = 1f;

        if (photonView == null || !PhotonNetwork.IsConnected)
        {
            ClearLevelProgress();
            SceneManager.LoadScene("Lobby");
            return;
        }

        string playerName = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("playerName")
            ? (string)PhotonNetwork.LocalPlayer.CustomProperties["playerName"]
            : "A player";

        photonView.RPC("RPC_GoToLobby", RpcTarget.All, playerName);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    [PunRPC]
    void RPC_GoToLobby(string callerName)
    {
        Debug.Log("[LevelManager] RPC_GoToLobby on Actor: "
            + PhotonNetwork.LocalPlayer.ActorNumber);

        isPaused = false;
        pausedByActor = -1;

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
        if (pauseBlurPanel != null) pauseBlurPanel.SetActive(false);
        if (missionCompletePanel != null) missionCompletePanel.SetActive(false);
        if (missionFailedPanel != null) missionFailedPanel.SetActive(false);

        ShowNotification(callerName + " is returning to lobby...");

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            ExitGames.Client.Photon.Hashtable props =
                new ExitGames.Client.Photon.Hashtable { { "ready", false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        ClearLevelProgress();

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(LoadAfterDelay("Lobby", notificationDuration));
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            ExitGames.Client.Photon.Hashtable props =
                new ExitGames.Client.Photon.Hashtable { { "ready", false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("MainMenu");
        else if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            Debug.Log("[LevelManager] Waiting for host to load main menu...");
        else
            SceneManager.LoadScene("MainMenu");
    }

    // ==================== PHOTON CALLBACKS ====================

    public void OnPlayerEnteredRoom(Player newPlayer) { }
    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }
    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) { }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("[LevelManager] Master switched to: " + newMasterClient.NickName);
    }

    // ==================== NOTIFICATION ====================

    void ShowNotification(string message)
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
            if (notificationText != null)
                notificationText.text = message;
            StartCoroutine(HideNotificationAfterDelay());
        }
        Debug.Log("NOTIFICATION: " + message);
    }

    IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSecondsRealtime(notificationDuration);
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }

    IEnumerator LoadAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        PhotonNetwork.LoadLevel(sceneName);
    }

    // ==================== HELPERS ====================

    private void SetPlayerControl(bool enabledState)
    {
        if (playerMovementScripts == null) return;
        foreach (MonoBehaviour script in playerMovementScripts)
            if (script != null) script.enabled = enabledState;
    }

    private void SaveLevelProgress()
    {
        int levelNumber = GetCurrentLevelNumber();
        string prefix = "Level" + levelNumber;

        int totalScore = GetTotalScore();
        int bestStars = PlayerPrefs.GetInt(prefix + "_stars", 0);
        int bestScore = PlayerPrefs.GetInt(prefix + "_score", 0);
        float bestTime = PlayerPrefs.GetFloat(prefix + "_bestTime", float.MaxValue);

        if (earnedStars > bestStars) PlayerPrefs.SetInt(prefix + "_stars", earnedStars);
        if (totalScore > bestScore) PlayerPrefs.SetInt(prefix + "_score", totalScore);
        if (levelTimer < bestTime) PlayerPrefs.SetFloat(prefix + "_bestTime", levelTimer);

        PlayerPrefs.SetInt(prefix + "_coins", coinsCollected);
        PlayerPrefs.SetInt(prefix + "_cash", cashCollected);
        PlayerPrefs.SetInt(prefix + "_gems", gemsCollected);
        PlayerPrefs.SetInt(prefix + "_completed", 1);
        PlayerPrefs.Save();
    }

    private void ClearLevelProgress()
    {
        int levelNumber = GetCurrentLevelNumber();
        string prefix = "Level" + levelNumber;
        PlayerPrefs.DeleteKey(prefix + "_coins");
        PlayerPrefs.DeleteKey(prefix + "_cash");
        PlayerPrefs.DeleteKey(prefix + "_gems");
        PlayerPrefs.Save();

        coinsCollected = 0;
        cashCollected = 0;
        gemsCollected = 0;
        levelTimer = 0f;
        collectedIds.Clear();
    }

    public int GetCurrentLevelNumber()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string digits = "";
        foreach (char c in sceneName)
            if (char.IsDigit(c)) digits += c;
        return string.IsNullOrEmpty(digits) ? 1 : int.Parse(digits);
    }

    private IEnumerator ShowFailedPanelAfterDelay()
    {
        yield return new WaitForSecondsRealtime(failedPanelDelay);
        if (missionFailedPanel != null)
            missionFailedPanel.SetActive(true);
    }

    public void RegisterPlayerScripts(PlayerController pc, KinematicObject ko)
    {
        playerMovementScripts = new MonoBehaviour[] { pc, ko };
        Debug.Log("[LevelManager] Player scripts registered: " + pc.name);
    }
}