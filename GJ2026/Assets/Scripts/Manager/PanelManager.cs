using UnityEngine;
using System.Collections.Generic;

public class PanelManager : Singleton<PanelManager>
{
    [Header("Panel Settings")]
    [SerializeField] private Transform panelContainer;
    
    private Dictionary<string, GameObject> loadedPanels = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> activePanels = new Dictionary<string, GameObject>();

    public override void Awake()
    {
        base.Awake();
        
        if (panelContainer == null)
        {
            Debug.LogError("[PanelManager] Panel Container chưa được gán! Hãy kéo Canvas vào Inspector.");
        }
    }

    /// <summary>
    /// Hiển thị panel từ Resources/Panels/
    /// </summary>
    public GameObject ShowPanel(string panelName)
    {
        // Nếu panel đang active, trả về luôn
        if (activePanels.ContainsKey(panelName))
        {
            Debug.Log($"[PanelManager] Panel {panelName} đã được mở rồi");
            return activePanels[panelName];
        }
        
        GameObject panel = GetOrLoadPanel(panelName);
        
        if (panel != null)
        {
            panel.SetActive(true);
            activePanels[panelName] = panel;
            Debug.Log($"[PanelManager] Đã hiển thị panel: {panelName}");
        }
        
        return panel;
    }

    /// <summary>
    /// Ẩn panel
    /// </summary>
    public void HidePanel(string panelName)
    {
        if (activePanels.ContainsKey(panelName))
        {
            activePanels[panelName].SetActive(false);
            activePanels.Remove(panelName);
            Debug.Log($"[PanelManager] Đã ẩn panel: {panelName}");
        }
    }

    /// <summary>
    /// Đóng và destroy panel
    /// </summary>
    public void ClosePanel(string panelName)
    {
        if (activePanels.ContainsKey(panelName))
        {
            activePanels.Remove(panelName);
        }
        
        if (loadedPanels.ContainsKey(panelName))
        {
            Destroy(loadedPanels[panelName]);
            loadedPanels.Remove(panelName);
            Debug.Log($"[PanelManager] Đã đóng panel: {panelName}");
        }
    }

    /// <summary>
    /// Kiểm tra panel có đang hiển thị không
    /// </summary>
    public bool IsPanelActive(string panelName)
    {
        return activePanels.ContainsKey(panelName);
    }

    /// <summary>
    /// Đóng tất cả panel
    /// </summary>
    public void CloseAllPanels()
    {
        foreach (var panel in loadedPanels.Values)
        {
            if (panel != null)
            {
                Destroy(panel);
            }
        }
        
        loadedPanels.Clear();
        activePanels.Clear();
        Debug.Log("[PanelManager] Đã đóng tất cả panel");
    }

    /// <summary>
    /// Ẩn tất cả panel (không destroy)
    /// </summary>
    public void HideAllPanels()
    {
        foreach (var panel in activePanels.Values)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
        
        activePanels.Clear();
        Debug.Log("[PanelManager] Đã ẩn tất cả panel");
    }

    private GameObject GetOrLoadPanel(string panelName)
    {
        // Kiểm tra xem đã load chưa
        if (loadedPanels.ContainsKey(panelName))
        {
            return loadedPanels[panelName];
        }
        
        // Load từ Resources/Panels/
        GameObject prefab = Resources.Load<GameObject>($"Panels/{panelName}");
        
        if (prefab == null)
        {
            Debug.LogError($"[PanelManager] Không tìm thấy panel prefab: {panelName} trong Resources/Panels/");
            return null;
        }
        
        // Instantiate panel
        GameObject panel = Instantiate(prefab, panelContainer);
        panel.name = panelName;
        panel.SetActive(false);
        
        loadedPanels[panelName] = panel;
        
        return panel;
    }

    /// <summary>
    /// Unload panel khỏi cache (nhưng không destroy nếu đang active)
    /// </summary>
    public void UnloadPanel(string panelName)
    {
        if (!activePanels.ContainsKey(panelName) && loadedPanels.ContainsKey(panelName))
        {
            Destroy(loadedPanels[panelName]);
            loadedPanels.Remove(panelName);
        }
    }
}
