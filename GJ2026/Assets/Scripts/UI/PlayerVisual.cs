using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private string _boolAnim = "isPull";
    private bool _isPull = false;
    private Vector3 _lastPosition;
    private float _movementThreshold = 0.01f;
    private float _movementThresholdSqr;

    void Start()
    {
        _lastPosition = transform.position;
        _movementThresholdSqr = _movementThreshold * _movementThreshold;
    }

    void Update()
    {
        Vector3 currentPos = transform.position;
        
        // Ngắt sớm nếu vị trí không đổi
        if (currentPos == _lastPosition) return;
        
        // Dùng sqrMagnitude thay vì Distance để tránh sqrt
        float sqrDistance = (currentPos - _lastPosition).sqrMagnitude;
        bool isMoving = sqrDistance > _movementThresholdSqr;
        
        // Chỉ set animator khi state thực sự thay đổi
        if (isMoving != _isPull)
        {
            _isPull = isMoving;
            animator?.SetBool(_boolAnim, isMoving);
        }
        
        _lastPosition = currentPos;
    }
}
