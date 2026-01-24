using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    [Header("Win Condition Settings")]
    [SerializeField] private Transform player1Goal; // Trigger cho Player 1
    [SerializeField] private Transform player2Goal; // Trigger cho Player 2
    [SerializeField] private Player player1Reference; // Reference đến Player 1
    [SerializeField] private Player player2Reference; // Reference đến Player 2
    
    [Header("Visual Feedback")]
    [SerializeField] private Color goalColor = Color.yellow;
    
    private bool player1InGoal = false;
    private bool player2InGoal = false;
    private bool gameWon = false;

    private void Start()
    {
        // Đảm bảo các trigger có GoalTriggerZone component
        SetupGoalZone(player1Goal, 1);
        SetupGoalZone(player2Goal, 2);
    }

    private void SetupGoalZone(Transform goalTransform, int playerNumber)
    {
        if (goalTransform != null)
        {
            var zone = goalTransform.GetComponent<GoalTriggerZone>();
            if (zone == null)
            {
                zone = goalTransform.gameObject.AddComponent<GoalTriggerZone>();
            }
            zone.Initialize(this, playerNumber);
        }
    }

    public void OnPlayerEnterGoal(int playerNumber, Player player)
    {
        if (gameWon) return;

        // Kiểm tra player có đúng không
        if (playerNumber == 1 && player == player1Reference)
        {
            player1InGoal = true;
            Debug.Log("Player 1 đã vào trigger của mình!");
        }
        else if (playerNumber == 2 && player == player2Reference)
        {
            player2InGoal = true;
            Debug.Log("Player 2 đã vào trigger của mình!");
        }

        CheckWinCondition();
    }

    public void OnPlayerExitGoal(int playerNumber, Player player)
    {
        if (gameWon) return;

        if (playerNumber == 1 && player == player1Reference)
        {
            player1InGoal = false;
            Debug.Log("Player 1 rời khỏi trigger");
        }
        else if (playerNumber == 2 && player == player2Reference)
        {
            player2InGoal = false;
            Debug.Log("Player 2 rời khỏi trigger");
        }
    }

    private void CheckWinCondition()
    {
        if (player1InGoal && player2InGoal && !gameWon)
        {
            gameWon = true;
            Debug.Log("WIN GAME! Cả 2 player đã vào đúng trigger!");
            GameManager.Instance.WinGame();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = goalColor;
        
        if (player1Goal != null)
        {
            Gizmos.DrawWireCube(player1Goal.position, Vector3.one * 0.8f);
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(player1Goal.position + Vector3.up * 0.5f, "Player 1 Goal");
            #endif
        }
        
        if (player2Goal != null)
        {
            Gizmos.DrawWireCube(player2Goal.position, Vector3.one * 0.8f);
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(player2Goal.position + Vector3.up * 0.5f, "Player 2 Goal");
            #endif
        }
    }
}

// Component helper cho từng trigger zone
public class GoalTriggerZone : MonoBehaviour
{
    private WinTrigger parentManager;
    private int playerNumber;

    public void Initialize(WinTrigger manager, int number)
    {
        parentManager = manager;
        playerNumber = number;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            parentManager.OnPlayerEnterGoal(playerNumber, player);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            parentManager.OnPlayerExitGoal(playerNumber, player);
        }
    }
}
