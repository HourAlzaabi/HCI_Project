using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(AudioSource))]
public class PushableCrate : MonoBehaviour
{
    [Header("Push Sound")]
    public AudioClip pushLoopClip;
    [Range(0f, 1f)] public float pushVolume = 1f;

    [Header("Fall Impact Sound")]
    public AudioClip fallImpactClip;
    [Range(0f, 1f)] public float fallVolume = 1f;
    public float minFallVelocityForSound = 6f;
    public float fallSoundStartTime = 0.5f;

    Rigidbody2D rb;
    AudioSource audioSource;

    float lastYVelocity;
    bool wasMovingLastFrame;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
      

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = pushVolume;
    }

    void FixedUpdate()
    {
        lastYVelocity = rb.linearVelocity.y;

        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        if (pushLoopClip != null)
        {
            if (isMoving)
            {
                if (!audioSource.isPlaying || audioSource.clip != pushLoopClip)
                {
                    audioSource.clip = pushLoopClip;
                    audioSource.loop = true;
                    audioSource.volume = pushVolume;
                    audioSource.Play();
                }
            }
            else
            {
                if (audioSource.isPlaying && audioSource.clip == pushLoopClip)
                {
                    audioSource.Stop();
                }
            }
        }

        wasMovingLastFrame = isMoving;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (fallImpactClip == null) return;

        if (lastYVelocity < -minFallVelocityForSound)
        {
            if (audioSource.isPlaying && audioSource.clip == pushLoopClip)
                audioSource.Stop();

            audioSource.clip = fallImpactClip;
            audioSource.loop = false;
            audioSource.volume = fallVolume;

            float startTime = Mathf.Clamp(fallSoundStartTime, 0f, fallImpactClip.length - 0.01f);
            audioSource.time = startTime;

            audioSource.Play();
        }
    }
}