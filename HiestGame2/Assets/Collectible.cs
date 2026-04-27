using UnityEngine;
using Photon.Pun;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType { Coin, Cash, Gem }
    public CollectibleType collectibleType;

    [Header("Audio")]
    public AudioClip pickupSound;

    [Header("Hover")]
    public float hoverAmplitude = 0.2f;
    public float hoverSpeed = 2f;

    private Vector3 startPos;
    private bool collected = false;
    private string uniqueId;

    private void Start()
    {
        startPos = transform.position;

        // ID uses ONLY the start position — never changes
        // Use the GameObject's scene path for guaranteed uniqueness
        uniqueId = gameObject.name
            + "_" + startPos.x.ToString("F2")
            + "_" + startPos.y.ToString("F2")
            + "_" + startPos.z.ToString("F2");

        Debug.Log("Collectible ID: " + uniqueId);
    }

    private void Update()
    {
        if (collected) return;
        float newY = startPos.y
            + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (LevelManager.Instance == null) return;

        PhotonView playerView = other.GetComponent<PhotonView>();
        if (playerView != null && !playerView.IsMine) return;

        collected = true;

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        LevelManager.Instance.CollectAndDestroy(uniqueId, (int)collectibleType);
    }
    public string GetUniqueId() => uniqueId;
    public void DestroyMe()
    {
        Destroy(gameObject);
    }
}