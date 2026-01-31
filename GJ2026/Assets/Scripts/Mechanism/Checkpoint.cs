using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private bool autoSaveOnce = true; // Chỉ save 1 lần khi chạm vào

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private GameObject effectPrefab; // Optional: particle effect khi activate

    [Header("Audio")]
    [SerializeField] private AudioClip activateSound;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem có phải player không
        if (other.GetComponent<Player>() == null)
            return;

        // Nếu đã activate và chỉ save 1 lần thì return
        if (isActivated && autoSaveOnce)
            return;

        // Kích hoạt checkpoint
        ActivateCheckpoint();
    }

    private void ActivateCheckpoint()
    {
        if (CheckpointManager.Instance == null)
        {
            Debug.LogError("[Checkpoint] Không tìm thấy CheckpointManager trong scene!");
            return;
        }

        // Lưu checkpoint
        CheckpointManager.Instance.SetCheckpoint(transform.position);
        isActivated = true;

        // Visual feedback
        UpdateVisual();

        // Effect
        if (effectPrefab != null)
        {
            Instantiate(effectPrefab, transform.position, Quaternion.identity);
        }

        // Sound
        if (activateSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(activateSound);
        }
    }

    private void UpdateVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isActivated ? activeColor : inactiveColor;
        }
    }

    // Gọi để reset checkpoint (ví dụ khi restart level)
    public void ResetCheckpoint()
    {
        isActivated = false;
        UpdateVisual();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
