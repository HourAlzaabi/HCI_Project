using UnityEngine;
using System.Collections;

public class LaserDoor : MonoBehaviour
{
    public Transform laserVisual;
    public Collider2D laserCollider;

    public AudioSource audioSource;
    public AudioClip activeSound;
    public AudioClip disableSound;

    public float shrinkSpeed = 2f;

    private bool isDisabled = false;
    public float moveDistance = 2f;   // how far it moves left/right
    public float moveSpeed = 2f;      // how fast it moves
    private Vector3 startPos;
    void Start()
    {
        startPos = transform.position;

        if (audioSource != null && activeSound != null)
        {
            audioSource.clip = activeSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDisabled) return;

        if (other.CompareTag("Player"))
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.FailMission("Detected by laser");
            }
        }
    }

    public void DisableDoor()
    {
        StartCoroutine(DisableDoorRoutine());
    }
    void Update()
{
    if (isDisabled) return;

    float x = Mathf.PingPong(Time.time * moveSpeed, moveDistance * 2) - moveDistance;
    transform.position = new Vector3(startPos.x + x, startPos.y, startPos.z);
}
    private IEnumerator DisableDoorRoutine()
    {
        if (isDisabled) yield break;
        isDisabled = true;

        // wait before shutting down
        yield return new WaitForSeconds(1f);

        if (laserCollider != null)
            laserCollider.enabled = false;

        if (audioSource != null)
        {
            audioSource.Stop();

            if (disableSound != null)
                audioSource.PlayOneShot(disableSound);
        }

        StartCoroutine(ShrinkLaser());

        Debug.Log("Laser door disabled.");
    }
    IEnumerator ShrinkLaser()
    {
        Vector3 scale = laserVisual.localScale;

        while (scale.y > 0)
        {
            scale.y -= shrinkSpeed * Time.deltaTime;
            scale.y = Mathf.Max(scale.y, 0);
            laserVisual.localScale = scale;

            yield return null;
        }

        laserVisual.gameObject.SetActive(false);
    }
}