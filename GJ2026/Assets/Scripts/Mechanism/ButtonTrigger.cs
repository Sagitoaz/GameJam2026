using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ButtonType
{
    SinglePlayer,  // Chỉ cần 1 player
    DualPlayer     // Cần cả 2 player
}

public enum PlayerRequirement
{
    Any,           // Bất kỳ player nào
    Player1Only,   // Chỉ Player 1
    Player2Only    // Chỉ Player 2
}

public enum ButtonMode
{
    Normal,   // Nút thường: ấn là giữ luôn
    Special   // Nút đặc biệt: rời ra là bật lại
}

public class ButtonTrigger : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private ButtonType buttonType = ButtonType.SinglePlayer;
    [SerializeField] private PlayerRequirement playerRequirement = PlayerRequirement.Any;
    [SerializeField] private ButtonMode buttonMode = ButtonMode.Normal;
    [SerializeField] private string eventName = "Button01_Pressed";
    [SerializeField] private bool requireSeparatePlayers = true;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer buttonSprite;
    [SerializeField] private Color normalColorAny = Color.gray;
    [SerializeField] private Color normalColorP1 = new Color(0.3f, 0.6f, 1f); // Xanh dương cho P1
    [SerializeField] private Color normalColorP2 = new Color(1f, 0.4f, 0.3f); // Đỏ cam cho P2
    [SerializeField] private Color pressedColor = Color.green;
    [SerializeField] private float pressedYOffset = -0.1f;

    private HashSet<Player> playersOnButton = new HashSet<Player>();
    private bool isPressed = false;
    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.position;

        if (buttonSprite == null)
            buttonSprite = GetComponentInChildren<SpriteRenderer>();
        
        // Set màu ban đầu để người chơi thấy
        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        playersOnButton.Add(player);
        CheckButtonState();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        playersOnButton.Remove(player);
        CheckButtonState();
    }

    private void CheckButtonState()
    {
        bool shouldBePressed = false;

        if (buttonType == ButtonType.SinglePlayer)
        {
            // Check player requirement
            if (playerRequirement == PlayerRequirement.Any)
            {
                shouldBePressed = playersOnButton.Count > 0;
            }
            else if (playerRequirement == PlayerRequirement.Player1Only)
            {
                shouldBePressed = HasPlayer("Player1") || HasPlayer("player1");
            }
            else if (playerRequirement == PlayerRequirement.Player2Only)
            {
                shouldBePressed = HasPlayer("Player2") || HasPlayer("player2");
            }
        }
        else if (buttonType == ButtonType.DualPlayer)
        {
            // Cần 2 player cùng đứng trên 1 nút này
            shouldBePressed = playersOnButton.Count >= 2;
        }

        if (shouldBePressed != isPressed)
        {
            isPressed = shouldBePressed;

            if (isPressed)
            {
                OnButtonPressed();
            }
            else
            {
                if (buttonMode == ButtonMode.Special)
                {
                    OnButtonReleased();
                }
            }
        }
    }
    
    private bool HasPlayer(string playerName)
    {
        foreach (var player in playersOnButton)
        {
            string cleanName = player.name.Replace(" ", "").ToLower();
            string searchName = playerName.Replace(" ", "").ToLower();
            
            if (cleanName.Contains(searchName))
                return true;
        }
        return false;
    }

    private void OnButtonPressed()
    {
        Debug.Log($"<color=green>[Button {name}] PRESSED! Event: {eventName}</color>");
        EventManager.Instance.TriggerEvent(eventName);
        UpdateVisual();
    }

    private void OnButtonReleased()
    {
        EventManager.Instance.TriggerEvent(eventName + "_Released");
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (buttonSprite != null)
        {
            if (isPressed)
            {
                buttonSprite.color = pressedColor;
            }
            else
            {
                // Màu normal dựa theo player requirement
                switch (playerRequirement)
                {
                    case PlayerRequirement.Player1Only:
                        buttonSprite.color = normalColorP1;
                        break;
                    case PlayerRequirement.Player2Only:
                        buttonSprite.color = normalColorP2;
                        break;
                    default:
                        buttonSprite.color = normalColorAny;
                        break;
                }
            }
        }

        if (isPressed)
        {
            transform.position = originalPosition + Vector3.up * pressedYOffset;
        }
        else
        {
            transform.position = originalPosition;
        }
    }

    private void OnDrawGizmos()
    {
        // Màu gizmo theo player requirement
        if (playerRequirement == PlayerRequirement.Player1Only)
            Gizmos.color = normalColorP1;
        else if (playerRequirement == PlayerRequirement.Player2Only)
            Gizmos.color = normalColorP2;
        else
            Gizmos.color = buttonType == ButtonType.SinglePlayer ? Color.yellow : Color.cyan;
            
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>()?.bounds.size ?? Vector3.one * 0.5f);
    }
}
