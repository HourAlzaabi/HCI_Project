using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerController : MonoBehaviourPun
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private bool wasGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (!photonView.IsMine)
        {
            // Other player — kinematic, Photon moves them
            rb.bodyType = RigidbodyType2D.Kinematic;
            return;
        }

        // Apply color chosen in lobby
        spriteRenderer.color = GameData.SelectedColor;
        photonView.RPC("SyncColor", RpcTarget.AllBuffered,
            ColorUtility.ToHtmlStringRGB(GameData.SelectedColor));

        Debug.Log("MY player spawned and ready!");
    }

    void Update()
    {
        // CRITICAL: Only control your own player
        if (!photonView.IsMine) return;

        CheckGround();
        Move();
        Jump();
    }

    void CheckGround()
    {
        if (groundCheck == null) return;
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer);
    }

    void Move()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float input = 0;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input = -1;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input = 1;

        rb.linearVelocity = new Vector2(input * moveSpeed, rb.linearVelocity.y);

        if (input > 0) spriteRenderer.flipX = false;
        else if (input < 0) spriteRenderer.flipX = true;
    }

    void Jump()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        bool jumpPressed = kb.wKey.wasPressedThisFrame
            || kb.upArrowKey.wasPressedThisFrame
            || kb.spaceKey.wasPressedThisFrame;

        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            Debug.Log("JUMPED!");
        }
    }

    [PunRPC]
    void SyncColor(string hex)
    {
        Color c;
        if (ColorUtility.TryParseHtmlString("#" + hex, out c))
            spriteRenderer.color = c;
    }
}