using TMPro;
using UnityEngine;

public class CollectibleUI : MonoBehaviour
{
    public TMP_Text coinsText;
    public TMP_Text cashText;
    public TMP_Text gemsText;
    public TMP_Text scoreText;

    private void Update()
    {
        if (LevelManager.Instance == null) return;

        if (coinsText != null)
            coinsText.text = "Coins: " + LevelManager.Instance.coinsCollected;

        if (cashText != null)
            cashText.text = "Cash: " + LevelManager.Instance.cashCollected;

        if (gemsText != null)
            gemsText.text = "Gems: " + LevelManager.Instance.gemsCollected;

        if (scoreText != null)
            scoreText.text = "Score: " + LevelManager.Instance.GetTotalScore();
    }
}