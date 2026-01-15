using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGameMode : MonoBehaviour 
{
    [SerializeField] private Player _playerVertical;
    [SerializeField] private Player _playerHorizontal;
    public void OnChangeMode(InputAction.CallbackContext context)
    {
        if (!context.performed || GameManager.Instance.playerMode == PlayerMode.None) return;
        Vector3 currentPlayerPos;
        if (GameManager.Instance.playerMode == PlayerMode.Horizontal)
        {
            currentPlayerPos = _playerHorizontal.transform.position;
            GameManager.Instance.currentGameMode = _playerVertical;
            _playerHorizontal.SetHide();
            _playerVertical.SetShow();
            _playerVertical.transform.position = currentPlayerPos;
            GameManager.Instance.playerMode = PlayerMode.Vertical;
        }
        else if(GameManager.Instance.playerMode == PlayerMode.Vertical)
        {
            currentPlayerPos = _playerVertical.transform.position;
            GameManager.Instance.currentGameMode = _playerHorizontal;
            _playerVertical.SetHide();
            _playerHorizontal.SetShow();
            _playerHorizontal.transform.position = currentPlayerPos;
            GameManager.Instance.playerMode = PlayerMode.Horizontal;
        }
    }
}