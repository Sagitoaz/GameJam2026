using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private GroundDetector groundDetector;
    [SerializeField] private bool canShow;
    private SpriteRenderer sprite;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private Vector2 moveInput;
    private bool isKnockback;
    public PlayerState State { get;  private set; } = PlayerState.Control;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (canShow) SetShow();
        else SetHide();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (State == PlayerState.Stay) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (State == PlayerState.Stay) return;
        if (context.performed && groundDetector.IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void FixedUpdate()
    {
        if (State == PlayerState.Stay || isKnockback) return;
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        groundDetector.Check();
    }

    public void ApplyKnockback(Vector2 velocity)
    {
        isKnockback = true;
        rb.linearVelocity = velocity;
        StartCoroutine(EndKnockback());
    }

    IEnumerator EndKnockback()
    {
        yield return new WaitForSeconds(0.15f);
        isKnockback = false;
    }

    public void SetHide()
    {
        State = PlayerState.Stay;
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
        Debug.Log("show " + gameObject.name);
    }
}
