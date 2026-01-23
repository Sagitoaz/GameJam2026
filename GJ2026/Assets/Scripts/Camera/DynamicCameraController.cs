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
    [SerializeField] private float positionSmoothTime = 0.3f;
    [SerializeField] private float zoomSmoothTime = 0.3f;
    [SerializeField] private float splitTransitionSpeed = 5f;
    
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
    private Vector3 positionVelocity;
    private float zoomVelocity;
    private float splitTransition = 0f; // 0 = merged, 1 = split
    
    // Velocity riêng cho mỗi split camera
    private Vector3 player1CamVelocity;
    private Vector3 player2CamVelocity;
    
    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found!");
            return;
        }
        
        SetupSplitCameras();
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
        
        // Nếu đang merged, chỉ follow merged player, không split
        if (GameManager.Instance.PlayerMode != PlayerMode.None)
        {
            isSplit = false;
            splitTransition = 0f;
            UpdateMergedCamera();
            return;
        }
        
        // Chỉ split khi 2 player separate
        float distance = Vector2.Distance(player1.position, player2.position);
        
        // Hysteresis: tránh flicker giữa split/merge
        if (!isSplit && distance > splitDistance)
        {
            isSplit = true;
        }
        else if (isSplit && distance < mergeDistance)
        {
            isSplit = false;
        }
        
        // Smooth transition
        float targetTransition = isSplit ? 1f : 0f;
        splitTransition = Mathf.Lerp(splitTransition, targetTransition, splitTransitionSpeed * Time.deltaTime);
        
        if (splitTransition < 0.1f)
        {
            UpdateMergedCamera();
        }
        else if (splitTransition > 0.9f)
        {
            UpdateSplitCamera();
        }
        else
        {
            // Đang transition
            UpdateTransitionCamera();
        }
    }
    
    private void UpdateMergedCamera()
    {
        mainCamera.enabled = true;
        player1Camera.enabled = false;
        player2Camera.enabled = false;
        
        Vector3 targetPosition;
        float targetZoom;
        
        // Nếu đang merged, follow merged player
        if (GameManager.Instance.PlayerMode != PlayerMode.None && GameManager.Instance.CurrentGameMode != null)
        {
            Vector3 mergedPos = GameManager.Instance.CurrentGameMode.transform.position;
            targetPosition = new Vector3(mergedPos.x, mergedPos.y, mainCamera.transform.position.z);
            targetZoom = minZoom; // Zoom in khi merged
        }
        else
        {
            // Tính midpoint giữa 2 player separate
            Vector3 midpoint = (player1.position + player2.position) * 0.5f;
            targetPosition = new Vector3(midpoint.x, midpoint.y, mainCamera.transform.position.z);
            
            // Auto zoom để fit cả 2 player
            float distance = Vector2.Distance(player1.position, player2.position);
            targetZoom = Mathf.Clamp(distance * 0.5f + zoomPadding, minZoom, maxZoom);
        }
        
        if (useBounds)
        {
            targetPosition = ApplyBounds(targetPosition);
        }
        
        mainCamera.transform.position = Vector3.SmoothDamp(
            mainCamera.transform.position,
            targetPosition,
            ref positionVelocity,
            positionSmoothTime
        );
        
        mainCamera.orthographicSize = Mathf.SmoothDamp(
            mainCamera.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            zoomSmoothTime
        );
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
        FollowPlayer(player1Camera, player1.position, ref player1CamVelocity);
        FollowPlayer(player2Camera, player2.position, ref player2CamVelocity);
    }
    
    private void UpdateTransitionCamera()
    {
        // Smooth transition: dùng merged camera nhưng adjust zoom
        mainCamera.enabled = true;
        player1Camera.enabled = false;
        player2Camera.enabled = false;
        
        UpdateMergedCamera();
    }
    
    private void FollowPlayer(Camera cam, Vector3 targetPos, ref Vector3 velocity)
    {
        Vector3 camPos = new Vector3(targetPos.x, targetPos.y, cam.transform.position.z);
        
        if (useBounds)
        {
            camPos = ApplyBoundsSplit(camPos, cam);
        }
        
        cam.transform.position = Vector3.SmoothDamp(
            cam.transform.position,
            camPos,
            ref velocity,
            positionSmoothTime
        );
        
        // Dùng splitCameraZoom để điều chỉnh vùng nhìn
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, splitCameraZoom, Time.deltaTime * 2f);
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
        if (!isSplit || splitTransition < 0.5f) return;
        
        // Vẽ vertical split line (dọc)
        GUI.color = splitLineColor;
        
        float lineX = Screen.width * 0.5f - Screen.width * splitLineThickness * 0.5f;
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
