using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Transform _origin;
    [SerializeField] private Vector2 _size = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _offsetY = 0.05f;
    [Header("Debug")]
    [SerializeField] private bool _drawGizmos = true;

    public bool IsGrounded { get; private set; }
    public bool WasGroundedLastFrame { get; private set; }
    public void Check()
    {
        WasGroundedLastFrame = IsGrounded;
        Vector2 originPos = _origin ? (Vector2)_origin.position : (Vector2)transform.position;
        RaycastHit2D hit = Physics2D.BoxCast(originPos, _size, 0f, Vector2.down, _offsetY, _groundLayer);
        IsGrounded = hit.collider != null;
    }
    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos) return;
        Gizmos.color = IsGrounded ? Color.green : Color.red;

        Vector2 originPos = _origin ? (Vector2)_origin.position : (Vector2)transform.position;
        Vector2 boxCenter = originPos + Vector2.down * _offsetY * 0.5f;

        Gizmos.matrix = Matrix4x4.TRS(boxCenter, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(_size.x, _size.y + _offsetY, 0f));
    }
}