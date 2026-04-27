using UnityEngine;
using TMPro;

public class InGameTopButtons : MonoBehaviour
{
    [Header("Hint")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TMP_Text hintLabel;
    [TextArea(2, 5)]
    [SerializeField] private string hintText = "Tip goes here.";

    private void Start()
    {
        // Pre-fill the hint label with this level's text
        if (hintLabel != null)
            hintLabel.text = hintText;

        // Make sure hint panel starts hidden
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    public void HintButton()
    {
        if (hintPanel == null) return;

        if (hintLabel != null)
            hintLabel.text = hintText;

        hintPanel.SetActive(true);
    }

    public void CloseHintButton()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }
}