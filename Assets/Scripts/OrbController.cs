using System;
using UnityEngine;

public class OrbController : MonoBehaviour
{
    [Header("Orb Settings")]
    public float moveSpeed = 5f;
    public float fadeDuration = 5f;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;

    private Vector2 moveInput;
    private float timer;
    private bool isActive = false;

    // Player Reference
    public Transform playerTransform;

    // Dash Settings
    public float dashForce = 10f;
    public float dashDuration = 0.2f;
    private bool isDashing = false;
    private float dashTimer;

    // Event for notifying the player
    public event Action OnOrbDeactivated;

    void Start()
    {
        // Auto-assign components if not assigned
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Start inactive
        spriteRenderer.enabled = false;
        rb.simulated = false;
    }

    void Update()
    {
        if (!isActive) return;

        // Raw movement input (no smoothing)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Lifetime countdown
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            if (playerTransform != null)
                DeactivateOrb(playerTransform);
        }

        // Flip sprite
        if (moveInput.x > 0) spriteRenderer.flipX = true;
        else if (moveInput.x < 0) spriteRenderer.flipX = false;
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        // Dash logic
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;

            if (dashTimer <= 0)
            {
                isDashing = false;
                rb.linearVelocity = Vector2.zero;
            }

            return; // Skip normal movement when dashing
        }

        // Normal orb movement
        if (moveInput != Vector2.zero)
            rb.linearVelocity = moveInput.normalized * moveSpeed;
        else
            rb.linearVelocity = Vector2.zero;
    }

    // Activate Orb (called when pressing E)
    public void ActivateOrb()
    {
        isActive = true;
        timer = fadeDuration;
        spriteRenderer.enabled = true;
        rb.simulated = true;

        // Spawn orb at player
        if (playerTransform != null)
        {
            transform.position = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y,
                0f
            );
        }

        // Ensure orb is not a child
        transform.parent = null;

        // Start dash
        isDashing = true;
        dashTimer = dashDuration;

        // Dash direction (based on player facing)
        float dashDir = 1f;
        SpriteRenderer playerSprite = playerTransform?.GetComponent<SpriteRenderer>();

        if (playerSprite != null && playerSprite.flipX)
            dashDir = -1f;

        // Dash velocity
        rb.linearVelocity = new Vector2(dashDir * dashForce, 0f);
    }

    //  Return Orb to Player
    public void DeactivateOrb(Transform playerTransform)
    {
        isActive = false;
        spriteRenderer.enabled = false;
        rb.simulated = false;
        isDashing = false;

        // Move orb back to player
        if (playerTransform != null)
        {
            transform.position = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y,
                0f
            );

            transform.parent = playerTransform;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);
        }

        OnOrbDeactivated?.Invoke();
    }
}
