using UnityEngine;
public class GameManager : Singleton<GameManager>
{
    public PlayerMode PlayerMode;
    public Player CurrentGameMode;

    private void Start()
    {
        PlayerMode = PlayerMode.None;
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