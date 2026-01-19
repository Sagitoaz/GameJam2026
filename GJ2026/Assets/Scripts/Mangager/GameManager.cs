public class GameManager : Singleton<GameManager>
{
    public PlayerMode playerMode;
    public Player currentGameMode;

    private void Start()
    {
        playerMode = PlayerMode.None;
    }
}