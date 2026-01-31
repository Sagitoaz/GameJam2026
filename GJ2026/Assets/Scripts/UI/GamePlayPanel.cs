using Unity.VisualScripting;
using UnityEngine;

public class GamePlayPanel : Panel
{
    [SerializeField] public DynamicCameraController Camera;
    [SerializeField] private PausePanel PausePanel;
    public void OnClickPauseButton()
    {
        PausePanel.gameObject.SetActive(true);
        Time.timeScale = 0f;
        ToggleCamera(false);
    }

    public void ToggleCamera(bool status)
    {
        Camera.enabled = status;
    }

}
