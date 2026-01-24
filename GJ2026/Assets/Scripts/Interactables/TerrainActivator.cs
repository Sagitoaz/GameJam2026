using UnityEngine;

public enum ActivatorAction
{
    Show,      // Hiện terrain
    Hide,      // Ẩn terrain
    Toggle     // Chuyển đổi hiện/ẩn
}

public class TerrainActivator : MonoBehaviour
{
    [Header("Activator Settings")]
    [SerializeField] private string listenEventName = "Button01_Pressed";
    [SerializeField] private ActivatorAction action = ActivatorAction.Toggle;
    
    [Header("Target Objects")]
    [SerializeField] private GameObject[] targetObjects; // Các terrain/platform cần bật/tắt
    [SerializeField] private bool startActive = false; // Trạng thái ban đầu
    
    [Header("Animation (Optional)")]
    [SerializeField] private bool useAnimation = true;
    [SerializeField] private float animationDuration = 0.5f;
    
    private bool isActive;

    private void Start()
    {
        isActive = startActive;
        EventManager.Instance.Subscribe(listenEventName, OnEventReceived);
        
        // Set trạng thái ban đầu
        foreach (var obj in targetObjects)
        {
            if (obj != null)
                obj.SetActive(isActive);
        }
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe(listenEventName, OnEventReceived);
        }
    }

    private void OnEventReceived()
    {
        switch (action)
        {
            case ActivatorAction.Show:
                SetObjectsActive(true);
                break;

            case ActivatorAction.Hide:
                SetObjectsActive(false);
                break;

            case ActivatorAction.Toggle:
                isActive = !isActive;
                SetObjectsActive(isActive);
                break;
        }
    }

    private void SetObjectsActive(bool active)
    {
        foreach (var obj in targetObjects)
        {
            if (obj != null)
            {
                if (useAnimation && obj.TryGetComponent<SpriteRenderer>(out var sprite))
                {
                    StartCoroutine(AnimateFade(sprite, active));
                }
                else
                {
                    obj.SetActive(active);
                }
            }
        }
    }

    private System.Collections.IEnumerator AnimateFade(SpriteRenderer sprite, bool fadeIn)
    {
        if (fadeIn && !sprite.gameObject.activeSelf)
            sprite.gameObject.SetActive(true);

        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsed = 0f;

        Color color = sprite.color;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / animationDuration);
            color.a = alpha;
            sprite.color = color;
            yield return null;
        }

        color.a = endAlpha;
        sprite.color = color;

        if (!fadeIn)
            sprite.gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        // Vẽ line đến các target objects
        if (targetObjects != null)
        {
            foreach (var obj in targetObjects)
            {
                if (obj != null)
                {
                    Gizmos.DrawLine(transform.position, obj.transform.position);
                }
            }
        }
    }
}
