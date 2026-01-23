using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGameMode : MonoBehaviour 
{
    [SerializeField] private Player _playerVertical;
    [SerializeField] private Player _playerHorizontal;
    public void OnChangeMode(InputAction.CallbackContext context)
    {
        if (!context.performed || GameManager.Instance.PlayerMode == PlayerMode.None) return;
        Vector3 currentPlayerPos;
        if (GameManager.Instance.PlayerMode == PlayerMode.Horizontal)
        {
            currentPlayerPos = _playerHorizontal.transform.position;
            GameManager.Instance.CurrentGameMode = _playerVertical;
            _playerHorizontal.SetHide();
            _playerVertical.SetShow();
            _playerVertical.transform.position = currentPlayerPos;
            GameManager.Instance.PlayerMode = PlayerMode.Vertical;
        }
        else if(GameManager.Instance.PlayerMode == PlayerMode.Vertical)
        {
            currentPlayerPos = _playerVertical.transform.position;
            GameManager.Instance.CurrentGameMode = _playerHorizontal;
            _playerVertical.SetHide();
            _playerHorizontal.SetShow();
            _playerHorizontal.transform.position = currentPlayerPos;
            GameManager.Instance.PlayerMode = PlayerMode.Horizontal;
        }
    }
}