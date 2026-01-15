using System.Collections;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;

class PlayerMergeSplitController : MonoBehaviour
{
    [SerializeField] private Player _player1;
    [SerializeField] private Player _player2;
    [SerializeField] private Player _playerVertical;
    [SerializeField] private float _mergeSpeed = 5f;
    private float _splitForceP1 = 10f;
    private float _splitForceP2 = 50f;

    private BoxCollider2D col1, col3;
    private Rigidbody2D rb1, rb2;
    enum MergeState { Separate, Merging, Merged }
    MergeState _state = MergeState.Separate;

    private bool _isP1Left = true;

    private void Awake()
    {
        col1 = _player1.GetComponent<BoxCollider2D>();
        col3 = _playerVertical.GetComponent<BoxCollider2D>();
        rb1 = _player1.GetComponent<Rigidbody2D>();
        rb2 = _player2.GetComponent<Rigidbody2D>();
    }
    public void OnMerge(InputAction.CallbackContext context)
    {
        Debug.Log(_state.ToString());
        if (!context.performed) return;
        if (_state == MergeState.Separate)
        {
            _state = MergeState.Merging;
            StartCoroutine(MergeRoutine());
        }
        else if(_state == MergeState.Merged)
        {
            _state = MergeState.Separate;
            Split();
        }
    }

    IEnumerator MergeRoutine()
    {
        Vector3 midpoint = (_player1.transform.position + _player2.transform.position) / 2f;
        if (_player1.transform.position.x > _player2.transform.position.x)
        {
            _isP1Left = false;
        }
        rb1.gravityScale = 0;
        rb2.gravityScale = 0;
        while (Vector2.Distance(_player1.transform.position, midpoint) > col1.size.x * 0.6f)
        {
            _player1.transform.position = Vector3.Lerp(_player1.transform.position, midpoint, _mergeSpeed * Time.deltaTime);
            _player2.transform.position = Vector3.Lerp(_player2.transform.position, midpoint, _mergeSpeed * Time.deltaTime);
            yield return null;
        }
        _player2.SetHide();
        _player1.SetHide();

        _playerVertical.SetShow();
        _playerVertical.transform.position = midpoint;
        _state = MergeState.Merged;
    }

    private void Split()
    {
        Vector3 p3Pos = _playerVertical.transform.position;
        _playerVertical.SetHide();
        _player1.SetShow();
        _player2.SetShow();

        float halfW1 = col1.size.x * 0.5f;
        float halfW3 = col3.size.x * 0.5f;

        float gap = 0.02f;
        float offset = halfW3 + halfW1 + gap;

        Vector3 p1Pos = p3Pos + (_isP1Left ? Vector3.left : Vector3.right) * offset;
        Vector3 p2Pos = p3Pos + (_isP1Left ? Vector3.right : Vector3.left) * offset;

        _player1.transform.position = p1Pos;
        _player2.transform.position = p2Pos;

        Vector2 p1Dir = _isP1Left ? Vector2.left : Vector2.right;
        Vector2 p2Dir = -p1Dir;

        _player1.ApplyKnockback(p1Dir * _splitForceP1);
        _player2.ApplyKnockback(p2Dir * _splitForceP2);

        StartCoroutine(DampVelocity(rb1));
        StartCoroutine(DampVelocity(rb2));
    }

    IEnumerator DampVelocity(Rigidbody2D rb)
    {
        Debug.Log("fire");
        while (rb.linearVelocity.magnitude > 0.05f)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime);
            yield return null;
        }
        rb.linearVelocity = Vector2.zero;
    }
}