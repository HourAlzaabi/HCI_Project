using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class DisconnectHandler : MonoBehaviourPunCallbacks
{
    [Header("Disconnect Overlay")]
    public GameObject disconnectOverlay;
    public TMP_Text disconnectMessage;

    private bool isHandlingDisconnect = false;

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (isHandlingDisconnect) return;
        isHandlingDisconnect = true;

        Debug.Log($"[DisconnectHandler] Player left: {otherPlayer.NickName}");

        // CRITICAL: clear any pause state before loading Lobby
        // This handles the case where someone leaves while the game is paused
        ClearPauseState();

        // Show overlay
        if (disconnectOverlay != null)
        {
            disconnectOverlay.SetActive(true);
            if (disconnectMessage != null)
                disconnectMessage.text =
                    otherPlayer.NickName + " disconnected. Returning to lobby...";
        }

        // Do NOT set Time.timeScale = 0 — kills Photon FixedUpdate
        Time.timeScale = 1f;

        StartCoroutine(ReturnToLobbyAfterDelay());
    }

    void ClearPauseState()
    {
        if (LevelManager.Instance == null) return;

        LevelManager.Instance.isPaused = false;
        LevelManager.Instance.pausedByActor = -1;

        // Hide pause UI on this client
        if (LevelManager.Instance.pauseOverlay != null)
            LevelManager.Instance.pauseOverlay.SetActive(false);
        if (LevelManager.Instance.pauseBlurPanel != null)
            LevelManager.Instance.pauseBlurPanel.SetActive(false);
        if (LevelManager.Instance.pauseMenu != null)
            LevelManager.Instance.pauseMenu.SetActive(false);
    }

    IEnumerator ReturnToLobbyAfterDelay()
    {
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 1f;

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Hashtable props = new Hashtable { { "ready", false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        // Use SceneManager.LoadScene, not PhotonNetwork.LoadLevel —
        // we're effectively alone, AutomaticallySyncScene has nothing to do
        SceneManager.LoadScene("Lobby");
    }

    public void OnReturnToLobby()
    {
        Time.timeScale = 1f;
        ClearPauseState();

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Hashtable props = new Hashtable { { "ready", false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        SceneManager.LoadScene("Lobby");
    }

    public void OnReturnToMainMenu()
    {
        Time.timeScale = 1f;
        GameData.SessionEndedByDisconnect = true;
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu");
    }
}