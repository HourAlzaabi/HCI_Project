using UnityEngine;
using UnityEngine.UI;

public class SettingsNavigateUI : MonoBehaviour
{
    public enum SettingsTab
    {
        Controls,
        Audio,
        Dialogue
    }

    [Header("Buttons")]
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button dialogueButton;

    [Header("Panels")]
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject dialoguePanel;

    [Header("Default Tab")]
    [SerializeField] private SettingsTab defaultTab = SettingsTab.Audio;

    private void Start()
    {
        controlsButton.onClick.AddListener(OpenControls);
        audioButton.onClick.AddListener(OpenAudio);
        dialogueButton.onClick.AddListener(OpenDialogue);

        OpenTab(defaultTab);
    }

    public void OpenControls()
    {
        OpenTab(SettingsTab.Controls);
    }

    public void OpenAudio()
    {
        OpenTab(SettingsTab.Audio);
    }

    public void OpenDialogue()
    {
        OpenTab(SettingsTab.Dialogue);
    }

    private void OpenTab(SettingsTab tab)
    {
        // Switch panels
        controlsPanel.SetActive(tab == SettingsTab.Controls);
        audioPanel.SetActive(tab == SettingsTab.Audio);
        dialoguePanel.SetActive(tab == SettingsTab.Dialogue);

        // Disable selected button (acts as "selected state")
        controlsButton.interactable = tab != SettingsTab.Controls;
        audioButton.interactable = tab != SettingsTab.Audio;
        dialogueButton.interactable = tab != SettingsTab.Dialogue;
    }
}