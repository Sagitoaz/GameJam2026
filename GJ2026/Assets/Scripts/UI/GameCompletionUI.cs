using UnityEngine;

public class GameCompletionUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("GameObject sẽ hiện khi hoàn thành game (trophy, star, text, etc.)")]
    [SerializeField] private GameObject completionObject;

    private void Start()
    {
        UpdateCompletionUI();
    }

    private void OnEnable()
    {
        // Cập nhật mỗi khi scene được enable
        UpdateCompletionUI();
    }

    private void UpdateCompletionUI()
    {
        if (completionObject == null)
        {
            Debug.LogWarning("[GameCompletionUI] Completion Object chưa được gán!");
            return;
        }

        bool isCompleted = GameManager.IsGameCompleted();
        completionObject.SetActive(isCompleted);

        Debug.Log($"[GameCompletionUI] Game completed: {isCompleted}");
    }

    // Method để test trong editor
    [ContextMenu("Test - Mark Game Completed")]
    private void TestMarkCompleted()
    {
        PlayerPrefs.SetInt("GameCompleted", 1);
        PlayerPrefs.Save();
        UpdateCompletionUI();
        Debug.Log("[TEST] Game marked as completed!");
    }

    [ContextMenu("Test - Reset Game Completion")]
    private void TestResetCompletion()
    {
        PlayerPrefs.DeleteKey("GameCompleted");
        PlayerPrefs.Save();
        UpdateCompletionUI();
        Debug.Log("[TEST] Game completion reset!");
    }
}
