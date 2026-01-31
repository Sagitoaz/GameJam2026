using UnityEngine;

public class ManualPanel : Panel
{
    public void OnClickCloseButton()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.click);
        PanelManager.Instance.ClosePanel(GameConfig.PANEL_MANUAL);
    }
}
