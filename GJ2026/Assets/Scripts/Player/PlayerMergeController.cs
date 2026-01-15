using System.Collections;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;

class PlayerMergeSplitController : MonoBehaviour
{
    [SerializeField] private Player _player1;
    [SerializeField] private Player _player2;
    [SerializeField] private Player _playerVertical;
    [SerializeField] private Player _playerHorizontal;
    private float _mergeSpeed = 5f;
    private float _splitForceP1 = 10f;
    private float _splitForceP2 = 50f;

    private BoxCollider2D col1, col2, col3, col4;
    private Rigidbody2D rb1, rb2;
    MergeState _state = MergeState.Separate;

    private bool _isP1Left = true;

    private void Awake()
    {
        col1 = _player1.GetComponent<BoxCollider2D>();
        col2 = _player2.GetComponent<BoxCollider2D>();
        col3 = _playerVertical.GetComponent<BoxCollider2D>();
        col4 = _playerHorizontal.GetComponent<BoxCollider2D>();
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
            GameManager.Instance.playerMode = PlayerMode.None;
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

        bool isHorizontal = Mathf.Abs(
            _player1.transform.position.x - _player2.transform.position.x
        ) >
        Mathf.Abs(
            _player1.transform.position.y - _player2.transform.position.y
        );

        // xác định hướng P1
        _isP1Left = isHorizontal
            ? _player1.transform.position.x < _player2.transform.position.x
            : _player1.transform.position.y < _player2.transform.position.y;

        rb1.simulated = false;
        rb2.simulated = false;
        col1.enabled = false;
        col2.enabled = false;

        float targetDist = isHorizontal
            ? col1.size.x * 1.2f
            : col1.size.y * 1.2f;

        while (Vector2.Distance(_player1.transform.position, midpoint) > targetDist)
        {
            _player1.transform.position =
                Vector3.Lerp(_player1.transform.position, midpoint, _mergeSpeed * Time.deltaTime);

            _player2.transform.position =
                Vector3.Lerp(_player2.transform.position, midpoint, _mergeSpeed * Time.deltaTime);

            yield return null;
        }

        _player1.SetHide();
        _player2.SetHide();

        GameManager.Instance.playerMode = PlayerMode.Horizontal;
        GameManager.Instance.currentGameMode = _playerHorizontal;

        GameManager.Instance.currentGameMode.SetShow();
        GameManager.Instance.currentGameMode.transform.position = midpoint;

        _state = MergeState.Merged;
    }


    private void Split()
    {
        PlayerMode splitMode = GameManager.Instance.playerMode;
            bool isHorizontal = splitMode == PlayerMode.Horizontal;
        Debug.Log(isHorizontal.ToString() + " " + splitMode.ToString());

        Vector3 centerPos = GameManager.Instance.currentGameMode.transform.position;

        GameManager.Instance.currentGameMode.SetHide();
        GameManager.Instance.playerMode = PlayerMode.None;

        _player1.SetShow();
        _player2.SetShow();

        float halfP1 = isHorizontal ? col1.size.x * 0.5f : col1.size.y * 0.5f;
        float halfMerged = isHorizontal ? col3.size.x * 0.5f : col3.size.y * 0.5f;

        float gap = 0.02f;
        float offset = halfP1 + halfMerged + gap;

        Vector3 splitDir = isHorizontal
            ? (_isP1Left ? Vector3.left : Vector3.right)
            : (_isP1Left ? Vector3.down : Vector3.up);

        _player1.transform.position = centerPos + splitDir * offset;
        _player2.transform.position = centerPos - splitDir * offset;

        Vector2 forceDir = isHorizontal
            ? Vector2.right * Mathf.Sign(splitDir.x)
            : Vector2.up * Mathf.Sign(splitDir.y);
        
        if(isHorizontal)
        {
            _player1.ApplyKnockback(forceDir * _splitForceP1);
            _player2.ApplyKnockback(-forceDir * _splitForceP2);
            StartCoroutine(DampVelocity(rb1));
            StartCoroutine(DampVelocity(rb2));
        }
        else
        {
            if (_isP1Left)
            {
                _player2.ApplyKnockback(Vector2.up * _splitForceP2);
                StartCoroutine(DampVelocity(rb2));
            }
            else
            {
                _player1.ApplyKnockback(Vector2.up * _splitForceP1);
                StartCoroutine(DampVelocity(rb1));
            }
        }
    }



    IEnumerator DampVelocity(Rigidbody2D rb)
    {
        while (rb.linearVelocity.magnitude > 0.05f)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime);
            yield return null;
        }
        rb.linearVelocity = Vector2.zero;
    }
}