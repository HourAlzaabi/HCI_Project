using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraZone : MonoBehaviour
{
    public string activatorTag = "Player";

    private bool triggered = false;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag(activatorTag)) return;
        if (LevelManager.Instance == null) return;
        if (LevelManager.Instance.missionEnded) return;

        triggered = true;
        LevelManager.Instance.FailMission("Detected by camera");
    }
}