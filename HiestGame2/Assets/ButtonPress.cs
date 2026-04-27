using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(AudioSource))]
public class ButtonPress : MonoBehaviour
{
    [Header("Who can press")]
    public string activatorTag = "Player";

    [Header("Visual")]
    public Vector2 pressedOffset = new Vector2(0f, -0.08f);
    public float animSpeed = 10f;
    public Color normalColor = Color.white;
    public Color pressedColor = Color.grey;

    [Header("Button Sounds")]
    public AudioClip pressClip;
    public AudioClip releaseClip; // optional (auto reverse if empty)
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Door (optional)")]
    public Transform doorTransform;
    public Vector3 doorOpenOffset = new Vector3(0f, 2f, 0f);
    public float doorSpeed = 6f;
    public AudioClip doorOpenClip;
    public AudioClip doorCloseClip; // optional (auto reverse if empty)
    [Range(0f, 1f)] public float doorVolume = 1f;
    public bool disableDoorColliderWhenOpen = true;

    [Header("Platform (optional)")]
    public MovingPlatform platform;

    AudioSource audioSource;
    SpriteRenderer sr;
    Vector3 visualStart;
    Vector3 visualTarget;

    bool isHeld = false;
    int overlapCount = 0;

    Vector3 doorClosedLocal;
    Vector3 doorOpenLocal;

    AudioClip reversedDoorCloseClip;
    AudioClip reversedReleaseClip;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        sr = GetComponent<SpriteRenderer>();

        visualStart = transform.localPosition;
        visualTarget = visualStart;

        if (sr != null) sr.color = normalColor;

        if (doorTransform != null)
        {
            doorClosedLocal = doorTransform.localPosition;
            doorOpenLocal = doorClosedLocal + doorOpenOffset;
        }

        // Auto-create reversed door close clip
        if (doorOpenClip != null && doorCloseClip == null)
        {
            reversedDoorCloseClip = CreateReversedClip(doorOpenClip);
        }

        // Auto-create reversed release clip
        if (pressClip != null && releaseClip == null)
        {
            reversedReleaseClip = CreateReversedClip(pressClip);
        }
    }

    void Update()
    {
        // Button visual animation
        transform.localPosition =
            Vector3.Lerp(transform.localPosition, visualTarget, Time.deltaTime * animSpeed);

        if (sr != null)
        {
            Color targetColor = isHeld ? pressedColor : normalColor;
            sr.color = Color.Lerp(sr.color, targetColor, Time.deltaTime * animSpeed);
        }

        // Door movement
        if (doorTransform != null)
        {
            Vector3 target = isHeld ? doorOpenLocal : doorClosedLocal;
            doorTransform.localPosition =
                Vector3.Lerp(doorTransform.localPosition, target, Time.deltaTime * doorSpeed);

            if (disableDoorColliderWhenOpen)
            {
                var col = doorTransform.GetComponent<Collider2D>();
                if (col != null)
                    col.enabled = !isHeld;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(activatorTag)) return;

        overlapCount++;
        if (!isHeld)
            SetHeld(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(activatorTag)) return;

        overlapCount = Mathf.Max(0, overlapCount - 1);
        if (overlapCount == 0)
            SetHeld(false);
    }

    void SetHeld(bool held)
    {
        if (held == isHeld) return;

        isHeld = held;
        visualTarget = visualStart + (held ? (Vector3)pressedOffset : Vector3.zero);

        if (held)
        {
            if (pressClip)
                audioSource.PlayOneShot(pressClip, volume);

            // 🔒 ONLY play door sound if NO platform
            if (platform == null)
            {
                if (doorOpenClip)
                    audioSource.PlayOneShot(doorOpenClip, doorVolume);
            }
        }
        else
        {
            if (releaseClip)
                audioSource.PlayOneShot(releaseClip, volume);
            else if (reversedReleaseClip)
                audioSource.PlayOneShot(reversedReleaseClip, volume);

            // 🔒 ONLY play door sound if NO platform
            if (platform == null)
            {
                if (doorCloseClip)
                    audioSource.PlayOneShot(doorCloseClip, doorVolume);
                else if (reversedDoorCloseClip)
                    audioSource.PlayOneShot(reversedDoorCloseClip, doorVolume);
            }
        }

        if (platform != null && held)
            platform.SetActive(true);
    }

    // -------- Reverse Audio Helper --------
    AudioClip CreateReversedClip(AudioClip original)
    {
        float[] data = new float[original.samples * original.channels];
        original.GetData(data, 0);
        System.Array.Reverse(data);

        AudioClip reversed = AudioClip.Create(
            original.name + "_reversed",
            original.samples,
            original.channels,
            original.frequency,
            false
        );

        reversed.SetData(data, 0);
        return reversed;
    }
}