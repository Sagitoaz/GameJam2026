using UnityEngine;

public class CheckpointManager : Singleton<CheckpointManager>
{
    [Header("Player References")]
    [SerializeField] private Player player1;
    [SerializeField] private Player player2;
    [SerializeField] private Player playerMerged;
    [SerializeField] private PlayerMergeSplitController mergeSplitController;

    [Header("Checkpoint Settings")]
    [SerializeField] private Vector3 defaultSpawnPosition = Vector3.zero;
    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private GameObject loseScreenUI;

    private Vector3 currentCheckpointPosition;
    private bool hasCheckpoint = false;

    public override void Awake()
    {
        base.Awake();
        currentCheckpointPosition = defaultSpawnPosition;
    }
    private void Start()
    {
        // Tự động tìm references nếu chưa gán
        if (player1 == null || player2 == null)
        {
            Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p.name.ToLower().Contains("player1") || p.name.ToLower().Contains("p1"))
                    player1 = p;
                else if (p.name.ToLower().Contains("player2") || p.name.ToLower().Contains("p2"))
                    player2 = p;
                else if (p.name.ToLower().Contains("merge") || p.name.ToLower().Contains("horizontal"))
                    playerMerged = p;
            }
        }

        if (mergeSplitController == null)
        {
            mergeSplitController = FindFirstObjectByType<PlayerMergeSplitController>();
        }
    }

    /// <summary>
    /// Lưu checkpoint mới
    /// </summary>
    public void SetCheckpoint(Vector3 position)
    {
        currentCheckpointPosition = position;
        hasCheckpoint = true;
        Debug.Log($"<color=cyan>[Checkpoint] Đã lưu checkpoint tại: {position}</color>");
    }

    /// <summary>
    /// Respawn player về checkpoint
    /// </summary>
    public void RespawnAtCheckpoint()
    {
        if (!hasCheckpoint)
        {
            currentCheckpointPosition = defaultSpawnPosition;
        }

        StartCoroutine(RespawnRoutine());
    }

    private System.Collections.IEnumerator RespawnRoutine()
    {
        // Fade out hoặc effect (có thể thêm sau)
        loseScreenUI.SetActive(true);
        yield return new WaitForSeconds(respawnDelay);

        // Reset game state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerMode = PlayerMode.None;
        }

        // Ẩn merged player nếu đang hiện
        if (playerMerged != null && playerMerged.gameObject.activeSelf)
        {
            playerMerged.SetHide();
        }

        // Spawn 2 player riêng lẻ tại checkpoint
        if (player1 != null && player2 != null)
        {
            // Spawn cách nhau 1 chút
            player1.transform.position = currentCheckpointPosition + Vector3.left * 0.5f;
            player2.transform.position = currentCheckpointPosition + Vector3.right * 0.5f;

            // Reset velocity trước khi SetShow
            Rigidbody2D rb1 = player1.GetComponent<Rigidbody2D>();
            Rigidbody2D rb2 = player2.GetComponent<Rigidbody2D>();
            if (rb1 != null) rb1.linearVelocity = Vector2.zero;
            if (rb2 != null) rb2.linearVelocity = Vector2.zero;

            // Delay nhỏ để đảm bảo position đã set xong
            yield return new WaitForSeconds(0.1f);

            // Bật lại control
            player1.SetShow();
            player2.SetShow();
        }

        // Resume game
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Debug.Log("<color=green>[Checkpoint] Đã respawn!</color>");
        loseScreenUI.SetActive(false);
    }

    /// <summary>
    /// Lấy vị trí checkpoint hiện tại
    /// </summary>
    public Vector3 GetCurrentCheckpoint()
    {
        return currentCheckpointPosition;
    }

    /// <summary>
    /// Reset về spawn mặc định
    /// </summary>
    public void ResetToDefaultSpawn()
    {
        currentCheckpointPosition = defaultSpawnPosition;
        hasCheckpoint = false;
    }
}
