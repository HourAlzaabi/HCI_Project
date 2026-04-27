using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    [Header("Override (Optional)")]
    [Tooltip("Leave empty to use default click from ButtonAudioManager")]
    public AudioClip overrideClip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("If true, plays sound even when button is not interactable")]
    public bool playWhenDisabled = false;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayClickSound);
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayClickSound);
    }

    void PlayClickSound()
    {
        if (!playWhenDisabled && !button.interactable) return;

        AudioClip clipToPlay = overrideClip != null
            ? overrideClip
            : ButtonAudioManager.Instance?.defaultClickClip;

        if (clipToPlay == null) return;

        ButtonAudioManager.Instance?.PlayClick(clipToPlay, volume);
    }
}