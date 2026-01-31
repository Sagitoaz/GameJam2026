using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private RectTransform p1Transform;     // Kéo P1_Visual_img
    [SerializeField] private RectTransform p2Transform;     // Kéo P2_Visual_img
    [SerializeField] private GameObject loadingPanel;       // Kéo Loading_panel
    [SerializeField] private GameObject mainArea;           // Kéo MainArea

    [Header("Attributes")]
    [SerializeField] private float hoverDist = 30f;         
    [SerializeField] private float mergeSpeed = 3.0f;       // Tốc độ lao vào
    [SerializeField] private float impactPauseTime = 0.5f;  // Dừng lại 0.5s để ngắm cảnh dính nhau
    [SerializeField] private float loadingTime = 2.0f;      // Chờ loading
    [SerializeField] private float centerOffset = 40f;      // Khoảng cách giữa 2 cục khi dính nhau (để không đè lên nhau)

    private Vector2 p1OriginalPos;
    private Vector2 p2OriginalPos;
    private bool isHovering = false;
    private bool isStarting = false;      
    private RectTransform parentRect;     // Để lấy kích thước màn hình

    private void Start()
    {
        // Lưu vị trí gốc
        if (p1Transform != null) {
            p1OriginalPos = p1Transform.anchoredPosition;
            parentRect = p1Transform.parent.GetComponent<RectTransform>();
        }
        if (p2Transform != null) p2OriginalPos = p2Transform.anchoredPosition;

        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    private void Update()
    {
        if (isStarting) return;

        // Logic Hover: 
        // P1 (Góc trái) cộng thêm X để nhích phải
        Vector2 targetP1 = isHovering ? p1OriginalPos + new Vector2(hoverDist, 0) : p1OriginalPos;
        // P2 (Góc phải) trừ đi X để nhích trái
        Vector2 targetP2 = isHovering ? p2OriginalPos - new Vector2(hoverDist, 0) : p2OriginalPos;

        if (p1Transform) p1Transform.anchoredPosition = Vector2.Lerp(p1Transform.anchoredPosition, targetP1, Time.deltaTime * 10f);
        if (p2Transform) p2Transform.anchoredPosition = Vector2.Lerp(p2Transform.anchoredPosition, targetP2, Time.deltaTime * 10f);
    }

    public void OnClickStart()
    {
        if (isStarting) return;
        StartCoroutine(SequenceStartGame());
    }

    public void OnClickSetting()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.click);
        PanelManager.Instance.OpenPanel(GameConfig.PANEL_SETTING);
    }
    
    public void OnClickManual()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.click);
        PanelManager.Instance.OpenPanel(GameConfig.PANEL_MANUAL);
    }

    public void OnClickQuit()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.click);
        Application.Quit();
    }
    
    public void OnStartHoverEnter() { if (!isStarting) isHovering = true; }
    public void OnStartHoverExit() { if (!isStarting) isHovering = false; }

    IEnumerator SequenceStartGame()
    {
        isStarting = true; 

        if (mainArea != null) mainArea.SetActive(false);

        float halfWidth = parentRect.rect.width / 2;
        float halfHeight = parentRect.rect.height / 2;

        Vector2 endP1 = new Vector2(halfWidth - centerOffset, halfHeight);

        Vector2 endP2 = new Vector2(-halfWidth + centerOffset, halfHeight);

        Vector2 startP1 = p1Transform.anchoredPosition;
        Vector2 startP2 = p2Transform.anchoredPosition;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * mergeSpeed;
            p1Transform.anchoredPosition = Vector2.Lerp(startP1, endP1, t);
            p2Transform.anchoredPosition = Vector2.Lerp(startP2, endP2, t);
            yield return null;
        }
        
        p1Transform.anchoredPosition = endP1;
        p2Transform.anchoredPosition = endP2;

        yield return new WaitForSeconds(impactPauseTime);

        if (loadingPanel != null) loadingPanel.SetActive(true);
        yield return new WaitForSeconds(loadingTime);

        SceneManager.LoadScene("LevelSelect"); 
    }

    public void QuitGame() { Application.Quit(); }
}