using UnityEngine;

public class PlatformCarry : MonoBehaviour
{
    private MovingPlatform platform;

    private void Awake()
    {
        platform = GetComponent<MovingPlatform>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        // ✅ Only parent if platform is moving
        if (platform != null && platform.IsMoving)
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        if (platform != null && platform.IsMoving)
        {
            if (collision.transform.parent != transform)
                collision.transform.SetParent(transform);
        }
        else
        {
            // 👇 Unparent if platform stopped while player is still on it
            if (collision.transform.parent == transform)
                collision.transform.SetParent(null);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        if (collision.transform.parent == transform)
            collision.transform.SetParent(null);
    }
}