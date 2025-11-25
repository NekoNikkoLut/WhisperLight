using Mono.Cecil.Cil;
using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Jump High Limit Settings")]
    private float jumpStartY;
    public float maxJumpHeight = 3.5f;



    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Jump Physics")]
    public float fallMultiplier = 3.8f;
    public float lowJumpMultiplier = 3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Orb Settings")]
    public OrbController orbController; // Drag your Orb here in Inspector
    private bool orbActive = false;    // Track if orb is active

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;


    [Header("Horizontal Physics")]
    public float acceleration = 20f;
    public float deceleration = 18f;
    public float velPower = 0.9f; // smoother control

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Subscribe to orb deactivation
        if (orbController != null)
            orbController.OnOrbDeactivated += ResetOrbActive;
    }

    void Update()
    {
        float move = 0f;

        // Only allow horizontal movement if orb is NOT active
        if (!orbActive)
            move = Input.GetAxisRaw("Horizontal");

        float targetSpeed = move * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, velPower) * Mathf.Sign(speedDiff);

        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Flip the sprite only if moving
        if (move > 0)
            spriteRenderer.flipX = false;
        else if (move < 0)
            spriteRenderer.flipX = true;

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Jumping (only if orb not active)
        if (!orbActive && ((isGrounded && Input.GetKeyDown(KeyCode.W)) || (isGrounded && Input.GetKeyDown(KeyCode.Space))))
        {

            jumpStartY = transform.position.y;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // Improved jump feel (like Hollow Knight)
        if (!orbActive)
        {
            if (rb.linearVelocity.y < 0)
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier) * Time.deltaTime;
            else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.Space))
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // Jump Settings

        if (!isGrounded && rb.linearVelocity.y > 0)
        {
            float jumpHeight = transform.position.y - jumpStartY;

            if (jumpHeight >= maxJumpHeight)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // CUT the jump
            }
        }

        // --- ORB CONTROLS (E/R) ---

        if (Input.GetKeyDown(KeyCode.E) && !orbActive && orbController != null)
        {
            orbActive = true;
            orbController.ActivateOrb();
            Debug.Log(orbController);
            Debug.Log(orbActive);
        }
        else if (Input.GetKeyDown(KeyCode.E) && orbActive && orbController != null)
        {
            orbActive = false;
            orbController.DeactivateOrb(transform);
            Debug.Log(orbController);
            Debug.Log(orbActive);
        }
        
        
        
    }

    

    void ResetOrbActive()
    {
        orbActive = false; // allows next split
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the ground check area
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
