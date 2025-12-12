using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private bool IsMouseClick()
    {
        return Input.GetMouseButtonDown(0) ||
            Input.GetMouseButtonDown(1) ||
            Input.GetMouseButtonDown(2);
    }

    private void Update()
    {
        
        // Read input
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right

        // Notify GameManager when player starts moving and tick timer while moving
        if (Input.anyKeyDown && !IsMouseClick())
        {
            GameManager.Instance?.SetPlayerMoving();
            
        }

        GameManager.Instance?.TimeStart();

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameManager.Instance.currentTime = GameManager.Instance.stageTime;
            GameManager.Instance.UpdateTimerText();
            GameManager.Instance.OnPlayerDeath();
            GameManager.Instance.RespawnPlayer();
            GameManager.Instance.ResetTimer();
        }

        // Flip sprite according to movement direction (optional)
        if (spriteRenderer != null)
        {
            if (horizontalInput > 0f) spriteRenderer.flipX = false;
            else if (horizontalInput < 0f) spriteRenderer.flipX = true;
        }
    }

    private void FixedUpdate()
    {
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        if (groundCheck != null)
        {
            Vector2 size = new Vector2(groundCheckRadius * 8f, groundCheckRadius * 2f);
            return Physics2D.OverlapBox(groundCheck.position, size, 0f, groundLayer) != null;
        }

        if (rb != null)
            return rb.IsTouchingLayers(groundLayer);

        return false;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;

            Vector2 size = new Vector2(groundCheckRadius * 8f, groundCheckRadius * 0.5f);
            Gizmos.DrawWireCube(groundCheck.position, size);
        }
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Dead"))
        {
            Debug.Log("Player Dead Triggered");
            GameManager.Instance.currentTime = GameManager.Instance.stageTime;
            GameManager.Instance.UpdateTimerText();
            GameManager.Instance.OnPlayerDeath(false);
            GameManager.Instance.RespawnPlayer(true);
            GameManager.Instance.ResetTimer();
        }
    }
}
