using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public BackgroundPlayer backgroundPlayer;
    public GameObject disconnectBanner;

    [Header("UI Elements")]
    public GameObject playButton;
    public GameObject settingsButton;
    public GameObject howToPlayButton;
    public GameObject titleText;
    public GameObject subtitleText;

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject howToPlayPanel;

    [Header("Splash")]
    public GameObject splashImage;

    [Header("Delay before showing buttons")]
    public float showUIDelay = 2f;

    void Start()
    {
        // Show splash immediately
        if (splashImage != null)
            splashImage.SetActive(true);

        // Hide all UI at start
        SetUIVisible(false);

        // Make sure sub-panels are hidden at start
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);

        if (disconnectBanner != null)
        {
            if (GameData.SessionEndedByDisconnect)
            {
                disconnectBanner.SetActive(true);
                GameData.SessionEndedByDisconnect = false;
                Invoke(nameof(HideBanner), 4f);
            }
            else
            {
                disconnectBanner.SetActive(false);
            }
        }

        StartCoroutine(ShowUIAfterDelay());
    }

    IEnumerator ShowUIAfterDelay()
    {
        yield return new WaitForSeconds(showUIDelay);

        if (splashImage != null)
            splashImage.SetActive(false);

        SetUIVisible(true);
    }

    void SetUIVisible(bool visible)
    {
        if (playButton != null) playButton.SetActive(visible);
        if (settingsButton != null) settingsButton.SetActive(visible);
        if (howToPlayButton != null) howToPlayButton.SetActive(visible);
        if (titleText != null) titleText.SetActive(visible);
        if (subtitleText != null) subtitleText.SetActive(visible);
    }

    void HideBanner()
    {
        if (disconnectBanner != null)
            disconnectBanner.SetActive(false);
    }

    // ===== MAIN BUTTONS =====

    public void OnPlayClicked()
    {
        SetUIVisible(false);
        if (backgroundPlayer != null)
            backgroundPlayer.PlayExitThen(() =>
                SceneManager.LoadScene("SessionCreation"));
        else
            SceneManager.LoadScene("SessionCreation");
    }

    public void OnSettingsClicked()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    public void OnHowToPlayClicked()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
    }

    // Called by the Back button in Settings or HowToPlay panels
    public void OnBackToMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }
}