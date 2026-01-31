using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Transform trailSpawnPoint; // Vị trí spawn trail (optional)
    [SerializeField] private Vector3 trailOffset = Vector3.zero; // Offset từ player
    [SerializeField] private bool enableTrailOnMove = true;
    [SerializeField] private float minSpeedForTrail = 0.5f;
    
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Tạo Trail Renderer nếu chưa có
        if (trailRenderer == null)
        {
            // Nếu có spawn point thì dùng, không thì tạo object con
            if (trailSpawnPoint != null)
            {
                trailRenderer = trailSpawnPoint.GetComponent<TrailRenderer>();
                if (trailRenderer == null)
                {
                    trailRenderer = trailSpawnPoint.gameObject.AddComponent<TrailRenderer>();
                }
            }
            else
            {
                // Tạo child object cho trail
                GameObject trailObj = new GameObject("Trail");
                trailObj.transform.SetParent(transform);
                trailObj.transform.localPosition = trailOffset;
                trailObj.transform.localRotation = Quaternion.identity;
                trailRenderer = trailObj.AddComponent<TrailRenderer>();
                trailSpawnPoint = trailObj.transform;
            }
            
            SetupDefaultTrail();
        }

        // Disable trail ban đầu
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
    }

    private void Update()
    {
        if (rb == null) return;

        // Lấy tốc độ ngang (không tính vertical)
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);

        // Xử lý trail - chỉ khi di chuyển ngang
        HandleTrail(horizontalSpeed);
        
        // Update trail position nếu có offset mới
        if (trailSpawnPoint != null && trailSpawnPoint.parent == transform)
        {
            trailSpawnPoint.localPosition = trailOffset;
        }
    }

    private void HandleTrail(float speed)
    {
        if (trailRenderer == null || !enableTrailOnMove) return;

        if (speed >= minSpeedForTrail)
        {
            trailRenderer.emitting = true;
        }
        else
        {
            trailRenderer.emitting = false;
        }
    }

    private void SetupDefaultTrail()
    {
        // Setup trail mặc định cho hồn ma
        trailRenderer.time = 0.5f;
        trailRenderer.startWidth = 0.3f;
        trailRenderer.endWidth = 0.05f;
        trailRenderer.minVertexDistance = 0.1f;
        
        // Tạo material với Sprites/Default shader
        Material trailMat = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.material = trailMat;
        
        // Set sorting để trail nằm sau player
        trailRenderer.sortingLayerName = "Default";
        trailRenderer.sortingOrder = -1; // Âm để nằm sau player
        
        // Gradient màu trắng xanh nhạt
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.9f, 0.95f, 1f, 1f), 0.0f), // Trắng xanh nhạt
                new GradientColorKey(new Color(0.7f, 0.85f, 0.95f, 1f), 1.0f)  // Xanh nhạt hơn
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        trailRenderer.colorGradient = gradient;
        
        // Shadow casting
        trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trailRenderer.receiveShadows = false;
    }

    /// <summary>
    /// Enable/Disable trail
    /// </summary>
    public void SetTrailEnabled(bool enabled)
    {
        enableTrailOnMove = enabled;
        if (!enabled && trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
    }

    /// <summary>
    /// Xóa trail hiện tại
    /// </summary>
    public void ClearTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }
}

