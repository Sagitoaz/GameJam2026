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
    private float _mergeSpeed = 15f;
    private float _splitForceP1 = 10f;
    private float _splitForceP2 = 50f;
    private float _splitForceVertical = 20f;
    private float _width;
    private float _height;
    private float _diagonal;

    private BoxCollider2D _colPlayer1, _colPlayer2, _colPlayerHorizontal, _colPlayerVertical;
    private Rigidbody2D _rbPlayer1, _rbPlayer2;
    MergeState _state = MergeState.Separate;

    private void Awake()
    {
        _colPlayer1 = _player1.GetComponent<BoxCollider2D>();
        _colPlayer2 = _player2.GetComponent<BoxCollider2D>();
        _colPlayerHorizontal = _playerVertical.GetComponent<BoxCollider2D>();
        _colPlayerVertical = _playerHorizontal.GetComponent<BoxCollider2D>();
        _rbPlayer1 = _player1.GetComponent<Rigidbody2D>();
        _rbPlayer2 = _player2.GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        _width = _colPlayer1.bounds.extents.x;
        _height = _colPlayer1.bounds.extents.y;
        _diagonal = Mathf.Sqrt(_width * _width + _height * _height);
    }
    public void OnMerge(InputAction.CallbackContext context)
    {
        Debug.Log(_state.ToString());
        if (!context.performed) return;
        if (_state == MergeState.Separate && Vector2.Distance(_player1.transform.position, _player2.transform.position) <= 2f)
        {
            _state = MergeState.Merging;
            GameManager.Instance.PlayerMode = PlayerMode.None;
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

        _rbPlayer1.simulated = false;
        _rbPlayer2.simulated = false;

        while (Vector2.Distance(_player1.transform.position, midpoint) >= _diagonal)
        {
            _player1.transform.position = Vector3.MoveTowards(_player1.transform.position, midpoint, _mergeSpeed * Time.deltaTime);
            _player2.transform.position = Vector3.MoveTowards(_player2.transform.position, midpoint, _mergeSpeed * Time.deltaTime);
            yield return null;
        }

        _player1.SetHide();
        _player2.SetHide();

        GameManager.Instance.PlayerMode = PlayerMode.Horizontal;
        GameManager.Instance.CurrentGameMode = _playerHorizontal;

        GameManager.Instance.CurrentGameMode.SetShow();
        GameManager.Instance.CurrentGameMode.transform.position = midpoint;

        _state = MergeState.Merged;
    }


    private void Split()
    {
        PlayerMode splitMode = GameManager.Instance.PlayerMode;

        Player mergedPlayer = GameManager.Instance.CurrentGameMode;
        Vector3 centerPos = mergedPlayer.transform.position;

        mergedPlayer.SetHide();
        GameManager.Instance.PlayerMode = PlayerMode.None;

        ShowSinglePlayers(centerPos, splitMode);

        if (splitMode == PlayerMode.Vertical)
        {
            SplitVertical();
        }
        else if (splitMode == PlayerMode.Horizontal)
        {
            SplitHorizontal();
        }

        _state = MergeState.Separate;
    }

    private void ShowSinglePlayers(Vector3 pos, PlayerMode splitMode)
    {
        if (splitMode == PlayerMode.Horizontal)
        {
            _player1.transform.position = new Vector3(pos.x - _width, pos.y, pos.z);
            _player2.transform.position = new Vector3(pos.x + _width, pos.y, pos.z);
        }
        else
        {
            _player1.transform.position = new Vector3(pos.x, pos.y - _height, pos.z);
            _player2.transform.position = new Vector3(pos.x, pos.y + _height, pos.z);
        }

        _player1.SetShow();
        _player2.SetShow();
    }


    private void SplitVertical()
    {
        Player topPlayer;
        if (_player1.transform.position.y >= _player2.transform.position.y)
        {
            topPlayer = _player1;
        }
        else
        {
            topPlayer = _player2;
        }

        Rigidbody2D rbTop = topPlayer.GetComponent<Rigidbody2D>();
        rbTop.linearVelocity = Vector2.zero;
        rbTop.AddForce(Vector2.up * _splitForceVertical, ForceMode2D.Impulse);
    }

    private void SplitHorizontal()
    {
        _player1.ApplyKnockback(Vector2.left * _splitForceP1);
        _player2.ApplyKnockback(Vector2.right * _splitForceP2);
    }

    private Vector3 FindSafeMergePosition(Vector3 desiredPos)
    {
        Vector2 size = _colPlayerHorizontal.bounds.size;
        LayerMask groundMask = LayerMask.GetMask("Ground");

        // Nếu vị trí hiện tại không kẹt → dùng luôn
        if (!Physics2D.OverlapBox(desiredPos, size, 0f, groundMask))
            return desiredPos;

        // Thử đẩy lên trên từng bước nhỏ
        const float step = 0.05f;
        const int maxTry = 20;

        for (int i = 1; i <= maxTry; i++)
        {
            Vector3 checkPos = desiredPos + Vector3.up * step * i;
            if (!Physics2D.OverlapBox(checkPos, size, 0f, groundMask))
                return checkPos;
        }

        // Fallback: trả về vị trí ban đầu (hiếm)
        return desiredPos;
    }

}