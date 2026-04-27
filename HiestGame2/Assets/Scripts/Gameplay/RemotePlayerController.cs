using UnityEngine;

public class RemotePlayerController : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer sr;

    private Vector3 lastPosition;
    private float smoothedSpeedX;
    private float speedXVelocity;

    // Track jump state separately from position
    private float airTime = 0f;
    private bool isGrounded = true;
    private float groundedThreshold = 0.12f;

    void Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;

        if (sr != null)
            sr.enabled = true;
    }

    void Update()
    {
        if (animator == null) return;

        Vector3 currentPos = transform.position;
        Vector3 delta = currentPos - lastPosition;
        lastPosition = currentPos;

        float rawSpeedX = Mathf.Abs(delta.x) / Time.deltaTime;
        float rawSpeedY = delta.y / Time.deltaTime;

        // When stopping snap to 0 quickly
        // When starting smooth more gradually
        float smoothTime = rawSpeedX < smoothedSpeedX ? 0.05f : 0.12f;

        smoothedSpeedX = Mathf.SmoothDamp(
            smoothedSpeedX,
            rawSpeedX,
            ref speedXVelocity,
            smoothTime);

        // Hard snap to 0 when very slow to prevent
        // animation lingering after stop
        if (smoothedSpeedX < 0.35f)
            smoothedSpeedX = 0f;

        // Grounded detection using airTime
        // Once in air stay in air for minimum time
        // to prevent glitch at jump peak
        if (rawSpeedY > 0.5f || rawSpeedY < -0.9f)
        {
            // Clearly moving vertically - in air
            airTime += Time.deltaTime;
        }
        else
        {
            // Not moving vertically much
            if (airTime > 0f)
                airTime -= Time.deltaTime * 3f;
            if (airTime < 0f)
                airTime = 0f;
        }

        // Only consider grounded if airTime has
        // fully drained - prevents peak glitch
        isGrounded = airTime <= 0f;

        float normalizedSpeed = Mathf.Clamp01(smoothedSpeedX / 7f);

        animator.SetBool("grounded", isGrounded);
        animator.SetFloat("velocityX", normalizedSpeed);

        // Flip sprite
        if (sr != null)
        {
            if (delta.x > 0.001f) sr.flipX = false;
            else if (delta.x < -0.001f) sr.flipX = true;
        }
    }
}