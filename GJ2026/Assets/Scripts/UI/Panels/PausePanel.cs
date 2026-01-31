using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanel : Panel
{
    public void Resume()
    {
        PanelManager.Instance.ClosePanel(GameConfig.PAUSE_PANEL);
        PanelManager.Instance.OpenPanel(GameConfig.GAMEPLAY_PANEL);
        Time.timeScale = 1;
    }
    public void Setting()
    {
        PanelManager.Instance.OpenPanel(GameConfig.SETTING_PANEL);
    }
    public void Menu()
    {
        PanelManager.Instance.ClosePanel(GameConfig.PAUSE_PANEL);
        SceneManager.LoadScene(GameConfig.MENU_SCENE);
    }
}
