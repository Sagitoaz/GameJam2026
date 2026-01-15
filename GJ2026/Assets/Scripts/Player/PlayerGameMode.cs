using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGameMode : MonoBehaviour 
{
    [SerializeField] private Player _playerVertical;
    [SerializeField] private Player _playerHorizontal;
    enum PlayerMode {Vertical, Horizontal};
    PlayerMode playerMode = PlayerMode.Vertical;
    public void OnChangeMode(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Vector3 currentPlayerPos;
        if (playerMode == PlayerMode.Vertical)
        {
            currentPlayerPos = _playerVertical.transform.position;
            _playerHorizontal.SetShow();
            _playerHorizontal.transform.position = currentPlayerPos;
            _playerVertical.SetHide();
            playerMode = PlayerMode.Horizontal;
        }
        else
        {
            currentPlayerPos = _playerHorizontal.transform.position;
            _playerHorizontal.SetHide();
            _playerVertical.SetShow();
            _playerVertical.transform.position = currentPlayerPos;
            playerMode = PlayerMode.Vertical;
        }

    }
}