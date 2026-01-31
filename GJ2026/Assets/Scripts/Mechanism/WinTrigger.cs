using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinTrigger : MonoBehaviour
{
    [Header("Win Condition")]
    [SerializeField] private bool requireBothPlayers = true;
    [SerializeField] private string targetSceneName = "MainMenu";

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = new Color(1f, 1f, 0f, 0.3f);
    [SerializeField] private Color readyColor = new Color(0f, 1f, 0f, 0.5f);

    [Header("Effects")]
    [SerializeField] private ParticleSystem winParticles;
    [SerializeField] private AudioClip winSound;

    [SerializeField] private GameObject winPanel;

    private HashSet<Player> playersInTrigger = new HashSet<Player>();
    private bool hasTriggered = false;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateVisual();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        playersInTrigger.Add(player);
        CheckWinCondition();
        UpdateVisual();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        playersInTrigger.Remove(player);
        UpdateVisual();
    }

    private void CheckWinCondition()
    {
        if (hasTriggered) return;

        bool shouldWin = false;

        if (requireBothPlayers)
        {
            // Điều kiện: 2 player riêng lẻ HOẶC 1 player đang merged
            if (playersInTrigger.Count >= 2)
            {
                shouldWin = true;
            }
            else if (playersInTrigger.Count == 1)
            {
                // Kiểm tra nếu player này đang ở trạng thái merged
                if (GameManager.Instance != null && GameManager.Instance.PlayerMode != PlayerMode.None)
                {
                    shouldWin = true;
                }
            }
        }
        else
        {
            if (playersInTrigger.Count > 0)
            {
                shouldWin = true;
            }
        }

        if (shouldWin)
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        hasTriggered = true;
        Debug.Log("<color=green>[WinTrigger] VICTORY!</color>");

        if (winParticles != null)
        {
            winParticles.Play();
        }

        if (winSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(winSound);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinGame(targetSceneName);
        }
        else
        {
            StartCoroutine(LoadSceneDelayed());
        }
    }

    private System.Collections.IEnumerator LoadSceneDelayed()
    {
        winPanel?.SetActive(true);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(targetSceneName);
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        bool isReady = false;
        
        if (requireBothPlayers)
        {
            // Sẵn sàng nếu: 2 player riêng lẻ HOẶC 1 merged player
            isReady = playersInTrigger.Count >= 2 || 
                     (playersInTrigger.Count == 1 && GameManager.Instance != null && GameManager.Instance.PlayerMode != PlayerMode.None);
        }
        else
        {
            isReady = playersInTrigger.Count > 0;
        }
        
        spriteRenderer.color = isReady ? readyColor : normalColor;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = requireBothPlayers ? Color.yellow : Color.cyan;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}
