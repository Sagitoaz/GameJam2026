using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMergeSplitController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Player _player1;
    [SerializeField] private Player _player2;
    [SerializeField] private Player _playerMerged; // Chỉ còn 1 dạng gộp
    
    [Header("Merge Settings")]
    [SerializeField] private float _maxMergeDistance = 8f;
    [SerializeField] private float _mergeSpeed = 15f;
    
    [Header("Split Settings")]
    [SerializeField] private float _splitForceP1 = 10f;
    [SerializeField] private float _splitForceP2 = 50f;
    [SerializeField] private float _splitForceVertical = 20f;
    [SerializeField] private float _splitNoGravityDuration = 0.3f;

    private float _width, _height, _diagonal;
    private BoxCollider2D _colPlayer1, _colPlayer2;
    private Rigidbody2D _rbPlayer1, _rbPlayer2;
    private LayerMask _groundMask;
    private MergeState _state = MergeState.Separate;

    private void Awake()
    {
        _colPlayer1 = _player1.GetComponent<BoxCollider2D>();
        _colPlayer2 = _player2.GetComponent<BoxCollider2D>();
        _rbPlayer1 = _player1.GetComponent<Rigidbody2D>();
        _rbPlayer2 = _player2.GetComponent<Rigidbody2D>();
        _groundMask = LayerMask.GetMask("Ground");
    }

    private void Start()
    {
        _width = _colPlayer1.bounds.extents.x;
        _height = _colPlayer1.bounds.extents.y;
        _diagonal = Mathf.Sqrt(_width * _width + _height * _height);
    }
    public void OnMerge(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (_state == MergeState.Separate && CanMerge())
        {
            // Gộp lại
            _state = MergeState.Merging;
            GameManager.Instance.PlayerMode = PlayerMode.None;
            StartCoroutine(MergeRoutine());
        }
        else if (_state == MergeState.Merged)
        {
            // Tách ngang
            _state = MergeState.Separate;
            SplitHorizontal();
        }
    }

    public void OnVerticalSplit(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (_state == MergeState.Merged)
        {
            // Tách dọc
            _state = MergeState.Separate;
            SplitVertical();
        }
    }

    private bool CanMerge()
    {
        Vector3 pos1 = _player1.transform.position;
        Vector3 pos2 = _player2.transform.position;
        float distance = Vector2.Distance(pos1, pos2);
        
        if (distance > _maxMergeDistance) return false;
        
        Vector2 direction = (pos2 - pos1).normalized;
        RaycastHit2D hit = Physics2D.Raycast(pos1, direction, distance, _groundMask);
        
        Debug.DrawLine(pos1, pos2, hit.collider != null ? Color.red : Color.green, 0.5f);
        
        return hit.collider == null;
    }

    private IEnumerator MergeRoutine()
    {
        Vector3 midpoint = (_player1.transform.position + _player2.transform.position) * 0.5f;

        _rbPlayer1.simulated = false;
        _rbPlayer2.simulated = false;

        // Tạo dư ảnh từ 2 player khi merge và tắt trail
        _player1.PlayMergeSplitEffect();
        _player2.PlayMergeSplitEffect();

        while (Vector2.Distance(_player1.transform.position, midpoint) >= _diagonal)
        {
            float step = _mergeSpeed * Time.deltaTime;
            _player1.transform.position = Vector3.MoveTowards(_player1.transform.position, midpoint, step);
            _player2.transform.position = Vector3.MoveTowards(_player2.transform.position, midpoint, step);
            yield return null;
        }

        _player1.SetHide();
        _player2.SetHide();

        // Chỉ còn 1 dạng gộp
        if (_playerMerged == null)
        {
            Debug.LogError("[PlayerMergeSplitController] _playerMerged chưa được gán! Hãy kéo Player Merged vào Inspector.");
            yield break;
        }

        GameManager.Instance.PlayerMode = PlayerMode.Horizontal;
        GameManager.Instance.CurrentGameMode = _playerMerged;
        GameManager.Instance.CurrentGameMode.transform.position = midpoint;
        GameManager.Instance.CurrentGameMode.SetShow();

        // Tắt trail renderer khi đã merged
        var vfx = _playerMerged.GetComponent<PlayerVFX>();
        if (vfx != null) vfx.EnableTrail(false);

        _state = MergeState.Merged;
    }


    private void ShowSinglePlayers(Vector3 pos, bool isHorizontal)
    {
        if (isHorizontal)
        {
            // Lấy hướng cuối cùng để quyết định vị trí spawn
            float direction = GameManager.Instance.CurrentGameMode.LastHorizontalDirection;
            
            if (direction > 0) // Bắn về phải: P1 trái, P2 phải
            {
                _player1.transform.position = new Vector3(pos.x - _width, pos.y, pos.z);
                _player2.transform.position = new Vector3(pos.x + _width, pos.y, pos.z);
            }
            else // Bắn về trái: P1 phải, P2 trái
            {
                _player1.transform.position = new Vector3(pos.x + _width, pos.y, pos.z);
                _player2.transform.position = new Vector3(pos.x - _width, pos.y, pos.z);
            }
        }
        else // Split dọc
        {
            _player1.transform.position = new Vector3(pos.x, pos.y - _height, pos.z);
            _player2.transform.position = new Vector3(pos.x, pos.y + _height, pos.z);
        }

        _player1.SetShow();
        _player2.SetShow();
    }


    private void SplitVertical()
    {
        Player mergedPlayer = GameManager.Instance.CurrentGameMode;
        Vector3 centerPos = mergedPlayer.transform.position;

        mergedPlayer.SetHide();
        GameManager.Instance.PlayerMode = PlayerMode.None;

        ShowSinglePlayers(centerPos, false); // false = dọc

        // Tạo dư ảnh từ 2 player khi split
        _player1.PlayMergeSplitEffect();
        _player2.PlayMergeSplitEffect();

        Player topPlayer = _player1.transform.position.y >= _player2.transform.position.y ? _player1 : _player2;
        Player bottomPlayer = topPlayer == _player1 ? _player2 : _player1;
        
        topPlayer.ApplyKnockbackNoGravity(Vector2.up * _splitForceVertical, _splitNoGravityDuration);
        bottomPlayer.ApplyKnockbackNoGravity(Vector2.down * _splitForceVertical * 0.5f, _splitNoGravityDuration);

        // Bật lại trail sau khi bay xong
        StartCoroutine(EnableTrailAfterDelay(_splitNoGravityDuration));
    }

    private void SplitHorizontal()
    {
        Player mergedPlayer = GameManager.Instance.CurrentGameMode;
        Vector3 centerPos = mergedPlayer.transform.position;

        mergedPlayer.SetHide();
        GameManager.Instance.PlayerMode = PlayerMode.None;

        ShowSinglePlayers(centerPos, true); // true = ngang

        // Tạo dư ảnh từ 2 player khi split
        _player1.PlayMergeSplitEffect();
        _player2.PlayMergeSplitEffect();

        // Lấy hướng ngang cuối cùng từ player đang merged
        float direction = GameManager.Instance.CurrentGameMode.LastHorizontalDirection;
        
        if (direction > 0) // Hướng cuối cùng là phải → P2 bay phải, P1 giật trái
        {
            _player1.ApplyKnockbackNoGravity(Vector2.left * _splitForceP1, _splitNoGravityDuration);
            _player2.ApplyKnockbackNoGravity(Vector2.right * _splitForceP2, _splitNoGravityDuration);
        }
        else // Hướng cuối cùng là trái → P2 bay trái, P1 giật phải
        {
            _player1.ApplyKnockbackNoGravity(Vector2.right * _splitForceP1, _splitNoGravityDuration);
            _player2.ApplyKnockbackNoGravity(Vector2.left * _splitForceP2, _splitNoGravityDuration);
        }

        // Bật lại trail sau khi bay xong
        StartCoroutine(EnableTrailAfterDelay(_splitNoGravityDuration));
    }

    private IEnumerator EnableTrailAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _player1.EnableTrailAfterSplit();
        _player2.EnableTrailAfterSplit();
    }

}