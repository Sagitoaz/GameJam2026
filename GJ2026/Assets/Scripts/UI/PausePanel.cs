using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanel : Panel
{
    [SerializeField] private GamePlayPanel GamePlayPanel;
    public void OnClickResumeButton()
    {
        this.gameObject.SetActive(false);
        Time.timeScale = 1f;
        GamePlayPanel.ToggleCamera(true);
    }
    public void OnClickSettingsButton()
    {
        PanelManager.Instance.OpenPanel(GameConfig.PANEL_SETTING);
    }
    public void OnClickHomeButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Intro");
    }
}
