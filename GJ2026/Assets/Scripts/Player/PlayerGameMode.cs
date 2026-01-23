using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGameMode : MonoBehaviour 
{
    [SerializeField] private Player _playerVertical;
    [SerializeField] private Player _playerHorizontal;
    
    private LayerMask _groundMask;
    
    private void Awake()
    {
        _groundMask = LayerMask.GetMask("Ground");
    }
    public void OnChangeMode(InputAction.CallbackContext context)
    {
        if (!context.performed || GameManager.Instance.PlayerMode == PlayerMode.None) return;
        StartCoroutine(ChangeModeRoutine());
    }
    
    private IEnumerator ChangeModeRoutine()
    {
        Vector3 currentPlayerPos;
        
        if (GameManager.Instance.PlayerMode == PlayerMode.Horizontal)
        {
            currentPlayerPos = _playerHorizontal.transform.position;
            _playerHorizontal.SetHide();
            
            yield return null; // Wait 1 frame
            
            Vector3 safePos = FindSafePosition(currentPlayerPos, _playerVertical.GetComponent<BoxCollider2D>());
            _playerVertical.transform.position = safePos;
            GameManager.Instance.CurrentGameMode = _playerVertical;
            GameManager.Instance.PlayerMode = PlayerMode.Vertical;
            _playerVertical.SetShow();
        }
        else if(GameManager.Instance.PlayerMode == PlayerMode.Vertical)
        {
            currentPlayerPos = _playerVertical.transform.position;
            _playerVertical.SetHide();
            
            yield return null; // Wait 1 frame
            
            Vector3 safePos = FindSafePosition(currentPlayerPos, _playerHorizontal.GetComponent<BoxCollider2D>());
            _playerHorizontal.transform.position = safePos;
            GameManager.Instance.CurrentGameMode = _playerHorizontal;
            GameManager.Instance.PlayerMode = PlayerMode.Horizontal;
            _playerHorizontal.SetShow();
        }
    }
    
    private Vector3 FindSafePosition(Vector3 desiredPos, BoxCollider2D targetCollider)
    {
        Vector2 size = targetCollider.bounds.size;
        
        // Kiểm tra vị trí hiện tại có an toàn không
        if (!Physics2D.OverlapBox(desiredPos, size * 0.9f, 0f, _groundMask))
            return desiredPos;
        
        // Thử đẩy lên trên từng bước nhỏ
        const float step = 0.1f;
        const int maxTries = 20;
        
        for (int i = 1; i <= maxTries; i++)
        {
            Vector3 checkPos = desiredPos + Vector3.up * (step * i);
            if (!Physics2D.OverlapBox(checkPos, size * 0.9f, 0f, _groundMask))
                return checkPos;
        }
        
        // Nếu không tìm được, thử xuống dưới
        for (int i = 1; i <= maxTries; i++)
        {
            Vector3 checkPos = desiredPos + Vector3.down * (step * i);
            if (!Physics2D.OverlapBox(checkPos, size * 0.9f, 0f, _groundMask))
                return checkPos;
        }
        
        // Fallback: trả về vị trí ban đầu
        return desiredPos;
    }
}