using System.Collections.Generic;
using UnityEngine;

public enum ButtonType
{
    SinglePlayer,  // Chỉ cần 1 player
    DualPlayer     // Cần cả 2 player
}

public class ButtonTrigger : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private ButtonType buttonType = ButtonType.SinglePlayer;
    [SerializeField] private string eventName = "Button01_Pressed";
    [SerializeField] private bool stayPressed = true; // Nút giữ trạng thái hay reset khi rời
    [SerializeField] private bool requireSeparatePlayers = true; // Dual button cần 2 player riêng biệt (không cho phép merged player)
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer buttonSprite;
    [SerializeField] private Color normalColor = Color.gray;
    [SerializeField] private Color pressedColor = Color.green;
    [SerializeField] private float pressedYOffset = -0.1f; // Độ lún của nút
    
    private HashSet<Player> playersOnButton = new HashSet<Player>();
    private bool isPressed = false;
    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.position;
        
        if (buttonSprite == null)
            buttonSprite = GetComponentInChildren<SpriteRenderer>();
        
        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            playersOnButton.Add(player);
            CheckButtonState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            playersOnButton.Remove(player);
            CheckButtonState();
        }
    }

    private void CheckButtonState()
    {
        bool shouldBePressed = false;

        if (buttonType == ButtonType.SinglePlayer)
        {
            shouldBePressed = playersOnButton.Count > 0;
        }
        else if (buttonType == ButtonType.DualPlayer)
        {
            if (requireSeparatePlayers)
            {
                // Đếm chỉ player riêng biệt (player1, player2), không tính merged player
                int separatePlayerCount = 0;
                foreach (var player in playersOnButton)
                {
                    // Check xem có phải player đơn lẻ không (không phải merged player)
                    // Merged player thường có tên chứa "Horizontal" hoặc "Vertical"
                    if (!player.name.Contains("Horizontal") && !player.name.Contains("Vertical"))
                    {
                        separatePlayerCount++;
                    }
                }
                shouldBePressed = separatePlayerCount >= 2;
            }
            else
            {
                // Cho phép merged player, chỉ cần có 2 player bất kỳ
                shouldBePressed = playersOnButton.Count >= 2;
            }
        }

        // Nếu trạng thái thay đổi
        if (shouldBePressed != isPressed)
        {
            isPressed = shouldBePressed;

            if (isPressed)
            {
                OnButtonPressed();
            }
            else if (!stayPressed)
            {
                OnButtonReleased();
            }
        }
    }

    private void OnButtonPressed()
    {
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
            buttonSprite.color = isPressed ? pressedColor : normalColor;
        }

        // Hiệu ứng lún xuống
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
        Gizmos.color = buttonType == ButtonType.SinglePlayer ? Color.yellow : Color.cyan;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>()?.bounds.size ?? Vector3.one * 0.5f);
    }
}
