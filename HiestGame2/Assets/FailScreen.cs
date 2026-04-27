using UnityEngine;
using UnityEngine.UI;

public class FailScreen : MonoBehaviour
{
    [Header("Buttons")]
    public Button replayButton;
    public Button homeButton;

    [Header("Audio")]
    public AudioClip failClip;
    [Range(0f, 1f)] public float volume = 1f;

    private void OnEnable()
    {
        SetupButtons();
        PlayFailSound();
    }

    private void SetupButtons()
    {
        if (LevelManager.Instance == null) return;

        if (replayButton != null)
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(() => LevelManager.Instance.RestartLevel());
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(() => LevelManager.Instance.GoToMainMenu());
        }
    }

    private void PlayFailSound()
    {
        if (failClip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(failClip, Camera.main.transform.position, volume);
        }
    }
}