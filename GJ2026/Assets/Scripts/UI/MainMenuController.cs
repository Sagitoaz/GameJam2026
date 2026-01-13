using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform p1Transform;
    public RectTransform p2Transform;
    
    [Header("Settings")]
    public float moveDistance = 100f;
    public float smoothTime = 0.1f;

    private Vector2 p1OriginalPos;
    private Vector2 p2OriginalPos;
    private Vector2 p1TargetPos;
    private Vector2 p2TargetPos;
    private bool isHovering = false;

    void Start()
    {
        p1OriginalPos = p1Transform.anchoredPosition;
        p2OriginalPos = p2Transform.anchoredPosition;
    }

    void Update()
    {
        Vector2 targetP1 = isHovering ? p1OriginalPos + new Vector2(moveDistance, 0) : p1OriginalPos;
        Vector2 targetP2 = isHovering ? p2OriginalPos - new Vector2(moveDistance, 0) : p2OriginalPos;

        p1Transform.anchoredPosition = Vector2.Lerp(p1Transform.anchoredPosition, targetP1, smoothTime);
        p2Transform.anchoredPosition = Vector2.Lerp(p2Transform.anchoredPosition, targetP2, smoothTime);
    }

    public void OnStartHoverEnter()
    {
        isHovering = true;
    }

    public void OnStartHoverExit()
    {
        isHovering = false;
    }
    
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scene1");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}