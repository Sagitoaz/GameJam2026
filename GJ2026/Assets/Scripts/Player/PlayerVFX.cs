using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Transform trailSpawnPoint; // Vị trí spawn trail (optional)
    [SerializeField] private Vector3 trailOffset = Vector3.zero; // Offset từ player
    [SerializeField] private bool enableTrailOnMove = true;
    [SerializeField] private float minSpeedForTrail = 0.5f;

    [Header("Movement Particles")]
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private float minSpeedForParticles = 1f;
    [SerializeField] private bool emitWhileMoving = true;

    [Header("Jump/Landing Particles")]
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem landParticles;

    [Header("Idle/Float Particles")]
    [SerializeField] private ParticleSystem idleParticles; // Hiệu ứng bay lơ lửng cho hồn ma
    
    private Rigidbody2D rb;
    private bool wasGrounded = true;

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

    private void Start()
    {
        // Bật idle particles nếu có
        if (idleParticles != null && !idleParticles.isPlaying)
        {
        
        // Update trail position nếu có offset mới
        if (trailSpawnPoint != null && trailSpawnPoint.parent == transform)
        {
            trailSpawnPoint.localPosition = trailOffset;
        }
            idleParticles.Play();
        }
    }

    private void Update()
    {
        if (rb == null) return;

        float speed = rb.linearVelocity.magnitude;

        // Xử lý trail
        HandleTrail(speed);

        // Xử lý movement particles
        HandleMovementParticles(speed);
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

    private void HandleMovementParticles(float speed)
    {
        if (moveParticles == null || !emitWhileMoving) return;

        if (speed >= minSpeedForParticles)
        {
            if (!moveParticles.isPlaying)
            {
                moveParticles.Play();
            }
        }
        else
        {
            if (moveParticles.isPlaying)
            {
                moveParticles.Stop();
            }
        }
    }

    /// <summary>
    /// Gọi khi player nhảy
    /// </summary>
    public void PlayJumpEffect()
    {
        if (jumpParticles != null)
        {
            jumpParticles.Play();
        }
    }

    /// <summary>
    /// Gọi khi player chạm đất
    /// </summary>
    public void PlayLandEffect()
    {
        if (landParticles != null)
        {
            landParticles.Play();
        }
    }

    /// <summary>
    /// Kiểm tra landing để tự động play effect
    /// </summary>
    public void CheckLanding(bool isGrounded)
    {
        if (isGrounded && !wasGrounded)
        {
            PlayLandEffect();
        }
        wasGrounded = isGrounded;
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
        
        // Gradient màu ma mị
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.5f, 0.8f, 1f, 1f), 0.0f), // Xanh nhạt
                new GradientColorKey(new Color(0.3f, 0.5f, 0.8f, 1f), 1.0f)  // Xanh đậm
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
