using UnityEngine;

public class DynamicCameraController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;
    
    [Header("Camera Settings")]
    [SerializeField] private float mergeDistance = 6f; // Khoảng cách để merge camera
    [SerializeField] private float splitDistance = 8f; // Khoảng cách để split camera
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 10f;
    [SerializeField] private float zoomPadding = 2f; // Buffer space xung quanh players
    [SerializeField] private float splitCameraZoom = 7f; // Zoom của split cameras (càng lớn càng rộng)
    
    [Header("Smooth Settings")]
    [SerializeField] private float positionSmoothTime = 0.15f; // Giảm để responsive hơn
    [SerializeField] private float zoomSmoothTime = 0.3f;
    [SerializeField] private float splitTransitionSpeed = 5f;
    [SerializeField] private bool useInterpolation = true; // Fix camera jittering
    [SerializeField] private float mergeTransitionDuration = 0.6f; // Thời gian slide camera khi merge/split (It Takes Two style)
    
    [Header("Split Screen Settings")]
    [SerializeField] private float splitLineThickness = 0.02f;
    [SerializeField] private Color splitLineColor = Color.black;
    
    [Header("Camera Bounds (Optional)")]
    [Tooltip("Giới hạn camera không đi ra ngoài map. Bật nếu không muốn thấy vùng ngoài map (void/background)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 boundsMin = new Vector2(-20f, -20f);
    [SerializeField] private Vector2 boundsMax = new Vector2(20f, 20f);
    
    private Camera mainCamera;
    private Camera player1Camera;
    private Camera player2Camera;
    
    private bool isSplit = false;
    private bool wasSplit = false; // Track previous state để detect transition
    private Vector3 positionVelocity;
    private float zoomVelocity;
    private float splitTransition = 0f; // 0 = merged, 1 = split
    
    // Velocity riêng cho mỗi split camera
    private Vector3 player1CamVelocity;
    private Vector3 player2CamVelocity;
    
    // Cache positions để tránh jitter
    private Vector3 lastTargetPosition;
    private Vector3 lastPlayer1Position;
    private Vector3 lastPlayer2Position;
    
    // Cache Rigidbody2D references để tránh GetComponent mỗi frame
    private Rigidbody2D player1Rb;
    private Rigidbody2D player2Rb;
    private Rigidbody2D mergedPlayerRb;
    private Transform cachedMergedTransform;
    
    // Smooth interpolation variables
    private Vector3 currentVelocity;
    private float currentZoomVelocity;
    
    // Transition smoothing (It Takes Two style)
    private bool isTransitioning = false;
    private float transitionTimer = 0f;
    private Vector3 transitionStartPos;
    private float transitionStartZoom;
    
    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found!");
            return;
        }
        
        SetupSplitCameras();
        
        // Cache Rigidbody2D references
        if (player1 != null) player1Rb = player1.GetComponent<Rigidbody2D>();
        if (player2 != null) player2Rb = player2.GetComponent<Rigidbody2D>();
    }
    
    private void SetupSplitCameras()
    {
        // Tạo camera cho Player 1 (left/top)
        GameObject cam1Obj = new GameObject("Player1Camera");
        cam1Obj.transform.SetParent(transform);
        player1Camera = cam1Obj.AddComponent<Camera>();
        player1Camera.CopyFrom(mainCamera);
        player1Camera.depth = mainCamera.depth + 1;
        player1Camera.enabled = false;
        
        // Tạo camera cho Player 2 (right/bottom)
        GameObject cam2Obj = new GameObject("Player2Camera");
        cam2Obj.transform.SetParent(transform);
        player2Camera = cam2Obj.AddComponent<Camera>();
        player2Camera.CopyFrom(mainCamera);
        player2Camera.depth = mainCamera.depth + 2;
        player2Camera.enabled = false;

        
    }
    
    private void LateUpdate()
    {
        if (player1 == null || player2 == null) return;
        
        // Check trạng thái merge/separate từ GameManager
        bool shouldBeMerged = GameManager.Instance.PlayerMode != PlayerMode.None;
        bool newIsSplit = !shouldBeMerged;
        
        // Detect state change và bắt đầu smooth transition
        if (newIsSplit != isSplit)
        {
            OnCameraStateChanged(isSplit, newIsSplit);
            wasSplit = isSplit;
            isSplit = newIsSplit;
        }
        
        // Update transition timer
        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            if (transitionTimer >= mergeTransitionDuration)
            {
                isTransitioning = false;
                transitionTimer = 0f;
            }
        }
        
        // Smooth transition với wipe effect
        float targetTransition = isSplit ? 1f : 0f;
        splitTransition = Mathf.MoveTowards(splitTransition, targetTransition, splitTransitionSpeed * Time.deltaTime);
        
        if (splitTransition <= 0.01f)
        {
            // Hoàn toàn merged
            UpdateMergedCamera(true);
        }
        else if (splitTransition >= 0.99f)
        {
            // Hoàn toàn split
            UpdateSplitCamera();
            // QUAN TRỌNG: Vẫn cập nhật vị trí main camera (dù disabled) để khi merge không bị giật
            UpdateMainCameraPositionSilent();
        }
        else
        {
            // Đang transition với wipe effect
            UpdateWipeTransition();
        }
    }
    
    /// <summary>
    /// Cập nhật vị trí main camera mà không bật/tắt nó
    /// Dùng khi đang split để main camera luôn ở đúng vị trí khi cần merge
    /// </summary>
    private void UpdateMainCameraPositionSilent()
    {
        // Tính vị trí target dựa trên players
        Vector3 p1Pos = GetPlayerPositionCached(player1, player1Rb);
        Vector3 p2Pos = GetPlayerPositionCached(player2, player2Rb);
        Vector3 midpoint = (p1Pos + p2Pos) * 0.5f;
        Vector3 targetPosition = new Vector3(midpoint.x, midpoint.y, mainCamera.transform.position.z);
        
        // Auto zoom để fit cả 2 player
        float distance = Vector2.Distance(p1Pos, p2Pos);
        float targetZoom = Mathf.Clamp(distance * 0.5f + zoomPadding, minZoom, maxZoom);
        
        if (useBounds)
        {
            targetPosition = ApplyBounds(targetPosition);
        }
        
        // Cập nhật trực tiếp (không smooth) để luôn sẵn sàng khi merge
        mainCamera.transform.position = targetPosition;
        mainCamera.orthographicSize = targetZoom;
    }
    
    /// <summary>
    /// Được gọi khi camera chuyển từ split sang merge hoặc ngược lại
    /// Reset velocity và setup smooth transition giống It Takes Two
    /// </summary>
    private void OnCameraStateChanged(bool wasSplit, bool nowSplit)
    {
        // Reset tất cả velocity để tránh camera giật
        currentVelocity = Vector3.zero;
        currentZoomVelocity = 0f;
        positionVelocity = Vector3.zero;
        zoomVelocity = 0f;
        player1CamVelocity = Vector3.zero;
        player2CamVelocity = Vector3.zero;
        
        // Bắt đầu smooth transition
        isTransitioning = true;
        transitionTimer = 0f;
        
        // Main camera đã được cập nhật vị trí liên tục trong UpdateMainCameraPositionSilent()
        // nên không cần snap nữa - chỉ cần lưu vị trí hiện tại làm start
        transitionStartPos = mainCamera.transform.position;
        transitionStartZoom = mainCamera.orthographicSize;
    }
    
    private void UpdateMergedCamera(bool isMerged)
    {
        if (isMerged)
        {
            mainCamera.enabled = true;
            player1Camera.enabled = false;
            player2Camera.enabled = false;
        }
        
        Vector3 targetPosition;
        float targetZoom;
        
        // Nếu đang merged, follow merged player
        if (GameManager.Instance.PlayerMode != PlayerMode.None && GameManager.Instance.CurrentGameMode != null)
        {
            // Cache merged player reference để tránh GetComponent mỗi frame
            Transform currentMergedTransform = GameManager.Instance.CurrentGameMode.transform;
            if (cachedMergedTransform != currentMergedTransform)
            {
                cachedMergedTransform = currentMergedTransform;
                mergedPlayerRb = currentMergedTransform.GetComponent<Rigidbody2D>();
            }
            
            Vector3 mergedPos;
            if (useInterpolation && mergedPlayerRb != null)
            {
                // Lấy position từ Rigidbody2D để sync với physics interpolation
                mergedPos = mergedPlayerRb.position;
            }
            else
            {
                mergedPos = currentMergedTransform.position;
            }
            
            targetPosition = new Vector3(mergedPos.x, mergedPos.y, mainCamera.transform.position.z);
            targetZoom = minZoom;
        }
        else
        {
            // Tính midpoint giữa 2 player separate
            Vector3 p1Pos = GetPlayerPositionCached(player1, player1Rb);
            Vector3 p2Pos = GetPlayerPositionCached(player2, player2Rb);
            Vector3 midpoint = (p1Pos + p2Pos) * 0.5f;
            targetPosition = new Vector3(midpoint.x, midpoint.y, mainCamera.transform.position.z);
            
            // Auto zoom để fit cả 2 player
            float distance = Vector2.Distance(p1Pos, p2Pos);
            targetZoom = Mathf.Clamp(distance * 0.5f + zoomPadding, minZoom, maxZoom);
        }
        
        if (useBounds)
        {
            targetPosition = ApplyBounds(targetPosition);
        }
        
        // Smooth camera movement với It Takes Two style transition
        if (isTransitioning)
        {
            // Trong quá trình transition, sử dụng smooth lerp với easing
            float t = transitionTimer / mergeTransitionDuration;
            // Smoother step easing (quintic) cho cảm giác mượt mà hơn
            float smoothT = SmootherStep(t);
            
            // Lerp mượt từ start position đến target
            mainCamera.transform.position = Vector3.Lerp(transitionStartPos, targetPosition, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(transitionStartZoom, targetZoom, smoothT);
        }
        else
        {
            // Normal smooth follow sau khi transition xong
            mainCamera.transform.position = Vector3.SmoothDamp(
                mainCamera.transform.position,
                targetPosition,
                ref currentVelocity,
                positionSmoothTime
            );
            
            mainCamera.orthographicSize = Mathf.SmoothDamp(
                mainCamera.orthographicSize,
                targetZoom,
                ref currentZoomVelocity,
                zoomSmoothTime
            );
        }
        
        lastTargetPosition = targetPosition;
    }
    
    /// <summary>
    /// Smooth step easing function (Hermite)
    /// </summary>
    private float SmoothStep(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }
    
    /// <summary>
    /// Smoother step easing (quintic) - mượt hơn SmoothStep
    /// </summary>
    private float SmootherStep(float t)
    {
        t = Mathf.Clamp01(t);
        // 6t^5 - 15t^4 + 10t^3
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    // Helper method để lấy position có interpolation (với cached Rigidbody)
    private Vector3 GetPlayerPositionCached(Transform playerTransform, Rigidbody2D rb)
    {
        if (useInterpolation && rb != null)
        {
            return rb.position;
        }
        return playerTransform.position;
    }
    
    // Helper method cho legacy calls
    private Vector3 GetPlayerPosition(Transform playerTransform)
    {
        if (!useInterpolation) return playerTransform.position;
        
        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            return rb.position;
        }
        return playerTransform.position;
    }
    
    private void UpdateSplitCamera()
    {
        mainCamera.enabled = false;
        player1Camera.enabled = true;
        player2Camera.enabled = true;
        
        // Luôn split vertical (left/right - dọc)
        player1Camera.rect = new Rect(0, 0, 0.5f, 1f);
        player2Camera.rect = new Rect(0.5f, 0, 0.5f, 1f);
        
        // Follow mỗi player với velocity riêng
        Vector3 p1Pos = GetPlayerPosition(player1);
        Vector3 p2Pos = GetPlayerPosition(player2);
        FollowPlayer(player1Camera, p1Pos, ref player1CamVelocity);
        FollowPlayer(player2Camera, p2Pos, ref player2CamVelocity);
    }
    
    private void UpdateWipeTransition()
    {
        // Hiển thị cả 2 camera trong quá trình transition
        mainCamera.enabled = false;
        player1Camera.enabled = true;
        player2Camera.enabled = true;
        
        // Animate wipe effect
        if (isSplit)
        {
            // Đang transition từ merged → split
            // Bắt đầu từ full screen, dần chia nhỏ
            float progress = splitTransition;
            float leftWidth = Mathf.Lerp(1f, 0.5f, progress);
            
            player1Camera.rect = new Rect(0, 0, leftWidth, 1f);
            player2Camera.rect = new Rect(leftWidth, 0, 1f - leftWidth, 1f);
        }
        else
        {
            // Đang transition từ split → merged
            // Mở rộng 1 bên, thu nhỏ bên kia
            float progress = 1f - splitTransition;
            float leftWidth = Mathf.Lerp(0.5f, 1f, progress);
            
            player1Camera.rect = new Rect(0, 0, leftWidth, 1f);
            player2Camera.rect = new Rect(leftWidth, 0, 1f - leftWidth, 1f);
        }
        
        // Follow player trong quá trình transition
        Vector3 p1Pos = GetPlayerPosition(player1);
        Vector3 p2Pos = GetPlayerPosition(player2);
        FollowPlayer(player1Camera, p1Pos, ref player1CamVelocity);
        FollowPlayer(player2Camera, p2Pos, ref player2CamVelocity);
    }
    
    private void FollowPlayer(Camera cam, Vector3 targetPos, ref Vector3 velocity)
    {
        Vector3 camPos = new Vector3(targetPos.x, targetPos.y, cam.transform.position.z);
        
        if (useBounds)
        {
            camPos = ApplyBoundsSplit(camPos, cam);
        }
        
        // Sử dụng SmoothDamp để mượt và consistent
        cam.transform.position = Vector3.SmoothDamp(
            cam.transform.position,
            camPos,
            ref velocity,
            positionSmoothTime
        );
        
        // Dùng splitCameraZoom để điều chỉnh vùng nhìn
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, splitCameraZoom, Time.deltaTime * 5f);
    }
    
    private Vector3 ApplyBounds(Vector3 position)
    {
        float camHeight = mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;
        
        position.x = Mathf.Clamp(position.x, boundsMin.x + camWidth, boundsMax.x - camWidth);
        position.y = Mathf.Clamp(position.y, boundsMin.y + camHeight, boundsMax.y - camHeight);
        
        return position;
    }
    
    private Vector3 ApplyBoundsSplit(Vector3 position, Camera cam)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        
        position.x = Mathf.Clamp(position.x, boundsMin.x + camWidth, boundsMax.x - camWidth);
        position.y = Mathf.Clamp(position.y, boundsMin.y + camHeight, boundsMax.y - camHeight);
        
        return position;
    }
    
    private void OnGUI()
    {
        if (splitTransition <= 0.01f) return; // Không vẽ khi hoàn toàn merged
        
        // Vẽ wipe line
        GUI.color = splitLineColor;
        
        float linePosition;
        if (isSplit)
        {
            // Split: line di chuyển từ trái sang giữa
            linePosition = Mathf.Lerp(0f, 0.5f, splitTransition);
        }
        else
        {
            // Merge: line di chuyển từ giữa sang phải
            linePosition = Mathf.Lerp(0.5f, 1f, 1f - splitTransition);
        }
        
        float lineX = Screen.width * linePosition - Screen.width * splitLineThickness * 0.5f;
        GUI.DrawTexture(
            new Rect(lineX, 0, Screen.width * splitLineThickness, Screen.height),
            Texture2D.whiteTexture
        );
    }
    
    private void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((boundsMin.x + boundsMax.x) * 0.5f, (boundsMin.y + boundsMax.y) * 0.5f, 0);
            Vector3 size = new Vector3(boundsMax.x - boundsMin.x, boundsMax.y - boundsMin.y, 0);
            Gizmos.DrawWireCube(center, size);
        }
        
        if (player1 != null && player2 != null)
        {
            // Vẽ merge distance
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player1.position, mergeDistance * 0.5f);
            Gizmos.DrawWireSphere(player2.position, mergeDistance * 0.5f);
            
            // Vẽ split distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player1.position, splitDistance * 0.5f);
            Gizmos.DrawWireSphere(player2.position, splitDistance * 0.5f);
        }
    }
}
