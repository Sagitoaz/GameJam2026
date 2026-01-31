using UnityEngine;

public class ManualPanel : Panel
{
    public void OnClickCloseButton()
    {
        PanelManager.Instance.ClosePanel(GameConfig.PANEL_MANUAL);
    }
}
