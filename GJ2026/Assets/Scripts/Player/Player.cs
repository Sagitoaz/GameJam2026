using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D _rb;
    [SerializeField] private GroundDetector _groundDetector;

    [Header("Attributes")]
    [SerializeField] private float _moveSpeed = 5f;
    public float MoveSpeed => _moveSpeed;
    [SerializeField] private float _jumpForce = 12f;

    [Header("Input System")]
    private PlayerController _playerController;
    private InputAction _move;
    private Vector2 _moveDirection;
    private InputAction _jump;
    private bool _isJumpPressed;
    public float JumpForce => _jumpForce;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerController = new PlayerController();
    }
    private void OnEnable()
    {
        _move = _playerController.Player.Move;
        _move.Enable();
        _jump = _playerController.Player.Jump;
        _jump.Enable();
    }
    private void Update()
    {
        _moveDirection = _move.ReadValue<Vector2>();

        _isJumpPressed = _jump.WasPressedThisFrame();
        
        if (_isJumpPressed && _groundDetector.IsGrounded)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
            _isJumpPressed = false;
        }
        
        _rb.linearVelocity = new Vector2(_moveDirection.x * _moveSpeed, _rb.linearVelocity.y);
    }
    private void FixedUpdate()
    {
        _groundDetector?.Check();
    }
    private void OnDisable()
    {
        _move.Disable();
        _jump.Disable();
    }
}