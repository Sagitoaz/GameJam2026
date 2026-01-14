using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private GroundDetector groundDetector;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    public PlayerState State { get;  private set; } = PlayerState.Separate;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (State == PlayerState.Merged) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (State == PlayerState.Merged) return;
        if (context.performed && groundDetector.IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void FixedUpdate()
    {
        if (State == PlayerState.Merged) return;
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        groundDetector.Check();
    }
    
    public void SetMerged()
    {
        State = PlayerState.Merged;
        // rb.linearVelocity = Vector2.zero;
        // rb.simulated = false;    
        gameObject.SetActive(false);
    }
}
