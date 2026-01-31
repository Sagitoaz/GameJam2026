using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanel : Panel
{
    [SerializeField] private GamePlayPanel GamePlayPanel;
    public void OnClickResumeButton()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.click);
        this.gameObject.SetActive(false);
        Time.timeScale = 1f;
        GamePlayPanel.ToggleCamera(true);
    }
    public void OnClickSettingsButton()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.click);
        PanelManager.Instance.OpenPanel(GameConfig.PANEL_SETTING);
    }
    public void OnClickHomeButton()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.click);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Intro");
    }
}
