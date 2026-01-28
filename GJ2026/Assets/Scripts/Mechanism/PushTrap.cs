using UnityEngine;

public class PushTrap : MonoBehaviour
{
    [Header("Detect Zone")]
    [SerializeField] private Collider2D detectZone;

    [Header("Push Settings")]
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private float horizontalForce = 0f;
    [SerializeField] private float noGravityDuration = 0.25f;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnDetectEnter(other);
        }
    }
    public void OnDetectEnter(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb.linearVelocity.y > 0f) return;

        Vector2 pushVelocity = new Vector2(horizontalForce, pushForce);
        player.ApplyKnockbackNoGravity(pushVelocity, noGravityDuration);
    }
}
