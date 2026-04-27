using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [Header("Level")]
    public int levelNumber;
    public string sceneName;

    [Header("Development Status")]
    [Tooltip("If checked, this level is permanently inactive (under development)")]
    public bool underDevelopment = false;

    [Tooltip("Alpha for under-development levels (0=invisible, 1=normal)")]
    [Range(0f, 1f)]
    public float underDevelopmentAlpha = 0.4f;

    [Header("Button")]
    public Button levelButton;

    [Header("Stars")]
    public Image[] stars;
    public Sprite activeStarSprite;
    public Sprite inactiveStarSprite;

    [Header("Lock")]
    public GameObject lockIcon;

    [Header("Debug")]
    public int currentStarCount;
    public int previousLevelStarCount;
    public bool unlocked;

    private bool isLoading = false;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Need a CanvasGroup to control alpha + interactability cleanly
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        Refresh();
        SetInteractability();
    }

    private void OnEnable()
    {
        Refresh();
        SetInteractability();
    }

    void SetInteractability()
    {
        // Under-development levels are NEVER interactable
        if (underDevelopment)
        {
            if (levelButton != null)
                levelButton.interactable = false;
            return;
        }

        // Only host can click level buttons
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            if (levelButton != null)
                levelButton.interactable = false;
        }
    }

    public void Refresh()
    {
        UpdateStars();
        UpdateLockState();
        UpdateAppearance();
    }

    public void LoadLevel()
    {
        // Under-development levels can NEVER load
        if (underDevelopment)
        {
            Debug.Log("Level " + levelNumber + " is under development.");
            return;
        }

        // Only host can load
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Only host can start the level!");
            return;
        }

        if (isLoading) return;
        if (!IsUnlocked()) return;

        isLoading = true;
        if (levelButton != null)
            levelButton.interactable = false;

        Time.timeScale = 1f;
        GameData.SelectedLevel = sceneName;

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Host loading level: " + sceneName);
            PhotonNetwork.LoadLevel(sceneName);
        }
        else if (!PhotonNetwork.IsConnected)
        {
            if (SceneTransition.Instance != null)
                SceneTransition.Instance.LoadSceneWithTransition(sceneName);
            else
                SceneManager.LoadScene(sceneName);
        }
    }

    public void UpdateStars()
    {
        currentStarCount = PlayerPrefs.GetInt(GetStarsKey(levelNumber), 0);
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
                stars[i].sprite = (i < currentStarCount)
                    ? activeStarSprite
                    : inactiveStarSprite;
        }
    }

    public void UpdateLockState()
    {
        unlocked = IsUnlocked();

        if (levelButton != null)
        {
            // Under-dev or non-host client - never interactable
            if (underDevelopment)
            {
                levelButton.interactable = false;
            }
            else if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            {
                levelButton.interactable = false;
            }
            else
            {
                levelButton.interactable = unlocked && !isLoading;
            }
        }

        // Hide lock icon for under-development levels (don't show lock at all)
        if (lockIcon != null)
        {
            if (underDevelopment)
                lockIcon.SetActive(false);
            else
                lockIcon.SetActive(!unlocked);
        }
    }

    void UpdateAppearance()
    {
        if (canvasGroup == null) return;

        // Under-development levels are translucent and non-interactable
        if (underDevelopment)
        {
            canvasGroup.alpha = underDevelopmentAlpha;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public bool IsUnlocked()
    {
        // Under-development levels are NEVER unlocked
        if (underDevelopment) return false;

        if (levelNumber == 1)
        {
            previousLevelStarCount = -1;
            return true;
        }

        previousLevelStarCount = PlayerPrefs.GetInt(
            GetStarsKey(levelNumber - 1), 0);
        return previousLevelStarCount >= 2;
    }

    private string GetStarsKey(int level)
    {
        return "Level" + level + "_stars";
    }
}