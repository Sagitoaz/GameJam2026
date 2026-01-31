using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorBehavior
{
    OpenClose,
    Toggle,
    OneWay
}

public enum OpenCloseMode
{
    AutoClose,         // Mở rồi tự đóng
    CloseOnRelease     // Đóng khi button bị nhả
}

public enum DoorControlMode
{
    SelfListen,         // Door tự nghe event (DEFAULT)
    SequenceControlled  // Door bị điều khiển bởi DoorSequenceController
}

public class Door : MonoBehaviour
{
    [Header("Control Mode")]
    [SerializeField] private DoorControlMode controlMode = DoorControlMode.SelfListen;

    [Header("Door Settings")]
    [SerializeField] private string[] listenEventNames = new string[] { "Button01_Pressed" };
    [SerializeField] private bool requireAllEvents = true;
    [SerializeField] private DoorBehavior doorBehavior = DoorBehavior.OpenClose;

    [Header("OpenClose Settings")]
    [SerializeField] private OpenCloseMode openCloseMode = OpenCloseMode.AutoClose;
    [SerializeField] private float openDuration = 3f; // chỉ dùng cho AutoClose

    [Header("Animation")]
    [SerializeField] private Transform doorVisual;
    [SerializeField] private Vector3 openOffset = Vector3.up * 2f;
    [SerializeField] private float animationSpeed = 2f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;

    private Coroutine closeCoroutine;
    private Coroutine animateCoroutine;

    private BoxCollider2D doorCollider;
    private HashSet<string> activeEvents = new HashSet<string>();

    private void Start()
    {
        if (doorVisual == null)
            doorVisual = transform;

        closedPosition = doorVisual.localPosition;
        openPosition = closedPosition + openOffset;

        doorCollider = GetComponent<BoxCollider2D>();

        // ❗ Nếu Door bị Sequence điều khiển → KHÔNG tự subscribe event
        if (controlMode == DoorControlMode.SequenceControlled)
            return;

        foreach (string eventName in listenEventNames)
        {
            EventManager.Instance.Subscribe(eventName, () => OnEventPressed(eventName));
            EventManager.Instance.Subscribe(eventName + "_Released", () => OnEventReleased(eventName));
        }
    }

    private void OnDestroy()
    {
        if (EventManager.Instance == null) return;
        if (controlMode == DoorControlMode.SequenceControlled) return;

        foreach (string eventName in listenEventNames)
        {
            EventManager.Instance.Unsubscribe(eventName, () => OnEventPressed(eventName));
            EventManager.Instance.Unsubscribe(eventName + "_Released", () => OnEventReleased(eventName));
        }
    }

    // ===================== EVENT HANDLING =====================

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
        if (controlMode == DoorControlMode.SequenceControlled)
            return;

        bool shouldOpen;

        if (requireAllEvents)
            shouldOpen = activeEvents.Count == listenEventNames.Length;
        else
            shouldOpen = activeEvents.Count > 0;

        switch (doorBehavior)
        {
            case DoorBehavior.Toggle:
                if (shouldOpen && !isOpen)
                    OpenDoor();
                else if (!shouldOpen && isOpen)
                    CloseDoor();
                break;

            case DoorBehavior.OneWay:
                if (shouldOpen && !isOpen)
                    OpenDoor();
                break;

            case DoorBehavior.OpenClose:
                HandleOpenClose(shouldOpen);
                break;
        }
    }

    private void HandleOpenClose(bool shouldOpen)
    {
        if (openCloseMode == OpenCloseMode.AutoClose)
        {
            if (shouldOpen)
            {
                OpenDoor();

                if (closeCoroutine != null)
                    StopCoroutine(closeCoroutine);

                closeCoroutine = StartCoroutine(CloseAfterDelay());
            }
        }
        else if (openCloseMode == OpenCloseMode.CloseOnRelease)
        {
            if (shouldOpen && !isOpen)
                OpenDoor();
            else if (!shouldOpen && isOpen)
                CloseDoor();
        }
    }

    // ===================== PUBLIC CONTROL =====================

    public void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;

        if (closeCoroutine != null)
            StopCoroutine(closeCoroutine);

        StartAnimate(openPosition);

        // doorCollider.enabled = false; // bật nếu muốn player đi xuyên
    }

    public void CloseDoor()
    {
        if (!isOpen) return;

        isOpen = false;

        StartAnimate(closedPosition);

        // doorCollider.enabled = true;
    }

    // ===================== ANIMATION =====================

    private void StartAnimate(Vector3 target)
    {
        if (animateCoroutine != null)
            StopCoroutine(animateCoroutine);

        animateCoroutine = StartCoroutine(AnimateDoor(target));
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
