using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : Singleton<GameManager>
{
    // private const string TotalWonKey = "LevelsWon";
    public PlayerMode PlayerMode;
    public Player CurrentGameMode;
    
    private bool isPaused = false;
    public bool IsPaused => isPaused;
    // [SerializeField] private GameObject letterPanel;

    private void Start()
    {
        PlayerMode = PlayerMode.None;
        // RegisterLevelWin("Map 1");
    }

    public void PauseGame()
    {
        if (isPaused) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        // Không pause sound - AudioListener.pause vẫn để false
        AudioListener.pause = false;
        
        Debug.Log("[GameManager] Game Paused (Sound vẫn chạy)");
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
        Debug.Log("[GameManager] Game Resumed");
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void TriggerEndGame()
    {
        Debug.Log("Game Over - Player hit trap!");
        
        // Respawn tại checkpoint thay vì dừng game
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnAtCheckpoint();
        }
        else
        {
            // Fallback: pause game nếu không có CheckpointManager
            Time.timeScale = 0f;
        }
    }

    public void WinGame()
    {
        WinGame("MainMenu");
    }

    public void WinGame(string targetSceneName)
    {
        UnityEngine.Debug.Log($"<color=green>YOU WIN! Loading scene: {targetSceneName}</color>");
        // // Register level win (idempotent: won't count same level twice)
        // RegisterLevelWin("Map 1");
        // Có thể thêm delay, show UI, v.v.
        StartCoroutine(WinGameRoutine(targetSceneName));
    }
    // private void RegisterLevelWin(string levelName)
    // {
    //     if (string.IsNullOrEmpty(levelName)) return;
    //     Debug.Log($"[GameManager] Level name is valid: {levelName}");

    //     string levelKey = $"LevelWon_{levelName}";
    //     if (PlayerPrefs.GetInt(levelKey, 0) == 1)
    //     {
    //         // Đã được ghi nhận trước đó => không cộng lại
    //         return;
    //     }

    //     PlayerPrefs.SetInt(levelKey, 1);

    //     int total = PlayerPrefs.GetInt(TotalWonKey, 0) + 1;
    //     PlayerPrefs.SetInt(TotalWonKey, total);
    //     PlayerPrefs.Save();

    //     Debug.Log($"[GameManager] Registered win for '{levelName}'. Total wins: {total}");
        
    //     // Kiểm tra nếu tên cấp độ là "Map 3" thì mới hiển thị letterPanel
    //     if (levelName == "Map 1" && total == 1)
    //     {
    //         letterPanel?.SetActive(true);
    //         Debug.Log("[GameManager] First 'Map 3' win - showing letter panel.");
    //     }
    // }


    private System.Collections.IEnumerator WinGameRoutine(string sceneName)
    {
        // Show win UI hoặc effects
        // TODO: PanelManager.Instance.ShowPanel("WinPanel");
        
        yield return new WaitForSeconds(1.5f); // Delay để player thấy
        
        // Load scene
        if (!string.IsNullOrEmpty(sceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        else
        {
            // Fallback: reload current scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
    }
}