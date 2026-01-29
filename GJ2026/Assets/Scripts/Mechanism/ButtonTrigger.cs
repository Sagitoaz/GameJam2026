using System.Collections.Generic;
using UnityEngine;

public enum ButtonType
{
    SinglePlayer,  // Chỉ cần 1 player
    DualPlayer     // Cần cả 2 player
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
    [SerializeField] private ButtonMode buttonMode = ButtonMode.Normal;
    [SerializeField] private string eventName = "Button01_Pressed";
    [SerializeField] private bool requireSeparatePlayers = true;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer buttonSprite;
    [SerializeField] private Color normalColor = Color.gray;
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
            shouldBePressed = playersOnButton.Count > 0;
        }
        else // DualPlayer
        {
            if (requireSeparatePlayers)
            {
                int separateCount = 0;
                foreach (var player in playersOnButton)
                {
                    if (!player.name.Contains("Horizontal") &&
                        !player.name.Contains("Vertical"))
                    {
                        separateCount++;
                    }
                }
                shouldBePressed = separateCount >= 2;
            }
            else
            {
                shouldBePressed = playersOnButton.Count >= 2;
            }
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
        Gizmos.color = buttonType == ButtonType.SinglePlayer
            ? Color.yellow
            : Color.cyan;

        var col = GetComponent<Collider2D>();
        if (col != null)
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
