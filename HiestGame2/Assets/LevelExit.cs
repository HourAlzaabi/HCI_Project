using UnityEngine;

public class LevelExit : MonoBehaviour
{
    public string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CompleteMission();
        }
    }
}