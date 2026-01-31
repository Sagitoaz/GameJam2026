using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelSelectController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingPanel;
    
    [Header("Settings")]
    public float loadingTime = 1.5f;
    public GameObject letterPanel;

    private bool isLoading = false;

    void Start()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    public void SelectMap(string mapName)
    {
        if (isLoading) return;
        StartCoroutine(LoadMapRoutine(mapName));
    }

    public void BackToMenu()
    {
        if (isLoading) return;
        SceneManager.LoadScene("Intro");
    }

    IEnumerator LoadMapRoutine(string sceneName)
    {
        isLoading = true;

        if (loadingPanel != null) loadingPanel.SetActive(true);

        yield return new WaitForSeconds(loadingTime);
        
        SceneManager.LoadScene(sceneName);
    }

    public void OnClickLetterButton() {
        letterPanel.SetActive(true);
    }
}