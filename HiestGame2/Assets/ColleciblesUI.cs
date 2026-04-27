using UnityEngine;
using TMPro;

public class CollectiblesUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private void OnEnable()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnCollectiblesChanged += UpdateScore;

        UpdateScore();
    }

    private void OnDisable()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnCollectiblesChanged -= UpdateScore;
    }

    public void UpdateScore()
    {
        if (LevelManager.Instance == null || scoreText == null) return;

        scoreText.text = LevelManager.Instance
            .GetCollectibleScore().ToString();
    }
}