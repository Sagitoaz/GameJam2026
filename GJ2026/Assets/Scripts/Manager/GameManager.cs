using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private const string GameCompletedKey = "GameCompleted";
    
    public PlayerMode PlayerMode;
    public Player CurrentGameMode;
    
    [Header("Win Settings")]
    [Tooltip("Tên scene khi hoàn thành sẽ đánh dấu game completed")]
    public string finalLevelSceneName = "Map 3";
    
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
        WinGame("MainMenu");
    }

    public void WinGame(string targetSceneName)
    {
        UnityEngine.Debug.Log($"<color=green>YOU WIN! Loading scene: {targetSceneName}</color>");
        
        // Kiểm tra xem có phải final level không
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == finalLevelSceneName)
        {
            MarkGameCompleted();
        }
        
        StartCoroutine(WinGameRoutine(targetSceneName));
    }

    private void MarkGameCompleted()
    {
        PlayerPrefs.SetInt(GameCompletedKey, 1);
        PlayerPrefs.Save();
        Debug.Log($"<color=yellow>[GameManager] GAME COMPLETED! Final level '{finalLevelSceneName}' beaten!</color>");
    }

    public static bool IsGameCompleted()
    {
        return PlayerPrefs.GetInt(GameCompletedKey, 0) == 1;
    }


    private IEnumerator WinGameRoutine(string sceneName)
    {
        yield return new WaitForSeconds(1.5f);
        
        if (!string.IsNullOrEmpty("LevelSelect"))
        {
            SceneManager.LoadScene("LevelSelect");
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}