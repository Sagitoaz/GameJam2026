using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorBehavior
{
    OpenClose,      // Mở rồi tự đóng lại
    Toggle,         // Chuyển đổi mở/đóng
    OneWay          // Chỉ mở một lần, không đóng lại
}

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private string[] listenEventNames = new string[] { "Button01_Pressed" }; // Danh sách event cần thỏa mãn
    [SerializeField] private bool requireAllEvents = true; // true = cần tất cả, false = cần ít nhất 1
    [SerializeField] private DoorBehavior doorBehavior = DoorBehavior.OpenClose;
    [SerializeField] private float openDuration = 3f; // Thời gian mở (với OpenClose)
    
    [Header("Animation")]
    [SerializeField] private Transform doorVisual;
    [SerializeField] private Vector3 openOffset = Vector3.up * 2f; // Cửa mở lên trên
    [SerializeField] private float animationSpeed = 2f;
    
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private Coroutine closeCoroutine;
    private BoxCollider2D doorCollider;
    private HashSet<string> activeEvents = new HashSet<string>();

    private void Start()
    {
        if (doorVisual == null)
            doorVisual = transform;

        closedPosition = doorVisual.localPosition;
        openPosition = closedPosition + openOffset;
        doorCollider = GetComponent<BoxCollider2D>();

        // Subscribe tất cả các event
        foreach (string eventName in listenEventNames)
        {
            EventManager.Instance.Subscribe(eventName, () => OnEventPressed(eventName));
            EventManager.Instance.Subscribe(eventName + "_Released", () => OnEventReleased(eventName));
        }
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            foreach (string eventName in listenEventNames)
            {
                EventManager.Instance.Unsubscribe(eventName, () => OnEventPressed(eventName));
                EventManager.Instance.Unsubscribe(eventName + "_Released", () => OnEventReleased(eventName));
            }
        }
    }

    private void OnEventPressed(string eventName)
    {
        activeEvents.Add(eventName);
        CheckDoorCondition();
    }

    private void OnEventReleased(string eventName)
    {
        activeEvents.Remove(eventName);
        CheckDoorCondition();
    }

    private void CheckDoorCondition()
    {
        bool shouldOpen = false;

        if (requireAllEvents)
        {
            // Kiểm tra TẤT CẢ event có được active không
            shouldOpen = activeEvents.Count == listenEventNames.Length;
        }
        else
        {
            // Chỉ cần ít nhất 1 event active
            shouldOpen = activeEvents.Count > 0;
        }

        // Xử lý theo behavior
        if (doorBehavior == DoorBehavior.Toggle)
        {
            if (shouldOpen && !isOpen)
                OpenDoor();
            else if (!shouldOpen && isOpen)
                CloseDoor();
        }
        else if (doorBehavior == DoorBehavior.OpenClose)
        {
            if (shouldOpen)
            {
                OpenDoor();
                if (closeCoroutine != null) StopCoroutine(closeCoroutine);
                closeCoroutine = StartCoroutine(CloseAfterDelay());
            }
            else
            {
                if (closeCoroutine != null) StopCoroutine(closeCoroutine);
                CloseDoor();
            }
        }
        else if (doorBehavior == DoorBehavior.OneWay)
        {
            if (shouldOpen && !isOpen)
                OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        StopAllCoroutines();
        StartCoroutine(AnimateDoor(openPosition));
        
        if (doorCollider != null)
            doorCollider.enabled = false;
    }

    private void CloseDoor()
    {
        isOpen = false;
        StopAllCoroutines();
        StartCoroutine(AnimateDoor(closedPosition));
        
        if (doorCollider != null)
            doorCollider.enabled = true;
    }

    private IEnumerator AnimateDoor(Vector3 targetPosition)
    {
        while (Vector3.Distance(doorVisual.localPosition, targetPosition) > 0.01f)
        {
            doorVisual.localPosition = Vector3.Lerp(
                doorVisual.localPosition,
                targetPosition,
                animationSpeed * Time.deltaTime
            );
            yield return null;
        }
        doorVisual.localPosition = targetPosition;
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(openDuration);
        CloseDoor();
    }
}
