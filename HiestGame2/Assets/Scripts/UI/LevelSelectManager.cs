using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class LevelSelectManager : MonoBehaviourPun
{
    [Header("Level Buttons (in order)")]
    public LevelButton[] levelButtons;

    [Header("Carousel Slots")]
    public RectTransform leftSlot;
    public RectTransform centerSlot;
    public RectTransform rightSlot;
    public Transform offscreenStash;

    [Header("Slot Visuals")]
    public float sideScale = 0.7f;
    public float sideAlpha = 0.5f;

    [Header("Carousel Controls")]
    public Button leftArrowButton;
    public Button rightArrowButton;
    public Button playButton;

    [Header("Joiner Overlay")]
    public GameObject joinerOverlay;
    public TMP_Text joinerOverlayText;

    [Header("Back")]
    public Button backButton;
    public BackgroundPlayer backgroundPlayer;

    private int currentIndex = 0;

    void Start()
    {
        // Stash all panels off-screen first
        foreach (LevelButton lb in levelButtons)
        {
            if (lb != null && offscreenStash != null)
                lb.transform.SetParent(offscreenStash, false);
        }

        // Wire arrow buttons
        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(OnLeftArrow);
        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(OnRightArrow);

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        // Joiner sees overlay, host doesn't
        bool isHost = !PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient;
        if (joinerOverlay != null)
            joinerOverlay.SetActive(!isHost);
        if (joinerOverlayText != null)
            joinerOverlayText.text = "Host is choosing the level...";

        // Joiner controls disabled
        SetHostControlsInteractable(isHost);

        // Hook up side-panel click-to-jump
        // We rely on each LevelButton having a Button. We'll add a wrapper listener.
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int capturedIndex = i;
            LevelButton lb = levelButtons[i];
            if (lb != null && lb.levelButton != null)
            {
                // Remove existing listeners that would call LoadLevel directly,
                // we route through our manager instead
                lb.levelButton.onClick.RemoveAllListeners();
                lb.levelButton.onClick.AddListener(() => OnPanelClicked(capturedIndex));
            }
        }

        // Default to first unlocked level
        currentIndex = FindFirstUnlocked();
        ApplyIndex(currentIndex);
    }

    int FindFirstUnlocked()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] != null && levelButtons[i].IsUnlocked())
                return i;
        }
        return 0;
    }

    void SetHostControlsInteractable(bool state)
    {
        if (leftArrowButton != null) leftArrowButton.interactable = state;
        if (rightArrowButton != null) rightArrowButton.interactable = state;
        if (playButton != null) playButton.interactable = state;
        // Level button clicks (side-jump) only enabled for host too
        foreach (LevelButton lb in levelButtons)
        {
            if (lb != null && lb.levelButton != null)
                lb.levelButton.interactable = state;
        }
    }

    // ===========================================================
    // HOST INPUT HANDLERS
    // ===========================================================

    void OnLeftArrow()
    {
        if (!IsHost()) return;
        if (currentIndex <= 0) return;
        ChangeIndex(currentIndex - 1);
    }

    void OnRightArrow()
    {
        if (!IsHost()) return;
        if (currentIndex >= levelButtons.Length - 1) return;
        ChangeIndex(currentIndex + 1);
    }

    void OnPanelClicked(int clickedIndex)
    {
        if (!IsHost()) return;
        if (clickedIndex == currentIndex)
        {
            // Already centered, do nothing — Play button handles launch
            return;
        }
        ChangeIndex(clickedIndex);
    }

    void OnPlayClicked()
    {
        if (!IsHost()) return;
        LevelButton current = levelButtons[currentIndex];
        if (current == null) return;
        if (!current.IsUnlocked())
        {
            Debug.Log("[LevelSelect] Level is locked.");
            return;
        }
        current.LoadLevel();
    }

    void OnBackClicked()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Lobby");
        else if (!PhotonNetwork.IsConnected)
            SceneManager.LoadScene("Lobby");
    }

    bool IsHost()
    {
        return !PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient;
    }

    // ===========================================================
    // INDEX CHANGE (networked)
    // ===========================================================

    void ChangeIndex(int newIndex)
    {
        if (PhotonNetwork.IsConnected && photonView != null)
        {
            photonView.RPC(nameof(RPC_SyncIndex), RpcTarget.All, newIndex);
            PhotonNetwork.SendAllOutgoingCommands();
        }
        else
        {
            ApplyIndex(newIndex);
        }
    }

    [PunRPC]
    void RPC_SyncIndex(int newIndex)
    {
        ApplyIndex(newIndex);
    }

    void ApplyIndex(int newIndex)
    {
        currentIndex = Mathf.Clamp(newIndex, 0, levelButtons.Length - 1);
        RepositionPanels();
        UpdateArrowsInteractable();
        UpdatePlayButtonInteractable();
    }

    // ===========================================================
    // VISUAL LAYOUT
    // ===========================================================

    void RepositionPanels()
    {
        // Stash everyone first
        foreach (LevelButton lb in levelButtons)
        {
            if (lb != null && offscreenStash != null)
                lb.transform.SetParent(offscreenStash, false);
        }

        // Place center
        if (currentIndex >= 0 && currentIndex < levelButtons.Length)
            PlacePanel(levelButtons[currentIndex], centerSlot, 1f, 1f);

        // Place left
        int leftIdx = currentIndex - 1;
        if (leftIdx >= 0 && leftIdx < levelButtons.Length)
            PlacePanel(levelButtons[leftIdx], leftSlot, sideScale, sideAlpha);

        // Place right
        int rightIdx = currentIndex + 1;
        if (rightIdx >= 0 && rightIdx < levelButtons.Length)
            PlacePanel(levelButtons[rightIdx], rightSlot, sideScale, sideAlpha);

        // Refresh each LevelButton's stars/lock display
        foreach (LevelButton lb in levelButtons)
            if (lb != null) lb.Refresh();
    }

    void PlacePanel(LevelButton lb, RectTransform slot, float scale, float alpha)
    {
        if (lb == null || slot == null) return;

        lb.transform.SetParent(slot, false);
        RectTransform rt = lb.transform as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one * scale;
        }

        // Apply alpha via CanvasGroup
        CanvasGroup cg = lb.GetComponent<CanvasGroup>();
        if (cg == null) cg = lb.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = alpha;
        cg.blocksRaycasts = true; // still clickable for jump-to-here
    }

    void UpdateArrowsInteractable()
    {
        if (!IsHost()) return; // joiner stays disabled
        if (leftArrowButton != null)
            leftArrowButton.interactable = currentIndex > 0;
        if (rightArrowButton != null)
            rightArrowButton.interactable = currentIndex < levelButtons.Length - 1;
    }

    void UpdatePlayButtonInteractable()
    {
        if (!IsHost()) return;
        if (playButton == null) return;
        LevelButton current = levelButtons[currentIndex];
        bool playable = current != null && current.IsUnlocked();
        playButton.interactable = playable;
    }
}