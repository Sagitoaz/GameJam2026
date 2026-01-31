using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Script đơn giản để load scene khi bấm button
/// </summary>
[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private GameObject loadingPanel; // Kéo loading panel vào đây
    [SerializeField] private float loadingDelay = 0.5f; // Delay trước khi load scene

    private void Start()
    {
        GetComponent<Button>()?.onClick.AddListener(LoadScene);
        
        // Ẩn loading panel ban đầu
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(LoadSceneWithLoading());
        }
    }

    private IEnumerator LoadSceneWithLoading()
    {
        // Show loading panel
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        yield return new WaitForSeconds(loadingDelay);

        SceneManager.LoadScene(sceneName);
    }

    // Có thể gọi trực tiếp từ Inspector onClick
    public void LoadSceneName(string name)
    {
        sceneName = name;
        LoadScene();
    }
}



