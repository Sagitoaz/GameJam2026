public class GameManager : Singleton<GameManager>
{
    public PlayerMode PlayerMode;
    public Player CurrentGameMode;

    private void Start()
    {
        PlayerMode = PlayerMode.None;
    }
}