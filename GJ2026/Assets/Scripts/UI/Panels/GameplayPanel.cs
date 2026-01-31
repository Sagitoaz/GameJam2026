using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayPanel : Panel
{
    public void Pause()
    {
        PanelManager.Instance.ClosePanel(GameConfig.GAMEPLAY_PANEL);
        PanelManager.Instance.OpenPanel(GameConfig.PAUSE_PANEL);
        Time.timeScale = 0;
    }
    public void UpdateLetterText()
    {
        
    }
}
