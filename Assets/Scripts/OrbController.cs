using System;
using UnityEngine;

public class OrbController : MonoBehaviour
{
    [Header("Orb Settings")]
    public float moveSpeed = 5f;
    public float fadeDuration = 5f;
    public Rigidbody2D rb;               // Drag orb Rigidbody here
    public SpriteRenderer spriteRenderer; // Drag orb sprite here

    private Vector2 moveInput;
    private float timer;

    // Event for notifying player when orb returns
    public event Action OnOrbDeactivated;

    private bool isActive = false;

    // Player Reference
    public Transform playerTransform;

    // Dash Settings
    public float dashForce = 10f;
    public float dashDuration = 0.2f;
    private bool isDashing = false;
    private float dashTimer;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Start inactive
        spriteRenderer.enabled = false;
        rb.simulated = false;
    }

    void Update()
    {
        if (!isActive) return;

        // Get input immediately (no smoothing)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Countdown for fade time
        if (timer > 0)
            timer -= Time.deltaTime;
        else
        {
            if (playerTransform != null)
                DeactivateOrb(playerTransform);
        }

        // Flip sprite when moving left/right
        if (moveInput.x > 0)
            spriteRenderer.flipX = true;
        else if (moveInput.x < 0)
            spriteRenderer.flipX = false;
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        // Dash movement
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                rb.linearVelocity = Vector2.zero; // stop dash
            }
            return; // skip normal control while dashing
        }

        // Regular orb movement
        if (moveInput.magnitude > 0)
        {
            rb.linearVelocity = moveInput.normalized * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // stop instantly when no input
        }
    }

    // Called by Player when pressing E
    public void ActivateOrb()
    {
        isActive = true;
        timer = fadeDuration;
        spriteRenderer.enabled = true;
        rb.simulated = true;

        // Reset position to player
        if (playerTransform != null)
            transform.position = playerTransform.position;

        // Detach from player
        transform.parent = null;

        // Dash effect
        isDashing = true;
        dashTimer = dashDuration;

        float dashDir = 1f; // default right
        if (playerTransform != null)
        {
            SpriteRenderer playerSprite = playerTransform.GetComponent<SpriteRenderer>();
            if (playerSprite != null && playerSprite.flipX)
                dashDir = -1f;
        }

        rb.linearVelocity = new Vector2(dashDir * dashForce, 0);
    }

    // Called by Player when pressing R or auto-return
    public void DeactivateOrb(Transform playerTransform)
    {
        isActive = false;
        spriteRenderer.enabled = false;
        rb.simulated = false;
        isDashing = false;

        // Return orb to player
        if (playerTransform != null)
        {
            transform.position = playerTransform.position;
            transform.parent = playerTransform;
        }

        OnOrbDeactivated?.Invoke();
    }
}

