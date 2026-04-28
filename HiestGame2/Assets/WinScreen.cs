using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinScreen : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text timeText;
    public TMP_Text starsText;
    public TMP_Text gemsText;

    [Header("Stars")]
    public Image[] starImages;
    public Sprite fullStar;
    public Sprite emptyStar;

    [Header("Audio")]
    public AudioClip showClip;
    [Range(0f, 1f)] public float volume = 1f;
    private AudioSource audioSource;

    [Header("Buttons")]
    public Button replayButton;
    public Button nextLevelButton;
    public Button mainMenuButton;

    [Header("Optional")]
    public TMP_Text nextLevelLockedText;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        PlayShowSound();
        Refresh();
    }

    private void PlayShowSound()
    {
        if (showClip != null && audioSource != null)
            audioSource.PlayOneShot(showClip, volume);
    }

    public void Refresh()
    {
        if (LevelManager.Instance == null) return;

        var lm = LevelManager.Instance;

        UpdateTime(lm.levelTimer);
        UpdateStars(lm.earnedStars);
        UpdateGems(lm.GetCollectibleScore());
        UpdateButtonStates(lm.earnedStars);
    }

    private void UpdateTime(float timeSeconds)
    {
        int minutes = Mathf.FloorToInt(timeSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeSeconds % 60f);

        if (timeText != null)
            timeText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private void UpdateStars(int starCount)
    {
        if (starsText != null)
            starsText.text = starCount + "/3";

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            starImages[i].sprite = (i < starCount) ? fullStar : emptyStar;
        }
    }

    private void UpdateGems(int gems)
    {
        if (gemsText != null)
            gemsText.text = gems.ToString();
    }

    // Only updates interactability — does NOT wire OnClick (Inspector handles that)
    private void UpdateButtonStates(int starCount)
    {
        if (nextLevelButton != null)
        {
            bool canGoNext = starCount >= 2;
            nextLevelButton.interactable = canGoNext;
        }

        if (nextLevelLockedText != null)
            nextLevelLockedText.gameObject.SetActive(starCount < 2);
    }
}