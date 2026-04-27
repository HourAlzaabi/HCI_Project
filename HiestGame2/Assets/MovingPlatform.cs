using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Two-point path")]
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float arriveThreshold = 0.02f;

    [Header("State")]
    public bool startAtA = true;
    public bool IsMoving { get; private set; }

    [Header("Sound (optional)")]
    public AudioClip moveLoopClip;
    [Range(0f, 1f)] public float moveVolume = 1f;
    public float soundStartTime = 0f;

    private Rigidbody2D rb;
    private AudioSource audioSource;

    private Vector2 currentTarget;
    private bool atA = true;
    private bool active = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError(name + ": Assign both pointA and pointB.");
            enabled = false;
            return;
        }

        atA = startAtA;
        rb.position = atA ? (Vector2)pointA.position : (Vector2)pointB.position;
        currentTarget = rb.position;
        IsMoving = false;
        active = false;
    }

    public void SetActive(bool isActive)
    {
        if (!isActive) return;
        if (IsMoving) return;
        if (pointA == null || pointB == null) return;

        currentTarget = atA ? (Vector2)pointB.position : (Vector2)pointA.position;
        active = true;
        IsMoving = true;

        if (moveLoopClip != null)
        {
            audioSource.clip = moveLoopClip;
            audioSource.loop = true;
            audioSource.volume = moveVolume;
            audioSource.time = Mathf.Clamp(soundStartTime, 0f, moveLoopClip.length - 0.01f);
            audioSource.Play();
        }
    }

    void FixedUpdate()
    {
        if (!active)
        {
            IsMoving = false;
            return;
        }

        Vector2 pos = rb.position;
        Vector2 next = Vector2.MoveTowards(pos, currentTarget, speed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        if (Vector2.Distance(next, currentTarget) <= arriveThreshold)
        {
            rb.MovePosition(currentTarget);

            atA = currentTarget == (Vector2)pointA.position;
            active = false;
            IsMoving = false;

            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA.position, pointB.position);
        Gizmos.DrawSphere(pointA.position, 0.1f);
        Gizmos.DrawSphere(pointB.position, 0.1f);
    }
#endif
}