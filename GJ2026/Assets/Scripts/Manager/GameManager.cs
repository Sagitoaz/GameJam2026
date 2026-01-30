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
        UnityEngine.Debug.Log("Game Over - Player hit trap!");
        // TODO: Xử lý endgame logic ở đây
        // Ví dụ: show UI game over, restart level, etc.
        Time.timeScale = 0f;
    }

    public void WinGame()
    {
        UnityEngine.Debug.Log("YOU WIN! Level completed!");
        // TODO: Xử lý win game logic ở đây
        // Ví dụ: show UI win, load next level, play victory sound, etc.
        Time.timeScale = 0f;
    }
}