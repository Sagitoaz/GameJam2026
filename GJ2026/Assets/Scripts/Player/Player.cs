using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool flipSprite = true; // Tự động flip sprite theo hướng di chuyển
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 8f;
    
    [Header("References")]
    [SerializeField] private GroundDetector groundDetector;
    [SerializeField] private bool canShow;
    
    private SpriteRenderer sprite;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private PlayerVFX vfx;
    private Vector2 moveInput;
    private bool isKnockback;
    private float lastHorizontalDirection = 1f; // 1 = phải, -1 = trái, mặc định là phải
    
    public Vector2 MoveInput => moveInput;
    public float LastHorizontalDirection => lastHorizontalDirection;
    
    public PlayerState State { get; private set; } = PlayerState.Control;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        vfx = GetComponent<PlayerVFX>();
    }

    private void Start()
    {
        if (canShow) SetShow();
        else SetHide();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (State == PlayerState.Free) return;
        moveInput = context.ReadValue<Vector2>();
        
        // Lưu hướng ngang cuối cùng nếu có input ngang
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            lastHorizontalDirection = Mathf.Sign(moveInput.x);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (State == PlayerState.Free) return;
        if (context.performed && groundDetector.IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            
            // Play jump effect
            if (vfx != null)
            {
                vfx.PlayJumpEffect();
            }
        }
    }

    private void FixedUpdate()
    {
        if (State == PlayerState.Free || isKnockback) return;
        
        groundDetector.Check();
        
        // Check landing effect
        if (vfx != null)
        {
            vfx.CheckLanding(groundDetector.IsGrounded);
        }
        
        float targetSpeed = moveInput.x * moveSpeed;
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        
        // Flip sprite theo hướng di chuyển
        if (flipSprite && Mathf.Abs(moveInput.x) > 0.1f)
        {
            FlipSprite(moveInput.x > 0);
        }
    }

    private void FlipSprite(bool faceRight)
    {
        if (sprite != null)
        {
            // Flip sprite renderer (an toàn vì sprite là object con)
            sprite.flipX = !faceRight;
        }
    }
    public void ApplyKnockback(Vector2 velocity)
    {
        isKnockback = true;
        rb.linearVelocity = velocity;
        StartCoroutine(EndKnockback());
    }

    public void ApplyKnockbackNoGravity(Vector2 velocity, float duration)
    {
        StartCoroutine(KnockbackNoGravityRoutine(velocity, duration));
    }

    private IEnumerator KnockbackNoGravityRoutine(Vector2 velocity, float duration)
    {
        isKnockback = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = velocity;
        
        yield return new WaitForSeconds(duration);
        
        rb.gravityScale = originalGravity;
        isKnockback = false;
    }

    IEnumerator EndKnockback()
    {
        yield return new WaitForSeconds(0.15f);
        isKnockback = false;
    }

    public void SetHide()
    {
        State = PlayerState.Free;
        rb.simulated = false;
        col.enabled = false;
        sprite.enabled = false;
        rb.gravityScale = 0f;
        moveInput = Vector2.zero;
    }

    public void SetShow()
    {
        State = PlayerState.Control;
        rb.simulated = true;
        col.enabled = true;
        sprite.enabled = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 3f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            GameManager.Instance.TriggerEndGame();
        }
    }
}
