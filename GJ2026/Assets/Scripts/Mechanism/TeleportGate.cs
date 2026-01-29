using UnityEngine;

public enum GateType
{
    In,
    Out
}

public class TeleportGate : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField] private GateType gateType;

    [Tooltip("Only Used For IN")]
    [SerializeField] private TeleportGate targetGate;

    [Header("Teleport Settings")]
    // [SerializeField] private bool keepRotation = false;
    [SerializeField] private Vector3 positionOffset;

    private bool isTeleporting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gateType != GateType.In) return;

        if (isTeleporting) return;

        if (!other.CompareTag("Player")) return;

        if (targetGate == null)
        {
            Debug.LogWarning($"{name} chưa được gán Target Gate!");
            return;
        }

        Teleport(other.transform);
    }

    private void Teleport(Transform target)
    {
        isTeleporting = true;

        target.position = targetGate.transform.position + positionOffset;

        // if (!keepRotation)
        // {
        //     target.rotation = targetGate.transform.rotation;
        // }

        Invoke(nameof(ResetTeleport), 0.2f);
        Debug.Log("Teleport Successfully");     
    }

    private void ResetTeleport()
    {
        isTeleporting = false;
    }
}
