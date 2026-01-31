using UnityEngine;
public class GameManager : Singleton<GameManager>
{
    public PlayerMode PlayerMode;
    public Player CurrentGameMode;
    
    private bool isPaused = false;
    public bool IsPaused => isPaused;

    private void Start()
    {
        PlayerMode = PlayerMode.None;
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
        WinGame("LevelSelect");
    }

    public void WinGame(string targetSceneName)
    {
        UnityEngine.Debug.Log($"<color=green>YOU WIN! Loading scene: {targetSceneName}</color>");
        
        // Có thể thêm delay, show UI, v.v.
        StartCoroutine(WinGameRoutine(targetSceneName));
    }

    private System.Collections.IEnumerator WinGameRoutine(string sceneName)
    {
        // Show win UI hoặc effects
        // TODO: PanelManager.Instance.ShowPanel("WinPanel");
        
        yield return new WaitForSeconds(1.5f); // Delay để player thấy
        
        // Load scene
        if (!string.IsNullOrEmpty(sceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelect");
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